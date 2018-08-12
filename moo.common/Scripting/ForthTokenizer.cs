using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using static ForthDatum;
using static ForthVariable;

public static class ForthTokenizer
{
    private enum ForwardOperation
    {
        None,
        ReadingString,
        ReadingWordName,
        SkipUntilEndComment,
        SkipUntilNonWhitespace,
        SkipUntilWhitespace,
        SkipUntilNonLineBreak
    }


    public static async Task<ForthTokenizerResult> Tokenzie(PlayerConnection connection, string program, Dictionary<string, ForthVariable> programLocalVariables = null)
    {
        var defines = new Dictionary<string, string>();
        var words = new List<ForthWord>();
        if (programLocalVariables == null)
            programLocalVariables = new Dictionary<string, ForthVariable>();

        var lineNumber = 1;
        var columnNumber = 0;
        var forwardOperation = ForwardOperation.None;
        var linebreakCharacters = new char[] { '\r', '\n' };
        var whitespaceCharacters = new char[] { ' ', '\t' };
        var currentWordName = new StringBuilder();
        List<ForthDatum> currentWordData = null;
        var currentDatum = new StringBuilder();
        var currentNonDatum = new StringBuilder();
        var verbosity = 1;

        foreach (var c in program)
        {
            columnNumber++;

            switch (forwardOperation)
            {
                case ForwardOperation.ReadingString:
                    if ((currentDatum.Length > 0 && currentDatum[currentDatum.Length - 1] == '\\' && c == '\"') || c != '\"')
                    {
                        currentDatum.Append(c);
                        continue;
                    }
                    else
                    {
                        if (verbosity >= 2)
                            await connection.sendOutput($"STRING({lineNumber},{columnNumber - currentDatum.ToString().Length - 1}): \"{currentDatum.ToString()}\"");
                        currentWordData.Add(new ForthDatum(currentDatum.ToString(), DatumType.String, lineNumber, columnNumber - currentDatum.ToString().Length - 1));
                        currentDatum.Clear();
                        forwardOperation = ForwardOperation.None;
                        continue;
                    }
                case ForwardOperation.ReadingWordName:
                    if (whitespaceCharacters.Contains(c) && currentWordName.Length == 0)
                        continue;
                    else if (!whitespaceCharacters.Contains(c) && !linebreakCharacters.Contains(c))
                    {
                        currentWordName.Append(c);
                        continue;
                    }
                    else
                    {
                        if (verbosity >= 1)
                            await connection.sendOutput($"WORD({lineNumber},{columnNumber - currentWordName.ToString().Length}): {currentWordName.ToString()}");
                        forwardOperation = ForwardOperation.None;
                        if (linebreakCharacters.Contains(c))
                        {
                            lineNumber++;
                            columnNumber = 0;
                        }
                        continue;
                    }
                case ForwardOperation.SkipUntilEndComment:
                    if (c == ')')
                    {
                        forwardOperation = ForwardOperation.None;
                        continue;
                    }
                    else
                    {
                        if (linebreakCharacters.Contains(c))
                        {
                            lineNumber++;
                            columnNumber = 0;
                        }
                        continue;
                    }
                case ForwardOperation.SkipUntilNonLineBreak:
                    if (linebreakCharacters.Contains(c))
                    {
                        lineNumber++;
                        columnNumber = 0;
                        continue;
                    }
                    else
                        forwardOperation = ForwardOperation.None;
                    break;
                case ForwardOperation.SkipUntilNonWhitespace:
                    if (whitespaceCharacters.Contains(c))
                        continue;
                    else
                        forwardOperation = ForwardOperation.None;
                    break;
            }

            if (c == '(')
            {
                forwardOperation = ForwardOperation.SkipUntilEndComment;
                continue;
            }

            if (linebreakCharacters.Contains(c))
            {
                // Handle word datum
                if (currentWordName.Length > 0 && currentDatum.Length > 0)
                {
                    if (verbosity > 3)
                        await connection.sendOutput($"DATUM({lineNumber},{columnNumber - currentDatum.ToString().Length}): {currentDatum.ToString()}");
                    if (ForthWord.GetPrimatives().Contains(currentDatum.ToString()))
                    {
                        if (currentDatum.ToString().Length == 1 && (new[] { '@', '!' }.Contains(currentDatum.ToString()[0])))
                        {
                            var last = currentWordData.Last();
                            currentWordData.Remove(last);
                            last.Type = DatumType.Variable;
                            currentWordData.Add(last);
                        }

                        currentWordData.Add(new ForthDatum(currentDatum.ToString(), DatumType.Primitive, lineNumber, columnNumber - currentDatum.ToString().Length));
                    }
                    else if (ForthDatum.TryInferType(currentDatum.ToString(), out Tuple<DatumType, object> result))
                        currentWordData.Add(new ForthDatum(result.Item2, result.Item1, lineNumber, columnNumber - currentDatum.ToString().Length));
                    else
                        currentWordData.Add(new ForthDatum(currentDatum.ToString(), DatumType.Unknown, lineNumber, columnNumber - currentDatum.ToString().Length));
                    currentDatum.Clear();
                }
                else
                {
                    // Handle out-of-word primative
                    var directive = currentNonDatum.ToString().Trim();
                    if (directive.Length > 0)
                    {
                        // LVAR
                        {
                            var lvarMatch = Regex.Match(directive, @"(?:lvar\s+(?<lvar>\w{1,20}))");
                            if (lvarMatch.Success)
                                programLocalVariables.Add(lvarMatch.Groups["lvar"].Value, default(ForthVariable));
                        }

                        if (verbosity > 3)
                            await connection.sendOutput($"PRE({lineNumber},{columnNumber - currentNonDatum.ToString().Length}): {currentNonDatum.ToString()}");
                    }
                    currentNonDatum.Clear();
                }

                forwardOperation = ForwardOperation.SkipUntilNonLineBreak;

                lineNumber++;
                columnNumber = 0;
                continue;
            }

            if (c == '\"' && currentWordName.Length > 0)
            {
                forwardOperation = ForwardOperation.ReadingString;
                continue;
            }

            if (whitespaceCharacters.Contains(c))
            {
                if (currentWordName.Length > 0 && currentDatum.Length > 0)
                {
                    if (verbosity > 3)
                        await connection.sendOutput($"DATUM({lineNumber},{columnNumber - currentDatum.ToString().Length}): {currentDatum.ToString()}");

                    if (ForthWord.GetPrimatives().Contains(currentDatum.ToString()))
                    {
                        if (currentDatum.ToString().Length == 1 && (new[] { '@', '!' }.Contains(currentDatum.ToString()[0])))
                        {
                            var last = currentWordData.Last();
                            currentWordData.Remove(last);
                            last.Type = DatumType.Variable;
                            currentWordData.Add(last);
                        }
                        currentWordData.Add(new ForthDatum(currentDatum.ToString(), DatumType.Primitive, lineNumber, columnNumber - currentDatum.ToString().Length));
                    }
                    else if (ForthDatum.TryInferType(currentDatum.ToString(), out Tuple<DatumType, object> result))
                        currentWordData.Add(new ForthDatum(result.Item2, result.Item1, lineNumber, columnNumber - currentDatum.ToString().Length));
                    else
                        currentWordData.Add(new ForthDatum(currentDatum.ToString(), DatumType.Unknown, lineNumber, columnNumber - currentDatum.ToString().Length));
                    currentDatum.Clear();
                    forwardOperation = ForwardOperation.SkipUntilNonWhitespace;
                }
                else
                {
                    currentNonDatum.Append(c);
                }
                continue;
            }

            if (whitespaceCharacters.Contains(c) && currentDatum.Length == 0)
                continue;
            /*UNNECESSARY if (linebreakCharacters.Contains(c))
            {
                if (currentWordName.Length == 0)
                {
                    await connection.sendOutput($"PREX({lineNumber},{columnNumber - currentNonDatum.ToString().Length}): {currentNonDatum.ToString()}");
                    currentNonDatum.Clear();
                }

                lineNumber++;
                columnNumber = 0;
                                            preparserLine = false;
                continue;
            }*/

            if (c == ':' && currentWordName.Length == 0)
            {
                currentWordData = new List<ForthDatum>();
                forwardOperation = ForwardOperation.ReadingWordName;
                continue;
            }

            if (c == ';' && currentWordName.Length > 0)
            {
                words.Add(new ForthWord(currentWordName.ToString(), currentWordData));
                currentWordName.Clear();
                currentWordData = null;
            }

            if (currentWordName.Length > 0)
                currentDatum.Append(c);
            else if (!linebreakCharacters.Contains(c))
                currentNonDatum.Append(c);
        }

        if (verbosity > 3)
        {
            await connection.sendOutput($"Words: ({words.Count})");
            foreach (var word in words)
                await connection.sendOutput($" {word.name}");
        }

        if (words.Count == 0)
            return new ForthTokenizerResult("No Forth words could be found");

        return new ForthTokenizerResult(words, programLocalVariables);
    }

}
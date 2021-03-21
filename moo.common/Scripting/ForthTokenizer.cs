using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using moo.common.Connections;
using static moo.common.Scripting.ForthDatum;

namespace moo.common.Scripting
{
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
            SkipUntilNonLineBreak,
            ReadingWordArgList,
        }


        public static async Task<ForthTokenizerResult> Tokenzie(PlayerConnection? connection, string program, Dictionary<string, ForthVariable>? programLocalVariables = null)
        {
            var defines = new Dictionary<string, string>();
            var words = new List<ForthWord>();
            if (programLocalVariables == null)
                programLocalVariables = new Dictionary<string, ForthVariable>();

            var lineNumber = 1;
            var columnNumber = 0;
            var forwardOperation = ForwardOperation.None;
            var linebreakCharacters = new char[] { '\n' };
            var whitespaceCharacters = new char[] { ' ', '\t' };
            var currentWordNameBuilder = new StringBuilder();
            string? currentWordName = null;
            var currentWordArgListBuilder = new StringBuilder();
            int? currentWordLineNumber = null;
            List<ForthDatum>? currentWordData = null;
            var currentDatum = new StringBuilder();
            var currentNonDatum = new StringBuilder();
            var verbosity = 0;

            foreach (var c in program)
            {
                if (c == '\r')
                    continue; // Ignore all DOS returns.

                columnNumber++;

                switch (forwardOperation)
                {
                    case ForwardOperation.ReadingString:
                        if ((currentDatum.Length > 0 && currentDatum[^1] == '\\' && c == '\"') || c != '\"')
                        {
                            //if (program.Contains("@archive"))
                            //    Console.Write(c);
                            currentDatum.Append(c);
                            continue;
                        }
                        else
                        {
                            //if (verbosity >= 2 && program.Contains("@archive"))// && connection != null)
                            //    Console.WriteLine($"STRING({lineNumber},{columnNumber - currentDatum.ToString().Length - 1}): \"{currentDatum}\"");
                            currentWordData.Add(new ForthDatum(currentDatum.ToString(), DatumType.String, lineNumber, columnNumber - currentDatum.ToString().Length - 1, currentWordName, currentWordLineNumber));
                            //if (program.Contains("@archive"))
                            //    Console.WriteLine($"Output String: {currentDatum}");
                            currentDatum.Clear();
                            forwardOperation = ForwardOperation.None;
                            continue;
                        }
                    case ForwardOperation.ReadingWordArgList:
                        currentWordArgListBuilder.Append(c);
                        if (c == ']')
                            forwardOperation = ForwardOperation.None;
                        continue;
                    case ForwardOperation.ReadingWordName:
                        if (whitespaceCharacters.Contains(c) && currentWordNameBuilder.Length == 0)
                            continue;
                        else if (!whitespaceCharacters.Contains(c)
                            && !linebreakCharacters.Contains(c)
                            && c != '[')
                        {
                            currentWordNameBuilder.Append(c);
                            continue;
                        }
                        else
                        {
                            currentWordName = currentWordNameBuilder.ToString();
                            currentWordLineNumber = 0;
                            if (verbosity >= 1)
                            {
                                if (connection != null)
                                    await connection.SendOutput($"WORD({lineNumber},{columnNumber - currentWordName.Length}): {currentWordName}");
                                else
                                    Console.WriteLine($"WORD({lineNumber},{columnNumber - currentWordName.Length}): {currentWordName}");
                            }

                            if (c == '[')
                            {
                                currentWordArgListBuilder.Clear();
                                currentWordArgListBuilder.Append(c);
                                forwardOperation = ForwardOperation.ReadingWordArgList;
                            }
                            else
                                forwardOperation = ForwardOperation.None;

                            if (linebreakCharacters.Contains(c))
                            {
                                lineNumber++;
                                if (currentWordName != null) currentWordLineNumber++;
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
                                if (currentWordName != null) currentWordLineNumber++;
                                columnNumber = 0;
                            }
                            continue;
                        }
                    case ForwardOperation.SkipUntilNonLineBreak:
                        if (linebreakCharacters.Contains(c))
                        {
                            lineNumber++;
                            if (currentWordName != null) currentWordLineNumber++;
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
                    if (currentWordNameBuilder.Length > 0 && currentDatum.Length > 0 && currentWordData != null)
                    {
                        if (verbosity > 3)
                        {
                            if (connection != null)
                                await connection.SendOutput($"DATUM({lineNumber},{columnNumber - currentDatum.ToString().Length}): {currentDatum}");
                            else
                                Console.WriteLine($"DATUM({lineNumber},{columnNumber - currentDatum.ToString().Length}): {currentDatum}");
                        }

                        if (ForthWord.GetPrimatives().Contains(currentDatum.ToString()))
                        {
                            if (currentDatum.ToString().Length == 1 &&
                                (new[] { '@', '!' }.Contains(currentDatum.ToString()[0])))
                            {
                                var idx = currentWordData.Count - 1;
                                var last = currentWordData[idx];
                                currentWordData.RemoveAt(idx);
                                last.Type = DatumType.Variable;
                                currentWordData.Insert(idx, last);
                            }

                            currentWordData.Add(new ForthDatum(currentDatum.ToString(), DatumType.Primitive, lineNumber, columnNumber - currentDatum.ToString().Length, currentWordName, currentWordLineNumber));
                        }
                        else if (TryInferType(currentDatum.ToString(), out Tuple<DatumType, object>? result))
                            currentWordData.Add(new ForthDatum(result.Item2, result.Item1, lineNumber, columnNumber - currentDatum.ToString().Length, currentWordName, currentWordLineNumber));
                        else
                            currentWordData.Add(new ForthDatum(currentDatum.ToString(), DatumType.Unknown, lineNumber, columnNumber - currentDatum.ToString().Length, currentWordName, currentWordLineNumber));
                        currentDatum.Clear();
                    }
                    else
                    {
                        // Handle out-of-word primative
                        var directive = currentNonDatum.ToString().Trim();
                        if (directive.Length > 5)
                        {
                            // LVAR
                            {
                                var lvarMatch = Regex.Match(directive, @"(?:lvar\s+(?<lvar>\w{1,20}))", RegexOptions.Compiled | RegexOptions.IgnoreCase);
                                if (lvarMatch.Success)
                                    programLocalVariables.Add(lvarMatch.Groups["lvar"].Value, default);
                            }

                            if (verbosity > 3 && connection != null)
                                await connection.SendOutput($"PRE({lineNumber},{columnNumber - currentNonDatum.ToString().Length}): {currentNonDatum}");
                        }
                        currentNonDatum.Clear();
                    }

                    forwardOperation = ForwardOperation.SkipUntilNonLineBreak;

                    lineNumber++;
                    if (currentWordName != null) currentWordLineNumber++;
                    columnNumber = 0;
                    continue;
                }

                if (c == '\"' && currentWordNameBuilder.Length > 0)
                {
                    forwardOperation = ForwardOperation.ReadingString;
                    //if (program.Contains("@archive"))
                    //    Console.WriteLine($"Started reading string on line: {lineNumber}");
                    continue;
                }

                if (whitespaceCharacters.Contains(c))
                {
                    if (currentWordNameBuilder.Length > 0 && currentDatum.Length > 0)
                    {
                        if (verbosity > 3 && connection != null)
                            await connection.SendOutput($"DATUM({lineNumber},{columnNumber - currentDatum.ToString().Length}): {currentDatum}");

                        if (ForthWord.GetPrimatives().Contains(currentDatum.ToString()))
                        {
                            if (currentDatum.ToString().Length == 1 && (new[] { '@', '!' }.Contains(currentDatum.ToString()[0])))
                            {
                                var idx = currentWordData.Count - 1;
                                var last = currentWordData[idx];
                                currentWordData.RemoveAt(idx);
                                last.Type = DatumType.Variable;
                                currentWordData.Insert(idx, last);
                            }
                            currentWordData.Add(new ForthDatum(currentDatum.ToString(), DatumType.Primitive, lineNumber, columnNumber - currentDatum.ToString().Length, currentWordName, currentWordLineNumber));
                        }
                        else if (TryInferType(currentDatum.ToString(), out Tuple<DatumType, object>? result))
                            currentWordData.Add(new ForthDatum(result.Item2, result.Item1, lineNumber, columnNumber - currentDatum.ToString().Length, currentWordName, currentWordLineNumber));
                        else
                            currentWordData.Add(new ForthDatum(currentDatum.ToString(), DatumType.Unknown, lineNumber, columnNumber - currentDatum.ToString().Length, currentWordName, currentWordLineNumber));
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
                        await connection.sendOutput($"PREX({lineNumber},{columnNumber - currentNonDatum.ToString().Length}): {currentNonDatum}");
                        currentNonDatum.Clear();
                    }

                    lineNumber++;
                                    if (currentWordName != null) currentWordLineNumber++;
                    columnNumber = 0;
                                                preparserLine = false;
                    continue;
                }*/

                if (c == ':' && currentWordNameBuilder.Length == 0)
                {
                    currentWordData = new List<ForthDatum>();
                    forwardOperation = ForwardOperation.ReadingWordName;
                    continue;
                }

                if (c == ';' && currentWordNameBuilder.Length > 0)
                {
                    var wordName = currentWordNameBuilder.ToString();

                    // Parse input list
                    List<(string type, string name)> inputs = new();
                    if (currentWordArgListBuilder.Length > 0)
                    {
                        var argList = currentWordArgListBuilder.ToString();
                        var argMatches = Regex.Match(argList, @"^\[(\s*(?<input>\w+:\w+))+\s*(--|]$)", RegexOptions.Compiled);
                        if (argMatches.Success)
                        {
                            foreach (Capture cap in argMatches.Groups["input"].Captures)
                            {
                                var type = cap.Value.Split(':')[0];
                                var name = cap.Value.Split(':')[1];
                                inputs.Add((type, name));
                            }
                        }
                    }

                    words.Add(new ForthWord(wordName, currentWordData, inputs));

                    // Clean up for next word
                    currentWordName = null;
                    currentWordNameBuilder.Clear();
                    currentWordArgListBuilder.Clear();
                    currentWordLineNumber = 0;
                    currentWordData = null;
                }

                if (currentWordNameBuilder.Length > 0)
                    currentDatum.Append(c);
                else if (!linebreakCharacters.Contains(c))
                    currentNonDatum.Append(c);
            }

            if (verbosity > 3 && connection != null)
            {
                await connection.SendOutput($"Words: ({words.Count})");
                foreach (var word in words)
                    await connection.SendOutput($" {word.name}");
            }

            if (words.Count == 0)
                return new ForthTokenizerResult("No Forth words could be found");

            return new ForthTokenizerResult(words, programLocalVariables);
        }
    }
}
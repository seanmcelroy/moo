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
    public static ForthTokenizerResult TokenizeOLD(string program)
    {
        // Parse the program onto the stack
        var lines = program.Split(new string[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
        Console.Out.WriteLine($"Parsed {lines.Length} lines");

        var regexWordParsing = new Regex(@"(?:\([^\)]*\)|(?:\$def\s+(?<defName>\w{1,20})\s+(?:(?:""(?<string>[^""]*)"")|(?<float>\-?(?:\d+\.\d*|\d*\.\d+))|(?<int>\-?\d+)|(?<dbref>#\-?\d+)))|(?:lvar\s+(?<lvar>\w{1,20}))|(?<word>\:\s*(?<wordName>[\w\-]+)\s*(?<wordBody>[^;]+)\;))");
        var regexDatumParsing = new Regex(@"(?:\([^\)]*\)|(?:""(?<string>[^""]*)"")|(?<float>\-?(?:\d+\.\d*|\d*\.\d+))|(?<int>\-?\d+)|(?<dbref>#\-?\d+)|(?<prim>[\w\.\-\+\*\/%\?!><=@;:{}]+))", RegexOptions.Compiled);

        var words = new List<ForthWord>();
        var programLocalVariables = new Dictionary<string, ForthVariable>();

        //int lineRatchet = 0;
        foreach (System.Text.RegularExpressions.Match wordMatch in regexWordParsing.Matches(program))
        {
            /*while (lineRatchet < lines.Length) {
                if (lines[lineRatchet].IndexOf(wordMatch.Value) > -1)
                    break;
                lineRatchet++;
            }*/

            if (!string.IsNullOrWhiteSpace(wordMatch.Groups["defName"].Value))
            {
                var defName = wordMatch.Groups["defName"].Value.ToLowerInvariant();
                ForthVariable defValue;

                if (!string.IsNullOrWhiteSpace(wordMatch.Groups["string"].Value))
                {
                    defValue = new ForthVariable(wordMatch.Groups["string"].Value, VariableType.String, true);
                }
                else if (!string.IsNullOrWhiteSpace(wordMatch.Groups["float"].Value))
                {
                    defValue = new ForthVariable(float.Parse(wordMatch.Groups["float"].Value), VariableType.Float, true);
                }
                else if (!string.IsNullOrWhiteSpace(wordMatch.Groups["int"].Value))
                {
                    defValue = new ForthVariable(int.Parse(wordMatch.Groups["int"].Value), VariableType.Integer, true);
                }
                else if (!string.IsNullOrWhiteSpace(wordMatch.Groups["dbref"].Value))
                {
                    defValue = new ForthVariable(new Dbref(wordMatch.Groups["dbref"].Value), VariableType.DbRef, true);
                }
                else
                    return new ForthTokenizerResult($"Unable to parse line in $def program at index {wordMatch.Index}");

                programLocalVariables.Add(defName, defValue);
            }

            if (!string.IsNullOrWhiteSpace(wordMatch.Groups["lvar"].Value))
            {
                programLocalVariables.Add(wordMatch.Groups["lvar"].Value, default(ForthVariable));
            }

            if (!string.IsNullOrWhiteSpace(wordMatch.Groups["word"].Value))
            {
                // Cut the word up and preserve line numbers to aid in debugging words
                var wordName = wordMatch.Groups["wordName"].Value;
                var wordBody = wordMatch.Groups["wordBody"].Value;

                var wordBodySplit = wordBody.Split(new string[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
                var programData = new List<ForthDatum>();
                for (int i = 0; i < wordBodySplit.Length; i++)
                {
                    var matches = regexDatumParsing.Matches(wordBodySplit[i]);
                    foreach (System.Text.RegularExpressions.Match match in matches)
                    {
                        foreach (var group in match.Groups.Skip(1).Where(g => g.Success))
                        {
                            switch (group.Name)
                            {
                                case "string":
                                    {
                                        programData.Add(new ForthDatum(group.Value, i));
                                        continue;
                                    }
                                case "float":
                                    {
                                        programData.Add(new ForthDatum(float.Parse(group.Value), i));
                                        continue;
                                    }
                                case "int":
                                    {
                                        programData.Add(new ForthDatum(int.Parse(group.Value), i));
                                        continue;
                                    }
                                case "dbref":
                                    {
                                        programData.Add(new ForthDatum(new Dbref(group.Value), 0, i));
                                        continue;
                                    }
                                case "prim":
                                    {
                                        if (ForthWord.GetPrimatives().Any(s => string.Compare(s, group.Value, true) == 0))
                                            programData.Add(new ForthDatum(group.Value, ForthDatum.DatumType.Primitive, i));
                                        else // Could be a variable name
                                            programData.Add(new ForthDatum(group.Value, ForthDatum.DatumType.Unknown, i));
                                        continue;
                                    }
                            }

                            return new ForthTokenizerResult($"Unable to parse line in word {wordName}: {match.Value}.  Full line: {wordBodySplit[i]}");
                        }

                    }
                }
                var word = new ForthWord(wordName, programData);
                words.Add(word);
                continue;
            }
        }

        return new ForthTokenizerResult(words, programLocalVariables);
    }

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


    public static ForthTokenizerResult Tokenzie(PlayerConnection connection, string program)
    {
        var defines = new Dictionary<string, string>();
        var words = new List<ForthWord>();
        var programLocalVariables = new Dictionary<string, ForthVariable>();

        var lineNumber = 1;
        var columnNumber = 0;
        var preparserLine = false;
        var forwardOperation = ForwardOperation.None;
        var linebreakCharacters = new char[] { '\r', '\n' };
        var whitespaceCharacters = new char[] { ' ', '\t' };
        var currentWordName = new StringBuilder();
        List<ForthDatum> currentWordData = null;
        var currentDatum = new StringBuilder();
        var currentNonDatum = new StringBuilder();

        foreach (var c in program)
        {
            columnNumber++;
            if (columnNumber == 1 && c == '$')
                preparserLine = true;

            switch (forwardOperation)
            {
                case ForwardOperation.ReadingString:
                    if (c != '\"')
                    {
                        currentDatum.Append(c);
                        continue;
                    }
                    else
                    {
                        Console.WriteLine($"STRING({lineNumber},{columnNumber - currentDatum.ToString().Length - 1}): \"{currentDatum.ToString()}\"");
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
                        Console.WriteLine($"WORD({lineNumber},{columnNumber - currentWordName.ToString().Length}): {currentWordName.ToString()}");
                        forwardOperation = ForwardOperation.None;
                        if (linebreakCharacters.Contains(c))
                        {
                            lineNumber++;
                            columnNumber = 0;
                            preparserLine = false;
                        }
                        continue;
                    }
                case ForwardOperation.SkipUntilEndComment:
                    if (c != ')')
                    {
                        if (linebreakCharacters.Contains(c))
                        {
                            lineNumber++;
                            columnNumber = 0;
                                                        preparserLine = false;
                        }
                        continue;
                    }
                    else
                    {
                        forwardOperation = ForwardOperation.None;
                        continue;
                    }
                case ForwardOperation.SkipUntilNonLineBreak:
                    if (linebreakCharacters.Contains(c))
                    {
                        lineNumber++;
                        columnNumber = 0;
                                                    preparserLine = false;
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

            if (c == '(' && !preparserLine)
            {
                forwardOperation = ForwardOperation.SkipUntilEndComment;
                continue;
            }

            if (linebreakCharacters.Contains(c))
            {
                // Handle word datum
                if (currentWordName.Length > 0 && currentDatum.Length > 0 && !preparserLine)
                {
                    Console.WriteLine($"DATUM({lineNumber},{columnNumber - currentDatum.ToString().Length}): {currentDatum.ToString()}");
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
                        var done = false;
                        // LVAR
                        {
                            var lvarMatch = Regex.Match(directive, @"(?:lvar\s+(?<lvar>\w{1,20}))");
                            if (lvarMatch.Success)
                            {
                                programLocalVariables.Add(lvarMatch.Groups["lvar"].Value, default(ForthVariable));
                                done = true;
                            }
                        }

                        // $def
                        if (!done)
                        {
                            var defMatch = Regex.Match(directive, @"(?:\$def\s+(?<defName>\w{1,20})\s+(?<defValue>[^\(]+))");
                            if (defMatch.Success)
                            {
                                if (ForthVariable.TryInferType(defMatch.Groups["defValue"].Value, out Tuple<VariableType, object> result))
                                {
                                    programLocalVariables.Add(defMatch.Groups["defName"].Value, new ForthVariable(result.Item2, result.Item1, true));
                                    done = true;
                                }
                                else
                                    programLocalVariables.Add(defMatch.Groups["defName"].Value, new ForthVariable(defMatch.Groups["defValue"].Value, VariableType.String, true));
                            }
                        }

                        // $echo
                        if (!done)
                        {
                            var echoMatch = Regex.Match(directive, @"(?:\$echo\s+\""(?<value>[^\""]*)\"")");
                            if (echoMatch.Success)
                            {
                                connection.sendOutput(echoMatch.Groups["value"].Value);
                                done = true;
                            }
                        }

                        Console.WriteLine($"PRE({lineNumber},{columnNumber - currentNonDatum.ToString().Length}): {currentNonDatum.ToString()}");
                    }
                    currentNonDatum.Clear();
                }

                forwardOperation = ForwardOperation.SkipUntilNonLineBreak;

                lineNumber++;
                columnNumber = 0;
                                            preparserLine = false;
                continue;
            }

            if (c == '\"' && currentWordName.Length > 0 && !preparserLine)
            {
                forwardOperation = ForwardOperation.ReadingString;
                continue;
            }

            if (whitespaceCharacters.Contains(c))
            {
                if (currentWordName.Length > 0 && currentDatum.Length > 0 && !preparserLine)
                {
                    Console.WriteLine($"DATUM({lineNumber},{columnNumber - currentDatum.ToString().Length}): {currentDatum.ToString()}");
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
                    Console.WriteLine($"PREX({lineNumber},{columnNumber - currentNonDatum.ToString().Length}): {currentNonDatum.ToString()}");
                    currentNonDatum.Clear();
                }

                lineNumber++;
                columnNumber = 0;
                                            preparserLine = false;
                continue;
            }*/

            if (c == ':' && currentWordName.Length == 0&& !preparserLine)
            {
                currentWordData = new List<ForthDatum>();
                forwardOperation = ForwardOperation.ReadingWordName;
                continue;
            }

            if (c == ';' && currentWordName.Length > 0&& !preparserLine)
            {
                words.Add(new ForthWord(currentWordName.ToString(), currentWordData));
                currentWordName.Clear();
                currentWordData = null;
            }

            if (currentWordName.Length > 0&& !preparserLine)
                currentDatum.Append(c);
            else if (!linebreakCharacters.Contains(c))
                currentNonDatum.Append(c);
        }

        return new ForthTokenizerResult(words, programLocalVariables);
    }

}
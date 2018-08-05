using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using static ForthVariable;

public static class ForthParser
{
    public static ForthParseResult Parse(string program)
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
                    return new ForthParseResult($"Unable to parse line in $def program at index {wordMatch.Index}");

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

                            return new ForthParseResult($"Unable to parse line in word {wordName}: {match.Value}.  Full line: {wordBodySplit[i]}");
                        }

                    }
                }
                var word = new ForthWord(wordName, programData);
                words.Add(word);
                continue;
            }
        }

        return new ForthParseResult(words, programLocalVariables);
    }
}
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

public class ForthInterpreter
{
    private static readonly ConcurrentBag<ForthProcess> processes = new ConcurrentBag<ForthProcess>();

    private readonly string program;
    private readonly List<ForthWord> words = new List<ForthWord>();
    private readonly List<string> programLocalVariableDeclarations = new List<string>();

    public ForthInterpreter(string program)
    {
        this.program = program;
    }

    private void Parse()
    {
        // Parse the program onto the stack
        var lines = program.Split(new string[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);
        Console.Out.WriteLine($"Parsed {lines.Length} lines");

        var regexWordParsing = new Regex(@"(?:(?<comment>\([^\)]*\))|(?:lvar\s+(?<lvar>\w+))|(?<word>\:\s*(?<wordName>[\w\-]+)\s*(?<wordBody>[^;]+)\;))");
        var regexDatumParsing = new Regex(@"(?:(?<comment>\([^\)]*\))|(?:""(?<string>[^""]*)"")|(?<float>\-?(?:\d+\.\d*|\d*\.\d+))|(?<int>\-?\d+)|(?<dbref>#\d+)|(?<prim>[\w\.\-\+\*\/%\?!><=@;:{}]+))", RegexOptions.Compiled);

        foreach (Match wordMatch in regexWordParsing.Matches(program))
        {

            if (!string.IsNullOrWhiteSpace(wordMatch.Groups["comment"].Value))
                continue;

            if (!string.IsNullOrWhiteSpace(wordMatch.Groups["lvar"].Value))
            {
                programLocalVariableDeclarations.Add(wordMatch.Groups["lvar"].Value);
            }

            if (!string.IsNullOrWhiteSpace(wordMatch.Groups["word"].Value))
            {
                // Cut the word up and preserve line numbers to aid in debugging words
                var wordName = wordMatch.Groups["wordName"].Value;
                var wordBody = wordMatch.Groups["wordBody"].Value;

                var wordBodySplit = wordBody.Split(new string[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
                var programLineNumbersAndDatum = new Dictionary<int, ForthDatum[]>();
                for (int i = 0; i < wordBodySplit.Length; i++)
                {
                    var lineData = new List<ForthDatum>();
                    var matches = regexDatumParsing.Matches(wordBodySplit[i]);
                    foreach (Match match in matches)
                    {
                        if (!string.IsNullOrWhiteSpace(match.Groups["comment"].Value))
                            continue;

                        if (!string.IsNullOrWhiteSpace(match.Groups["string"].Value) || wordBodySplit[i].Trim() == "\"\"")
                        {
                            lineData.Add(new ForthDatum(match.Groups["string"].Value, ForthDatum.DatumType.String));
                            continue;
                        }

                        if (!string.IsNullOrWhiteSpace(match.Groups["float"].Value))
                        {
                            lineData.Add(new ForthDatum(float.Parse(match.Groups["float"].Value), ForthDatum.DatumType.Float));
                            continue;
                        }

                        if (!string.IsNullOrWhiteSpace(match.Groups["int"].Value))
                        {
                            lineData.Add(new ForthDatum(int.Parse(match.Groups["int"].Value), ForthDatum.DatumType.Integer));
                            continue;
                        }

                        if (!string.IsNullOrWhiteSpace(match.Groups["dbref"].Value))
                        {
                            lineData.Add(new ForthDatum(match.Groups["dbref"].Value, ForthDatum.DatumType.DbRef));
                            continue;
                        }

                        if (!string.IsNullOrWhiteSpace(match.Groups["prim"].Value))
                        {
                            if (ForthWord.GetPrimatives().Any(s => string.Compare(s, match.Groups["prim"].Value, true) == 0))
                                lineData.Add(new ForthDatum(match.Groups["prim"].Value, ForthDatum.DatumType.Primitive));
                            else // Could be a variable name
                                lineData.Add(new ForthDatum(match.Groups["prim"].Value, ForthDatum.DatumType.Unknown));
                            continue;
                        }

                        throw new System.InvalidOperationException($"Unable to parse line in word {wordName}: {match.Value}.  Full line: {wordBodySplit[i]}");
                    }

                    programLineNumbersAndDatum.Add(i + 1, lineData.ToArray());
                }
                var word = new ForthWord(wordName, programLineNumbersAndDatum);
                words.Add(word);
                continue;
            }
        }
        Console.Out.WriteLine($"Parsed {programLocalVariableDeclarations.Count} program local variables and {words.Count} words");
    }

    public async Task<ForthProgramResult> SpawnAsync(
        Dbref scriptId,
        Player player,
        Dbref trigger,
        string command,
        object[] args,
        CancellationToken cancellationToken)
    {
        if (words.Count == 0)
            Parse();

        var process = new ForthProcess(scriptId, words, player);
        foreach (var lvar in programLocalVariableDeclarations)
            process.SetProgramLocalVariable(lvar, null);
        processes.Add(process);

        var programResult = await process.RunAsync(trigger, command, args, cancellationToken);
        return programResult;
    }
}
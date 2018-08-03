using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

public class ForthInterpreter
{
    private readonly Server server;
    private readonly string program;
    private readonly List<ForthWord> words = new List<ForthWord>();
    private readonly List<string> programLocalVariableDeclarations = new List<string>();

    public ForthInterpreter(Server server, string program)
    {
        this.server = server;
        this.program = program;
    }

    private ForthParseResult Parse()
    {
        // Parse the program onto the stack
        var lines = program.Split(new string[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
        Console.Out.WriteLine($"Parsed {lines.Length} lines");

        var regexWordParsing = new Regex(@"(?:\([^\)]*\)|(?:lvar\s+(?<lvar>\w+))|(?<word>\:\s*(?<wordName>[\w\-]+)\s*(?<wordBody>[^;]+)\;))");
        var regexDatumParsing = new Regex(@"(?:\([^\)]*\)|(?:""(?<string>[^""]*)"")|(?<float>\-?(?:\d+\.\d*|\d*\.\d+))|(?<int>\-?\d+)|(?<dbref>#\-?\d+)|(?<prim>[\w\.\-\+\*\/%\?!><=@;:{}]+))", RegexOptions.Compiled);

        //int lineRatchet = 0;
        foreach (System.Text.RegularExpressions.Match wordMatch in regexWordParsing.Matches(program))
        {
            /*while (lineRatchet < lines.Length) {
                if (lines[lineRatchet].IndexOf(wordMatch.Value) > -1)
                    break;
                lineRatchet++;
            }*/

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

                            return new ForthParseResult(false, $"Unable to parse line in word {wordName}: {match.Value}.  Full line: {wordBodySplit[i]}");
                        }

                    }
                }
                var word = new ForthWord((d, s) => server.Notify(d, s), wordName, programData);
                words.Add(word);
                continue;
            }
        }

        return new ForthParseResult(true, $"Parsed {programLocalVariableDeclarations.Count} program local variables and {words.Count} words");
    }

    public async Task<ForthProgramResult> SpawnAsync(
        Dbref scriptId,
        PlayerConnection connection,
        Dbref trigger,
        string command,
        object[] args,
        CancellationToken cancellationToken)
    {
        if (words.Count == 0)
        {
            var result = Parse();

            if (!result.isSuccessful)
                return new ForthProgramResult(ForthProgramResult.ForthProgramErrorResult.SYNTAX_ERROR, result.reason);
        }

        var process = new ForthProcess(server, scriptId, words, connection);
        foreach (var lvar in programLocalVariableDeclarations)
            process.SetProgramLocalVariable(lvar, null);

        return await server.ExecuteAsync(process, trigger, command, args, cancellationToken);
    }
}
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

public class ForthInterpreter
{
    private readonly string program;
    private readonly List<List<ForthDatum>> programLines = new List<List<ForthDatum>>();

    private readonly ConcurrentBag<ForthExecutionContext> tasks = new ConcurrentBag<ForthExecutionContext>();

    public ForthInterpreter(string program)
    {
        this.program = program;
    }

    private void Parse()
    {
        // Parse the program onto the stack
        var lines = program.Split(new string[] { "\r\n", "\r" }, StringSplitOptions.RemoveEmptyEntries);

        var regex = new Regex(@"(?:(?<comment>\([^\)]*\))|(?<string>""[^""\r\n]*"")|(?<int>\-?\d+)|(?<prim>[\w\.\-\+\?!><=@;:{}]+))", RegexOptions.Compiled);
        foreach (var line in lines)
        {
            var lineData = new List<ForthDatum>();
            var matches = regex.Matches(line);

            foreach (Match match in matches)
            {
                if (!string.IsNullOrWhiteSpace(match.Groups["comment"].Value))
                    continue;

                if (!string.IsNullOrWhiteSpace(match.Groups["string"].Value))
                {
                    lineData.Add(new ForthDatum(match.Groups["string"].Value, ForthDatum.DatumType.String));
                    continue;
                }

                if (!string.IsNullOrWhiteSpace(match.Groups["int"].Value))
                {
                    lineData.Add(new ForthDatum(int.Parse(match.Groups["int"].Value), ForthDatum.DatumType.Integer));
                    continue;
                }

                if (!string.IsNullOrWhiteSpace(match.Groups["prim"].Value))
                {
                    lineData.Add(new ForthDatum(match.Groups["prim"].Value, ForthDatum.DatumType.Primitive));
                    continue;
                }

                throw new System.InvalidOperationException("Unable to parse program line: " + match.Value);
            }

            programLines.Add(lineData);
        }
    }

    public async Task<ForthProgramResult> SpawnAsync(Player player, CancellationToken cancellationToken)
    {
        if (programLines.Count == 0)
            Parse();

        var task = new ForthExecutionContext(programLines, player);
        tasks.Add(task);

        var programResult = await task.RunAsync(null, cancellationToken);
        return programResult;
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static ForthDatum;
using static ForthProgramResult;

public class ForthExecutionContext
{
    private static readonly Dictionary<string, Func<Stack<ForthDatum>, Dictionary<string, object>, ForthProgramResult>> callTable = new Dictionary<string, Func<Stack<ForthDatum>, Dictionary<string, object>, ForthProgramResult>>();
    private readonly List<List<ForthDatum>> programLines;
    private readonly Stack<ForthDatum> stack = new Stack<ForthDatum>();

    private readonly Dictionary<string, object> variables = new Dictionary<string, object>();
    private readonly Player me;

    static ForthExecutionContext()
    {
        // Setup call table
        callTable.Add("pop", (s, v) => Pop.Execute(s));
        callTable.Add("popn", (s, v) => PopN.Execute(s));
        callTable.Add("dup", (stack, v) =>
        {
            // DUP is the same as 1 pick.
            stack.Push(new ForthDatum(1));
            return Pick.Execute(stack);
        });
        callTable.Add("dupn", (s, v) => DupN.Execute(s));
        callTable.Add("ldup", (s, v) => LDup.Execute(s));
        callTable.Add("swap", (s, v) => Swap.Execute(s));
        callTable.Add("over", (stack, v) =>
        {
            // OVER is the same as 2 pick.
            stack.Push(new ForthDatum(2));
            return Pick.Execute(stack);
        });
        callTable.Add("rot", (stack, v) =>
        {
            // ROT is the same as 3 rotate
            stack.Push(new ForthDatum(3));
            return Rotate.Execute(stack);
        });
        callTable.Add("rotate", (s, v) => Rotate.Execute(s));
        callTable.Add("pick", (s, v) => Pick.Execute(s));
        callTable.Add("put", (s, v) => Put.Execute(s));
        callTable.Add("reverse", (s, v) => Reverse.Execute(s));
        callTable.Add("lreverse", (s, v) => LReverse.Execute(s));
        callTable.Add("depth", (stack, v) =>
        {
            // DEPTH ( -- i ) 
            // Returns the number of items currently on the stack.
            stack.Push(new ForthDatum(stack.Count));
            return default(ForthProgramResult);
        });
        callTable.Add("{", (stack, v) =>
        {
            // { ( -- marker) 
            // Pushes a marker onto the stack, to be used with } or }list or }dict.
            stack.Push(new ForthDatum("{", DatumType.Marker));
            return default(ForthProgramResult);
        });
        callTable.Add("}", (s, v) => MarkerEnd.Execute(s));
        callTable.Add("@", (s, v) => At.Execute(s, v));
    }

    public ForthExecutionContext(List<List<ForthDatum>> programLines, Player me)
    {
        this.programLines = programLines;
        this.me = me;
    }

    public async Task<ForthProgramResult> RunAsync(object[] args, CancellationToken cancellationToken)
    {
        // For each line
        int lineCount = 0;
        foreach (var line in programLines)
        {
            lineCount++;

            if (cancellationToken.IsCancellationRequested)
                return new ForthProgramResult(ForthProgramErrorResult.INTERRUPTED);

            // For each element in line
            foreach (var datum in line)
            {
                // Debug, print stack
                if (stack.Count == 0)
                    await me.sendOutput($"DEBUG ({lineCount}): ()");
                else
                    await me.sendOutput($"DEBUG ({lineCount}): (" +
                    stack.Reverse().Select(s =>
                    {
                        return s.Value.ToString();
                    }).Aggregate((c, n) => c + " " + n) + ") " + datum.Value);

                // Literals
                if (datum.Type == ForthDatum.DatumType.Integer ||
                    datum.Type == ForthDatum.DatumType.String)
                {
                    stack.Push(datum);
                    continue;
                }

                // Primatives
                if (datum.Type == ForthDatum.DatumType.Primitive)
                {
                    var primative = ((string)datum.Value).ToLowerInvariant();
                    if (callTable.ContainsKey(primative))
                    {
                        var result = callTable[primative].Invoke(stack, variables);
                        if (default(ForthProgramResult).Equals(result))
                            continue;
                        if (!result.isSuccessful)
                            return result;
                    }
                }
            }
        }

        // Debug, print stack
        if (stack.Count == 0)
            await me.sendOutput($"DEBUG ({lineCount}): ()");
        else
            await me.sendOutput($"DEBUG ({lineCount}): (" +
            stack.Reverse().Select(s =>
            {
                return s.Value.ToString();
            }).Aggregate((c, n) => c + " " + n) + ")");

        return new ForthProgramResult(null, "Program complete");
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static ForthDatum;
using static ForthProgramResult;

public class ForthExecutionContext
{
    private static readonly Dictionary<string, Func<Stack<ForthDatum>, Dictionary<string, object>, Player, int, string, ForthProgramResult>> callTable = new Dictionary<string, Func<Stack<ForthDatum>, Dictionary<string, object>, Player, int, string, ForthProgramResult>>();
    private readonly List<List<ForthDatum>> programLines;
    private readonly Stack<ForthDatum> stack = new Stack<ForthDatum>();
    private readonly Dictionary<string, object> outerScopeVariables = new Dictionary<string, object>();
    private readonly Dictionary<string, object> contextScopedVariables = new Dictionary<string, object>();
    // KEY = scopeId:scriptId, VALUE = dictionary of variables
    private static readonly Dictionary<string, Dictionary<string, object>> programLocalVariables = new Dictionary<string, Dictionary<string, object>>();
    private readonly int scriptId;
    private readonly Player me;
    private readonly String scopeId;
    private readonly String outerScopeId;
    private bool hasRan;

    static ForthExecutionContext()
    {
        // Setup call table
        callTable.Add("pop", (stack, variables, me, trigger, command) => Pop.Execute(stack));
        callTable.Add("popn", (stack, variables, me, trigger, command) => PopN.Execute(stack));
        callTable.Add("dup", (stack, variables, me, trigger, command) =>
        {
            // DUP is the same as 1 pick.
            stack.Push(new ForthDatum(1));
            return Pick.Execute(stack);
        });
        callTable.Add("dupn", (stack, variables, me, trigger, command) => DupN.Execute(stack));
        callTable.Add("ldup", (stack, variables, me, trigger, command) => LDup.Execute(stack));
        callTable.Add("swap", (stack, variables, me, trigger, command) => Swap.Execute(stack));
        callTable.Add("over", (stack, variables, me, trigger, command) =>
        {
            // OVER is the same as 2 pick.
            stack.Push(new ForthDatum(2));
            return Pick.Execute(stack);
        });
        callTable.Add("rot", (stack, variables, me, trigger, command) =>
        {
            // ROT is the same as 3 rotate
            stack.Push(new ForthDatum(3));
            return Rotate.Execute(stack);
        });
        callTable.Add("rotate", (stack, variables, me, trigger, command) => Rotate.Execute(stack));
        callTable.Add("pick", (stack, variables, me, trigger, command) => Pick.Execute(stack));
        callTable.Add("put", (stack, variables, me, trigger, command) => Put.Execute(stack));
        callTable.Add("reverse", (stack, variables, me, trigger, command) => Reverse.Execute(stack));
        callTable.Add("lreverse", (stack, variables, me, trigger, command) => LReverse.Execute(stack));
        callTable.Add("depth", (stack, variables, me, trigger, command) =>
        {
            // DEPTH ( -- i ) 
            // Returns the number of items currently on the stack.
            stack.Push(new ForthDatum(stack.Count));
            return default(ForthProgramResult);
        });
        callTable.Add("{", (stack, variables, me, trigger, command) =>
        {
            // { ( -- marker) 
            // Pushes a marker onto the stack, to be used with } or }list or }dict.
            stack.Push(new ForthDatum("{", DatumType.Marker));
            return default(ForthProgramResult);
        });
        callTable.Add("}", (stack, variables, me, trigger, command) => MarkerEnd.Execute(stack));
        callTable.Add("@", (stack, variables, me, trigger, command) => At.Execute(stack, variables, me, trigger, command));
        callTable.Add("!", (stack, variables, me, trigger, command) => Bang.Execute(stack, variables, me, trigger, command));
    }

    public ForthExecutionContext(
        int scriptId,
        List<List<ForthDatum>> programLines,
        Player me,
        string outerScopeId = null,
        Dictionary<string, object> outerScopeVariables = null)
    {
        this.scriptId = scriptId;
        this.programLines = programLines;
        this.me = me;
        this.scopeId = Guid.NewGuid().ToString();

        if (outerScopeId != null)
        {
            this.outerScopeId = outerScopeId;

            if (outerScopeVariables != null)
            {
                foreach (var kvp in outerScopeVariables)
                    this.outerScopeVariables.Add(kvp.Key, kvp.Value);
            }
        }
    }

    public static ICollection<string> GetPrimatives()
    {
        return callTable.Keys;
    }

    private static Dictionary<string, object> GetProgramLocalVariables(string scopeId, int scriptId)
    {
        var lvarKey = scopeId + ":" + scriptId;
        Dictionary<string, object> lvarDictionary = null;
        if (programLocalVariables.ContainsKey(lvarKey))
            lvarDictionary = programLocalVariables[lvarKey];

        return lvarDictionary;
    }

    public async Task<ForthProgramResult> RunAsync(int trigger, string command, object[] args, CancellationToken cancellationToken)
    {
        if (hasRan)
        {
            return new ForthProgramResult(ForthProgramErrorResult.INTERNAL_ERROR, $"Execution scope {scopeId} tried to run twice.");
        }
        hasRan = true;

        // For each line
        int lineCount = 0;
        foreach (var line in programLines)
        {
            lineCount++;

            if (cancellationToken.IsCancellationRequested)
                return new ForthProgramResult(ForthProgramErrorResult.INTERRUPTED);

            // Line-level items

            // LVAR
            Dictionary<string, object> lvarDictionary = GetProgramLocalVariables(scopeId, scriptId);
            if (line.Count == 2 && string.Compare(line[0].Value.ToString(), "LVAR", true) == 0)
            {
                if (lvarDictionary == null)
                {
                    lvarDictionary = new Dictionary<string, object>();
                    programLocalVariables.Add(scopeId + ":" + scriptId, lvarDictionary);
                }

                var varKey = line[1].Value.ToString().ToLowerInvariant();
                if (lvarDictionary.ContainsKey(varKey))
                    return new ForthProgramResult(ForthProgramErrorResult.VARIABLE_ALREADY_DEFINED, $"Variable '{varKey}' is already defined.");

                lvarDictionary.Add(varKey, null);
                await me.sendOutput(line.Select(d => d.Value.ToString()).Aggregate((c,n) => c + " " + n));
                continue;
            }

            // VAR
            if (line.Count == 2 && string.Compare(line[0].Value.ToString(), "VAR", true) == 0)
            {
                var varKey = line[1].Value.ToString().ToLowerInvariant();
                if (contextScopedVariables.ContainsKey(varKey))
                    return new ForthProgramResult(ForthProgramErrorResult.VARIABLE_ALREADY_DEFINED, $"Variable '{varKey}' is already defined.");

                contextScopedVariables.Add(varKey, null);
                await me.sendOutput(line.Select(d => d.Value.ToString()).Aggregate((c,n) => c + " " + n));
                continue;
            }

            // VAR!
            if (line.Count == 2 && string.Compare(line[0].Value.ToString(), "VAR!", true) == 0)
            {
                var varKey = line[1].Value.ToString().ToLowerInvariant();
                if (contextScopedVariables.ContainsKey(varKey))
                    return new ForthProgramResult(ForthProgramErrorResult.VARIABLE_ALREADY_DEFINED, $"Variable '{varKey}' is already defined.");

                contextScopedVariables.Add(varKey, stack.Count > 0 ? (object)stack.Pop() : null);
                await me.sendOutput(line.Select(d => d.Value.ToString()).Aggregate((c,n) => c + " " + n));
                continue;
            }

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
                        return s.Type == DatumType.DbRef ? "#" + s.Value : s.Value.ToString();
                    }).Aggregate((c, n) => c + " " + n) + ") " + datum.Value);

                // Literals
                if (datum.Type == ForthDatum.DatumType.Integer ||
                    datum.Type == ForthDatum.DatumType.String ||
                    datum.Type == ForthDatum.DatumType.Unknown ||
                    datum.Type == ForthDatum.DatumType.DbRef)
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
                        var variables = lvarDictionary == null
                            ? contextScopedVariables
                            : lvarDictionary
                                .Union(contextScopedVariables)
                                .ToDictionary(k => k.Key, v => v.Value);

                        var result = callTable[primative].Invoke(stack, variables, me, trigger, command);

                        // Push dirty variables where they may need to go.
                        if (result.dirtyVariables != null && result.dirtyVariables.Count > 0)
                        {
                            var programLocalVariables = GetProgramLocalVariables(scopeId, scriptId);
                            foreach (var dirty in result.dirtyVariables)
                            {
                                if (programLocalVariables.ContainsKey(dirty.Key))
                                    programLocalVariables[dirty.Key] = dirty.Value;

                                if (contextScopedVariables.ContainsKey(dirty.Key))
                                    contextScopedVariables[dirty.Key] = dirty.Value;
                            }
                        }

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
                return s.Type == DatumType.DbRef ? "#" + s.Value : s.Value.ToString();
            }).Aggregate((c, n) => c + " " + n) + ")");

        return new ForthProgramResult(null, "Program complete");
    }
}
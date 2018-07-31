using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static ForthDatum;
using static ForthProgramResult;

public struct ForthWord
{
    private static readonly Dictionary<string, Func<Stack<ForthDatum>, Dictionary<string, object>, Player, Dbref, string, ForthProgramResult>> callTable = new Dictionary<string, Func<Stack<ForthDatum>, Dictionary<string, object>, Player, Dbref, string, ForthProgramResult>>();
    public readonly string name;
    public readonly Dictionary<int, ForthDatum[]> programLineNumbersAndDatum;
    private readonly Dictionary<string, object> functionScopedVariables;

    static ForthWord()
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
        callTable.Add("<", (stack, variables, me, trigger, command) => OpLessThan.Execute(stack));
        callTable.Add(">", (stack, variables, me, trigger, command) => OpGreaterThan.Execute(stack));
        callTable.Add("=", (stack, variables, me, trigger, command) => OpEquals.Execute(stack));
        callTable.Add("<=", (stack, variables, me, trigger, command) => OpLessThanOrEqual.Execute(stack));
        callTable.Add(">=", (stack, variables, me, trigger, command) => OpGreaterThanOrEqual.Execute(stack));
        callTable.Add("not", (stack, variables, me, trigger, command) => OpNot.Execute(stack));
        callTable.Add("and", (stack, variables, me, trigger, command) => OpAnd.Execute(stack));
        callTable.Add("or", (stack, variables, me, trigger, command) => OpOr.Execute(stack));
        callTable.Add("xor", (stack, variables, me, trigger, command) => OpXor.Execute(stack));
        callTable.Add("string?", (stack, variables, me, trigger, command) => OpIsString.Execute(stack));
        callTable.Add("int?", (stack, variables, me, trigger, command) => OpIsInt.Execute(stack));
        callTable.Add("float?", (stack, variables, me, trigger, command) => OpIsFloat.Execute(stack));
        callTable.Add("dbref?", (stack, variables, me, trigger, command) => OpIsDbRef.Execute(stack));
        // TODO ARRAY?
        // TODO DICTIONARY/
        // TODO ADDRESS?
        // TODO LOCK?

        // MATHEMATICAL OPERATORS
        callTable.Add("int", (stack, variables, me, trigger, command) => MathInt.Execute(stack, variables, me, trigger, command));
        callTable.Add("+", (stack, variables, me, trigger, command) => MathAdd.Execute(stack));
        callTable.Add("-", (stack, variables, me, trigger, command) => MathSubtract.Execute(stack));
        callTable.Add("*", (stack, variables, me, trigger, command) => MathMultiply.Execute(stack));
        callTable.Add("/", (stack, variables, me, trigger, command) => MathDivide.Execute(stack));
        callTable.Add("%", (stack, variables, me, trigger, command) => MathModulo.Execute(stack));

        // PROPERTY MANIPULATION
        callTable.Add("getpropval", (stack, variables, me, trigger, command) => GetPropVal.ExecuteAsync(stack).Result);

        // TIME MANIPULATION
        callTable.Add("time", (stack, variables, me, trigger, command) => Time.Execute(stack));
        callTable.Add("date", (stack, variables, me, trigger, command) => Date.Execute(stack));
        callTable.Add("systime", (stack, variables, me, trigger, command) => SysTime.Execute(stack));
        callTable.Add("systime_precise", (stack, variables, me, trigger, command) => SysTimePrecise.Execute(stack));
        callTable.Add("gmtoffset", (stack, variables, me, trigger, command) => GmtOffset.Execute(stack));
        callTable.Add("timesplit", (stack, variables, me, trigger, command) => TimeSplit.Execute(stack));
        callTable.Add("timefmt", (stack, variables, me, trigger, command) => TimeFormat.Execute(stack));

        // MISCELLANEOUS
        callTable.Add("version", (stack, variables, me, trigger, command) => Version.Execute(stack));
    }

    public ForthWord(string name, Dictionary<int, ForthDatum[]> programLineNumbersAndDatum)
    {
        if (name == null)
            throw new System.ArgumentNullException(nameof(name));
        if (programLineNumbersAndDatum == null)
            throw new System.ArgumentNullException(nameof(programLineNumbersAndDatum));

        this.name = name;
        this.programLineNumbersAndDatum = programLineNumbersAndDatum;
        this.functionScopedVariables = new Dictionary<string, object>();
    }

    public static ICollection<string> GetPrimatives()
    {
        return callTable.Keys;
    }

    public async Task<ForthProgramResult> RunAsync(
        ForthProcess process,
        Stack<ForthDatum> stack,
        Player me,
        Dbref trigger,
        string command,
        CancellationToken cancellationToken)
    {
        // For each line
        var lineCount = 0;
        var ifControlStack = new Stack<IfControl>();

        foreach (var kvpLine in programLineNumbersAndDatum)
        {
            var line = kvpLine.Value;
            lineCount++;

            if (cancellationToken.IsCancellationRequested)
                return new ForthProgramResult(ForthProgramErrorResult.INTERRUPTED);

            // Line-level items

            // VAR
            if (line.Length == 2 && string.Compare(line[0].Value.ToString(), "VAR", true) == 0)
            {
                var varKey = line[1].Value.ToString().ToLowerInvariant();
                if (functionScopedVariables.ContainsKey(varKey))
                    return new ForthProgramResult(ForthProgramErrorResult.VARIABLE_ALREADY_DEFINED, $"Variable '{varKey}' is already defined.");

                functionScopedVariables.Add(varKey, null);
                await me.sendOutput(line.Select(d => d.Value.ToString()).Aggregate((c, n) => c + " " + n));
                continue;
            }

            // VAR!
            if (line.Length == 2 && string.Compare(line[0].Value.ToString(), "VAR!", true) == 0)
            {
                var varKey = line[1].Value.ToString().ToLowerInvariant();
                if (functionScopedVariables.ContainsKey(varKey))
                    return new ForthProgramResult(ForthProgramErrorResult.VARIABLE_ALREADY_DEFINED, $"Variable '{varKey}' is already defined.");

                functionScopedVariables.Add(varKey, stack.Count > 0 ? (object)stack.Pop() : null);
                await me.sendOutput(line.Select(d => d.Value.ToString()).Aggregate((c, n) => c + " " + n));
                continue;
            }

            // For each element in line
            foreach (var datum in line)
            {
                // Execution Control
                if (datum.Type == ForthDatum.DatumType.Unknown)
                {
                    // IF
                    var primative = ((string)datum.Value).ToLowerInvariant();
                    if (string.Compare("if", primative, true) == 0)
                    {
                // Debug, print stack
                await DumpStackToDebugAsync(stack, me, lineCount, datum);

                        if (stack.Count == 0)
                        {
                            await DumpVariablesToDebugAsync(process, me);
                            return new ForthProgramResult(ForthProgramErrorResult.STACK_UNDERFLOW, "IF had no value on the stack to evaluate");
                        }

                        var eval = stack.Pop();
                        if (eval.isTrue())
                            ifControlStack.Push(IfControl.InIfAndContinue);
                        else
                            ifControlStack.Push(IfControl.InIfAndSkip);
                        continue;
                    }

                    // ELSE
                    if (string.Compare("else", primative, true) == 0)
                    {
                                        // Debug, print stack
                await DumpStackToDebugAsync(stack, me, lineCount, datum);


                        if (ifControlStack.Count == 0)
                            return new ForthProgramResult(ForthProgramErrorResult.STACK_UNDERFLOW, "ELSE encountered without preceding IF");

                        var currentControl = ifControlStack.Pop();
                        if (currentControl == IfControl.InIfAndContinue)
                            ifControlStack.Push(IfControl.InElseAndSkip);
                        else
                            ifControlStack.Push(IfControl.InElseAndContinue);

                        continue;
                    }

                    // THEN
                    if (string.Compare("then", primative, true) == 0)
                    {
                                        // Debug, print stack
                await DumpStackToDebugAsync(stack, me, lineCount, datum);


                        if (ifControlStack.Count == 0)
                            return new ForthProgramResult(ForthProgramErrorResult.STACK_UNDERFLOW, "THEN encountered without preceding IF");
                        ifControlStack.Pop();
                        continue;
                    }
                }

                if (ifControlStack.Count > 0)
                {
                                    // Debug, print stack
                await DumpStackToDebugAsync(stack, me, lineCount, datum, "(skipped)");


                    var ifControlCurrent = ifControlStack.Peek();
                    if (ifControlCurrent == IfControl.InIfAndSkip || ifControlCurrent == IfControl.InElseAndSkip)
                        continue;
                }

                // Debug, print stack
                await DumpStackToDebugAsync(stack, me, lineCount, datum);

                // Function calls
                if (datum.Type == ForthDatum.DatumType.Unknown &&
                    process.HasWord(datum.Value.ToString()))
                {
                    // Yield to other word.
                    var wordResult = await process.RunWordAsync(datum.Value.ToString(), trigger, command, cancellationToken);
                    if (!wordResult.isSuccessful)
                        return wordResult;
                    continue;
                }

                // Literals
                if (datum.Type == ForthDatum.DatumType.Float ||
                    datum.Type == ForthDatum.DatumType.Integer ||
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
                        var variables = process.GetProgramLocalVariables()
                                .Union(functionScopedVariables)
                                .ToDictionary(k => k.Key, v => v.Value);

                        var result = callTable[primative].Invoke(stack, variables, me, trigger, command);

                        // Push dirty variables where they may need to go.
                        if (result.dirtyVariables != null && result.dirtyVariables.Count > 0)
                        {
                            var programLocalVariables = process.GetProgramLocalVariables();
                            foreach (var dirty in result.dirtyVariables)
                            {
                                if (programLocalVariables.ContainsKey(dirty.Key))
                                    programLocalVariables[dirty.Key] = dirty.Value;

                                if (functionScopedVariables.ContainsKey(dirty.Key))
                                    functionScopedVariables[dirty.Key] = dirty.Value;
                            }
                        }

                        if (default(ForthProgramResult).Equals(result))
                            continue;
                        if (!result.isSuccessful)
                            return result;

                        continue;
                    }
                }

                // Unable to handle!
                return new ForthProgramResult(ForthProgramErrorResult.INTERNAL_ERROR, $"Unable to handle datum: {datum}");
            }
        }

        // Debug, print stack at end of program
        await DumpStackToDebugAsync(stack, me, lineCount);

        return new ForthProgramResult(null, $"Word {name} completed");
    }

    private async Task DumpStackToDebugAsync(Stack<ForthDatum> stack, Player me, int lineCount, ForthDatum currentDatum = default(ForthDatum), string extra = null)
    {
        // Debug, print stack
        if (stack.Count == 0)
            await me.sendOutput($"DEBUG ({lineCount}): () " + extra);
        else
            await me.sendOutput($"DEBUG ({lineCount}): (" +
            stack.Reverse().Select(s =>
            {
                return (s.Type == DatumType.String) ? "\"" + s.Value.ToString() + "\"" : s.Value.ToString();
            }).Aggregate((c, n) => c + " " + n) + ") " + (default(ForthDatum).Equals(currentDatum) ? "" : ((currentDatum.Type == DatumType.String) ? "\"" + currentDatum.Value.ToString() + "\"" : currentDatum.Value.ToString())) + " " + extra);
    }

    private async Task DumpVariablesToDebugAsync(ForthProcess process, Player me)
    {
        foreach (var lvar in process.GetProgramLocalVariables())
        {
            await me.sendOutput($"LVAR {lvar.Key}={lvar.Value}");
        }

        foreach (var local in functionScopedVariables)
        {
            await me.sendOutput($"VAR {local.Key}={local.Value}");
        }
    }
}
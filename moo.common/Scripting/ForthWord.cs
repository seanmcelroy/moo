using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static ForthDatum;
using static ForthProgramResult;
using static ForthVariable;

public struct ForthWord
{
    private static readonly Dictionary<string, Func<ForthPrimativeParameters, ForthProgramResult>> callTable = new Dictionary<string, Func<ForthPrimativeParameters, ForthProgramResult>>();
    private readonly Action<Dbref, string> notifyAction;
    public readonly string name;
    public readonly List<ForthDatum> programData;
    private readonly Dictionary<string, ForthVariable> functionScopedVariables;

    static ForthWord()
    {
        // Setup call table
        callTable.Add("pop", (p) => Pop.Execute(p));
        callTable.Add("popn", (p) => PopN.Execute(p));
        callTable.Add("dup", (p) =>
        {
            // DUP is the same as 1 pick.
            p.Stack.Push(new ForthDatum(1));
            return Pick.Execute(p);
        });
        callTable.Add("dupn", (p) => DupN.Execute(p));
        callTable.Add("ldup", (p) => LDup.Execute(p));
        callTable.Add("swap", (p) => Swap.Execute(p));
        callTable.Add("over", (p) =>
        {
            // OVER is the same as 2 pick.
            p.Stack.Push(new ForthDatum(2));
            return Pick.Execute(p);
        });
        callTable.Add("rot", (p) =>
        {
            // ROT is the same as 3 rotate
            p.Stack.Push(new ForthDatum(3));
            return Rotate.Execute(p);
        });
        callTable.Add("rotate", (p) => Rotate.Execute(p));
        callTable.Add("pick", (p) => Pick.Execute(p));
        callTable.Add("put", (p) => Put.Execute(p));
        callTable.Add("reverse", (p) => Reverse.Execute(p));
        callTable.Add("lreverse", (p) => LReverse.Execute(p));
        callTable.Add("depth", (p) =>
        {
            // DEPTH ( -- i ) 
            // Returns the number of items currently on the stack.
            p.Stack.Push(new ForthDatum(p.Stack.Count));
            return default(ForthProgramResult);
        });
        callTable.Add("{", (p) =>
        {
            // { ( -- marker) 
            // Pushes a marker onto the stack, to be used with } or }list or }dict.
            p.Stack.Push(new ForthDatum("{", DatumType.Marker));
            return default(ForthProgramResult);
        });
        callTable.Add("}", (p) => MarkerEnd.Execute(p));
        callTable.Add("@", (p) => At.Execute(p));
        callTable.Add("!", (p) => Bang.Execute(p));
        callTable.Add("<", (p) => OpLessThan.Execute(p));
        callTable.Add(">", (p) => OpGreaterThan.Execute(p));
        callTable.Add("=", (p) => OpEquals.Execute(p));
        callTable.Add("<=", (p) => OpLessThanOrEqual.Execute(p));
        callTable.Add(">=", (p) => OpGreaterThanOrEqual.Execute(p));
        callTable.Add("not", (p) => OpNot.Execute(p));
        callTable.Add("and", (p) => OpAnd.Execute(p));
        callTable.Add("or", (p) => OpOr.Execute(p));
        callTable.Add("xor", (p) => OpXor.Execute(p));
        callTable.Add("string?", (p) => OpIsString.Execute(p));
        callTable.Add("int?", (p) => OpIsInt.Execute(p));
        callTable.Add("float?", (p) => OpIsFloat.Execute(p));
        callTable.Add("dbref?", (p) => OpIsDbRef.Execute(p));
        // TODO ARRAY?
        // TODO DICTIONARY/
        // TODO ADDRESS?
        // TODO LOCK?

        // I/O OPERATORS
        callTable.Add("notify", (p) => Notify.ExecuteAsync(p).Result);

        // MATHEMATICAL OPERATORS
        callTable.Add("int", (p) => MathInt.Execute(p));
        callTable.Add("+", (p) => MathAdd.Execute(p));
        callTable.Add("-", (p) => MathSubtract.Execute(p));
        callTable.Add("*", (p) => MathMultiply.Execute(p));
        callTable.Add("/", (p) => MathDivide.Execute(p));
        callTable.Add("%", (p) => MathModulo.Execute(p));

        // STRING MANIPULATION OPERATIONS
        callTable.Add("atoi", (p) => AtoI.Execute(p));
        callTable.Add("ctoi", (p) => CtoI.Execute(p));
        callTable.Add("strlen", (p) => StrLen.Execute(p));
        callTable.Add("strcat", (p) => StrCat.Execute(p));
        callTable.Add("strcmp", (p) => StrCmp.Execute(p));
        callTable.Add("strncmp", (p) => StrNCmp.Execute(p));
        callTable.Add("stringcmp", (p) => StringCmp.Execute(p));
        callTable.Add("stringpfx", (p) => StringPfx.Execute(p));
        callTable.Add("instr", (p) => Instr.Execute(p));
        callTable.Add("rinstr", (p) => RInstr.Execute(p));

        callTable.Add("intostr", (p) => IntoStr.Execute(p));

        // PROPERTY MANIPULATION
        callTable.Add("getpropval", (p) => GetPropVal.ExecuteAsync(p).Result);

        // Database Related Operators
        callTable.Add("dbref", (p) => DbrefConvert.Execute(p));
        // TODO: PROG
        callTable.Add("trig", (p) => Trig.Execute(p));
        // TODO: CALLER
        // TODO: DBTOP
        callTable.Add("dbcmp", (p) => DbCmp.Execute(p));
        callTable.Add("location", (p) => Location.ExecuteAsync(p).Result);
        callTable.Add("contents", (p) => Contents.ExecuteAsync(p).Result);
        callTable.Add("match", (p) => Match.ExecuteAsync(p).Result);
        callTable.Add("player?", (p) => IsPlayer.ExecuteAsync(p).Result);
        callTable.Add("name", (p) => Name.ExecuteAsync(p).Result);

        // TIME MANIPULATION
        callTable.Add("time", (p) => Time.Execute(p));
        callTable.Add("date", (p) => Date.Execute(p));
        callTable.Add("systime", (p) => SysTime.Execute(p));
        callTable.Add("systime_precise", (p) => SysTimePrecise.Execute(p));
        callTable.Add("gmtoffset", (p) => GmtOffset.Execute(p));
        callTable.Add("timesplit", (p) => TimeSplit.Execute(p));
        callTable.Add("timefmt", (p) => TimeFormat.Execute(p));

        // CONNECTION MANAGEMENT OPTIONS
        callTable.Add("awake?", (p) => Awake.Execute(p));
        callTable.Add("conidle", (p) => ConIdle.Execute(p));
        callTable.Add("descrcon", (p) => DescRcon.Execute(p));
        callTable.Add("descriptors", (p) => Descriptors.Execute(p));

        // MISCELLANEOUS
        callTable.Add("version", (p) => Version.Execute(p));
    }

    public ForthWord(Action<Dbref, string> notifyAction, string name, List<ForthDatum> programData)
    {
        if (notifyAction == null)
            throw new System.ArgumentNullException(nameof(notifyAction));
        if (name == null)
            throw new System.ArgumentNullException(nameof(name));
        if (programData == null)
            throw new System.ArgumentNullException(nameof(programData));

        this.notifyAction = notifyAction;
        this.name = name;
        this.programData = programData;
        this.functionScopedVariables = new Dictionary<string, ForthVariable>();
    }

    public static ICollection<string> GetPrimatives()
    {
        return callTable.Keys;
    }

    public async Task<ForthProgramResult> RunAsync(
        ForthProcess process,
        Stack<ForthDatum> stack,
        PlayerConnection connection,
        Dbref trigger,
        string command,
        CancellationToken cancellationToken)
    {
        // For each line
        var lineCount = 0;
        var controlFlow = new Stack<ControlFlowMarker>();

        int x = -1;
        while (x < programData.Count - 1)
        {
            x++;
            var datum = programData[x];
            lineCount++;

            if (cancellationToken.IsCancellationRequested)
                return new ForthProgramResult(ForthProgramErrorResult.INTERRUPTED);

            // Line-level items

            /*/
            // VAR
            if (line.Length == 2 && string.Compare(line[0].Value.ToString(), "VAR", true) == 0)
            {
                var varKey = line[1].Value.ToString().ToLowerInvariant();
                if (functionScopedVariables.ContainsKey(varKey))
                    return new ForthProgramResult(ForthProgramErrorResult.VARIABLE_ALREADY_DEFINED, $"Variable '{varKey}' is already defined.");

                functionScopedVariables.Add(varKey, null);
                await connection.sendOutput(line.Select(d => d.Value.ToString()).Aggregate((c, n) => c + " " + n));
                continue;
            }

            // VAR!
            if (line.Length == 2 && string.Compare(line[0].Value.ToString(), "VAR!", true) == 0)
            {
                var varKey = line[1].Value.ToString().ToLowerInvariant();
                if (functionScopedVariables.ContainsKey(varKey))
                    return new ForthProgramResult(ForthProgramErrorResult.VARIABLE_ALREADY_DEFINED, $"Variable '{varKey}' is already defined.");

                functionScopedVariables.Add(varKey, stack.Count > 0 ? (object)stack.Pop() : null);
                await connection.sendOutput(line.Select(d => d.Value.ToString()).Aggregate((c, n) => c + " " + n));
                continue;
            }
            */

            // For each element in line
            var datumString = datum.Value?.ToString().ToLowerInvariant();

            // Do we have something unknown on the top of the stack?
            if (stack.Count > 0 && stack.Peek().Type == ForthDatum.DatumType.Unknown)
            {
                return new ForthProgramResult(ForthProgramErrorResult.SYNTAX_ERROR, $"Unable to handle datum: {stack.Peek()}");
            }

            // Execution Control
            if (datum.Type == ForthDatum.DatumType.Unknown)
            {
                // IF
                if (string.Compare("if", datumString, true) == 0)
                {
                    // I could be an 'if' inside a skipped branch.
                    if (controlFlow.Count > 0)
                    {
                        var controlCurrent = controlFlow.Peek();
                        if (controlCurrent.Element == ControlFlowElement.InIfAndSkip
                         || controlCurrent.Element == ControlFlowElement.InElseAndSkip
                         || controlCurrent.Element == ControlFlowElement.SkippedBranch
                         || controlCurrent.Element == ControlFlowElement.SkipToAfterNextUntilOrRepeat)
                        {
                            await DumpStackToDebugAsync(stack, connection, lineCount, datum, "(skipped)");
                            controlFlow.Push(new ControlFlowMarker(ControlFlowElement.SkippedBranch, x));
                            continue;
                        }
                    }

                    // Debug, print stack
                    await DumpStackToDebugAsync(stack, connection, lineCount, datum);

                    if (stack.Count == 0)
                    {
                        await DumpVariablesToDebugAsync(process, connection);
                        return new ForthProgramResult(ForthProgramErrorResult.STACK_UNDERFLOW, "IF had no value on the stack to evaluate");
                    }

                    var eval = stack.Pop();
                    if (eval.isTrue())
                        controlFlow.Push(new ControlFlowMarker(ControlFlowElement.InIfAndContinue, x));
                    else
                        controlFlow.Push(new ControlFlowMarker(ControlFlowElement.InIfAndSkip, x));
                    continue;
                }

                // ELSE
                if (string.Compare("else", datumString, true) == 0)
                {
                    // I could be an 'else' inside a skipped branch.
                    if (controlFlow.Count > 0)
                    {
                        var controlCurrent = controlFlow.Peek();
                        if (controlCurrent.Element == ControlFlowElement.SkippedBranch
                         || controlCurrent.Element == ControlFlowElement.SkipToAfterNextUntilOrRepeat)
                        {
                            await DumpStackToDebugAsync(stack, connection, lineCount, datum, "(skipped)");
                            continue;
                        }
                    }

                    // Debug, print stack
                    await DumpStackToDebugAsync(stack, connection, lineCount, datum);

                    if (controlFlow.Count == 0)
                        return new ForthProgramResult(ForthProgramErrorResult.STACK_UNDERFLOW, "ELSE encountered without preceding IF");

                    var currentControl = controlFlow.Pop();
                    if (currentControl.Element == ControlFlowElement.InIfAndContinue)
                        controlFlow.Push(new ControlFlowMarker(ControlFlowElement.InElseAndSkip, x));
                    else
                        controlFlow.Push(new ControlFlowMarker(ControlFlowElement.InElseAndContinue, x));

                    continue;
                }

                // THEN
                if (string.Compare("then", datumString, true) == 0)
                {
                    // I could be an 'else' inside a skipped branch.
                    if (controlFlow.Count > 0)
                    {
                        var controlCurrent = controlFlow.Peek();
                        if (controlCurrent.Element == ControlFlowElement.SkippedBranch
                         || controlCurrent.Element == ControlFlowElement.SkipToAfterNextUntilOrRepeat)
                        {
                            await DumpStackToDebugAsync(stack, connection, lineCount, datum, "(skipped)");
                            // A skipped if will push a SkippedBranch, so we should pop it.
                            controlFlow.Pop();
                            continue;
                        }
                    }

                    // Debug, print stack
                    await DumpStackToDebugAsync(stack, connection, lineCount, datum);


                    if (controlFlow.Count == 0)
                        return new ForthProgramResult(ForthProgramErrorResult.STACK_UNDERFLOW, "THEN encountered without preceding IF");
                    controlFlow.Pop();
                    continue;
                }

                // EXIT
                if (string.Compare("exit", datumString, true) == 0)
                {
                    // I could be an 'exit' inside a skipped branch.
                    if (controlFlow.Count > 0)
                    {
                        var controlCurrent = controlFlow.Peek();
                        if (controlCurrent.Element == ControlFlowElement.InIfAndSkip
                         || controlCurrent.Element == ControlFlowElement.InElseAndSkip
                         || controlCurrent.Element == ControlFlowElement.SkippedBranch
                         || controlCurrent.Element == ControlFlowElement.SkipToAfterNextUntilOrRepeat)
                        {
                            await DumpStackToDebugAsync(stack, connection, lineCount, datum, "(skipped)");
                            continue;
                        }
                    }

                    // Debug, print stack
                    await DumpStackToDebugAsync(stack, connection, lineCount, datum);

                    return new ForthProgramResult(null, $"Word {name} completed via exit");
                }

                // BEGIN
                if (string.Compare("begin", datumString, true) == 0)
                {
                    // I could be an 'begin' inside a skipped branch.
                    if (controlFlow.Count > 0)
                    {
                        var controlCurrent = controlFlow.Peek();
                        if (controlCurrent.Element == ControlFlowElement.InIfAndSkip
                         || controlCurrent.Element == ControlFlowElement.InElseAndSkip
                         || controlCurrent.Element == ControlFlowElement.SkippedBranch
                         || controlCurrent.Element == ControlFlowElement.SkipToAfterNextUntilOrRepeat)
                        {
                            await DumpStackToDebugAsync(stack, connection, lineCount, datum, "(skipped)");
                            continue;
                        }
                    }

                    // Debug, print stack
                    await DumpStackToDebugAsync(stack, connection, lineCount, datum);

                    controlFlow.Push(new ControlFlowMarker(ControlFlowElement.BeginMarker, x));
                    continue;
                }

                // WHILE
                if (string.Compare("while", datumString, true) == 0)
                {
                    // I could be a 'while' inside a skipped branch.
                    if (controlFlow.Count > 0)
                    {
                        var controlCurrent = controlFlow.Peek();
                        if (controlCurrent.Element == ControlFlowElement.InIfAndSkip
                         || controlCurrent.Element == ControlFlowElement.InElseAndSkip
                         || controlCurrent.Element == ControlFlowElement.SkippedBranch
                         || controlCurrent.Element == ControlFlowElement.SkipToAfterNextUntilOrRepeat)
                        {
                            await DumpStackToDebugAsync(stack, connection, lineCount, datum, "(skipped)");
                            continue;
                        }
                    }

                    await DumpStackToDebugAsync(stack, connection, lineCount, datum);

                    if (stack.Count == 0)
                    {
                        return new ForthProgramResult(ForthProgramErrorResult.STACK_UNDERFLOW, "WHILE had no value on the stack to evaluate");
                    }

                    var eval = stack.Pop();
                    if (eval.isFalse())
                    {
                        controlFlow.Push(new ControlFlowMarker(ControlFlowElement.SkipToAfterNextUntilOrRepeat, x));
                        continue;
                    }
                }

                // REPEAT
                if (string.Compare("repeat", datumString, true) == 0)
                {
                    await DumpStackToDebugAsync(stack, connection, lineCount, datum);

                    // I could be an 'repeat' inside a skipped branch.
                    if (controlFlow.Count == 0)
                        return new ForthProgramResult(ForthProgramErrorResult.STACK_UNDERFLOW, "REPEAT but no previous BEGIN, FOR, or FOREACH on the stack");

                    var controlCurrent = controlFlow.Peek();

                    if (controlCurrent.Element == ControlFlowElement.SkipToAfterNextUntilOrRepeat)
                    {
                        controlFlow.Pop(); // Pop the skip
                        controlFlow.Pop(); // Pop the opening begin
                        continue;
                    }

                    // Go back to previous BEGIN, FOREACH, or FOR
                    var found = false;
                    while (controlFlow.Count > 0)
                    {
                        var nextControl = controlFlow.Pop();
                        if (nextControl.Element == ControlFlowElement.BeginMarker
                         || nextControl.Element == ControlFlowElement.ForEachMarker
                         || nextControl.Element == ControlFlowElement.ForMarker)
                        {
                            x = nextControl.Index - 1; // Go back to BEGIN, so it gets pushed back on the stack for the next iteration
                            found = true;
                            continue;
                        }
                    }

                    if (found)
                        continue;

                    return new ForthProgramResult(ForthProgramErrorResult.STACK_UNDERFLOW, "REPEAT but no previous BEGIN, FOR, or FOREACH");
                }

                // UNTIL
                if (string.Compare("until", datumString, true) == 0)
                {
                    // I could be an 'until' inside a skipped branch.
                    if (controlFlow.Count == 0)
                        return new ForthProgramResult(ForthProgramErrorResult.STACK_UNDERFLOW, "UNTIL but no previous BEGIN, FOR, or FOREACH on the stack");

                    var controlCurrent = controlFlow.Peek();

                    if (controlCurrent.Element == ControlFlowElement.SkipToAfterNextUntilOrRepeat)
                    {
                        await DumpStackToDebugAsync(stack, connection, lineCount, datum);
                        controlFlow.Pop(); // Pop the skip
                        controlFlow.Pop(); // Pop the opening begin
                        continue;
                    }

                    if (controlCurrent.Element == ControlFlowElement.InIfAndSkip || controlCurrent.Element == ControlFlowElement.InElseAndSkip || controlCurrent.Element == ControlFlowElement.SkippedBranch)
                    {
                        await DumpStackToDebugAsync(stack, connection, lineCount, datum, "(skipped)");
                        continue;
                    }

                    // Debug, print stack
                    await DumpStackToDebugAsync(stack, connection, lineCount, datum);

                    if (stack.Count == 0)
                    {
                        await DumpStackToDebugAsync(stack, connection, lineCount, datum);
                        return new ForthProgramResult(ForthProgramErrorResult.STACK_UNDERFLOW, "UNTIL had no value on the stack to evaluate");
                    }

                    var eval = stack.Pop();
                    if (eval.isTrue())
                        continue;

                    // Go back to previous BEGIN or FOR
                    var found = false;
                    while (controlFlow.Count > 0)
                    {
                        var nextControl = controlFlow.Pop();
                        if (nextControl.Element == ControlFlowElement.BeginMarker
                         || nextControl.Element == ControlFlowElement.ForMarker)
                        {
                            x = nextControl.Index;
                            found = true;
                            continue;
                        }
                    }

                    if (found)
                        continue;

                    await DumpStackToDebugAsync(stack, connection, lineCount, datum);
                    return new ForthProgramResult(ForthProgramErrorResult.STACK_UNDERFLOW, "UNTIL but no previous BEGIN or FOR");
                }
            }

            if (controlFlow.Count > 0)
            {
                var controlCurrent = controlFlow.Peek();
                if (controlCurrent.Element == ControlFlowElement.InIfAndSkip
                 || controlCurrent.Element == ControlFlowElement.InElseAndSkip
                 || controlCurrent.Element == ControlFlowElement.SkippedBranch
                 || controlCurrent.Element == ControlFlowElement.SkipToAfterNextUntilOrRepeat)
                {
                    // Debug, print stack
                    await DumpStackToDebugAsync(stack, connection, lineCount, datum, "(skipped)");
                    continue;
                }
            }

            // Debug, print stack
            await DumpStackToDebugAsync(stack, connection, lineCount, datum);

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

            var xx = 0;
            if (string.Compare("MIN_IDLE", datumString, true) == 0)
            {
                xx++;
            }

            // Variables
            var variables = process.GetProgramLocalVariables()
                            .Union(functionScopedVariables)
                            .ToDictionary(k => k.Key, v => v.Value);

            if (datum.Type == ForthDatum.DatumType.Unknown &&
                (string.Compare("me", datumString, true) == 0
                || string.Compare("here", datumString, true) == 0))
            {
                stack.Push(new ForthDatum(datum.Value, DatumType.Variable));
                continue;
            }

            if (datum.Type == ForthDatum.DatumType.Unknown &&
                variables.ContainsKey(datumString))
            {
                var v = variables[datumString];
                if (v.IsConstant)
                    stack.Push(new ForthDatum(v.Value, v.Type == VariableType.String ? DatumType.String : (v.Type == VariableType.Float ? DatumType.Float : (v.Type == VariableType.Integer ? DatumType.Integer : (v.Type == VariableType.DbRef ? DatumType.DbRef : DatumType.Unknown)))));
                else
                    stack.Push(new ForthDatum(datum.Value, DatumType.Variable));
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
                if (callTable.ContainsKey(datumString))
                {
                    var p = new ForthPrimativeParameters(process.Server, stack, variables, connection, trigger, command, (d, s) => process.Notify(d, s),
                                                cancellationToken);

                    var result = callTable[datumString].Invoke(p);

                    // Push dirty variables where they may need to go.
                    if (result.dirtyVariables != null && result.dirtyVariables.Count > 0)
                    {
                        var programLocalVariables = process.GetProgramLocalVariables();
                        foreach (var dirty in result.dirtyVariables)
                        {
                            if (programLocalVariables.ContainsKey(dirty.Key))
                            {
                                var v = programLocalVariables[dirty.Key];
                                if (v.IsConstant)
                                    return new ForthProgramResult(ForthProgramErrorResult.VARIABLE_IS_CONSTANT, $"Variable {dirty.Key} is a constant in this scope and cannot be changed.");

                                programLocalVariables[dirty.Key] = dirty.Value;
                            }

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

                // Unable to handle!
                return new ForthProgramResult(ForthProgramErrorResult.SYNTAX_ERROR, $"Unable to handle datum: {datum}");
            }

            // Unable to handle!
            return new ForthProgramResult(ForthProgramErrorResult.INTERNAL_ERROR, $"Unable to handle datum: {datum}");
        }

        // Debug, print stack at end of program
        await DumpStackToDebugAsync(stack, connection, lineCount);

        return new ForthProgramResult(null, $"Word {name} completed");
    }

    private async Task DumpStackToDebugAsync(Stack<ForthDatum> stack, PlayerConnection connection, int lineCount, ForthDatum currentDatum = default(ForthDatum), string extra = null)
    {
        // Debug, print stack
        if (stack.Count == 0)
            await connection.sendOutput($"DEBUG ({lineCount}): () " + extra);
        else
            await connection.sendOutput($"DEBUG ({lineCount}): (" +
            stack.Reverse().Select(s =>
            {
                return (s.Type == DatumType.String) ? "\"" + s.Value.ToString() + "\"" : s.Value.ToString();
            }).Aggregate((c, n) => c + " " + n) + ") " + (default(ForthDatum).Equals(currentDatum) ? "" : ((currentDatum.Type == DatumType.String) ? "\"" + currentDatum.Value.ToString() + "\"" : currentDatum.Value.ToString())) + " " + extra);
    }

    private async Task DumpVariablesToDebugAsync(ForthProcess process, PlayerConnection connection)
    {
        foreach (var lvar in process.GetProgramLocalVariables())
        {
            await connection.sendOutput($"LVAR {lvar.Key}={lvar.Value}");
        }

        foreach (var local in functionScopedVariables)
        {
            await connection.sendOutput($"VAR {local.Key}={local.Value}");
        }
    }
}
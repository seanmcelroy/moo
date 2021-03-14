using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using moo.common.Connections;
using moo.common.Models;
using moo.common.Scripting.ForthPrimatives;
using static moo.common.Scripting.ForthDatum;
using static moo.common.Scripting.ForthVariable;

namespace moo.common.Scripting
{
    public struct ForthWord
    {
        private static readonly Dictionary<string, Func<ForthPrimativeParameters, ForthPrimativeResult>> callTable = new();
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
                return default;
            });
            callTable.Add("{", (p) =>
            {
                // { ( -- marker) 
                // Pushes a marker onto the stack, to be used with } or }list or }dict.
                p.Stack.Push(new ForthDatum("{", DatumType.Marker));
                return default;
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
            callTable.Add("notify_except", (p) =>
            {
                // NOTIFY_EXCEPT is the same as 1 swap notify_exclude
                p.Stack.Push(new ForthDatum(1));
                var swapResult = Swap.Execute(p);
                if (swapResult.IsSuccessful)
                    return NotifyExclude.ExecuteAsync(p).Result;
                else
                    return swapResult;
            });
            callTable.Add("notify_exclude", (p) => NotifyExclude.ExecuteAsync(p).Result);

            // MATHEMATICAL OPERATORS
            callTable.Add("abs", (p) => Abs.Execute(p));
            callTable.Add("int", (p) => MathInt.Execute(p));
            callTable.Add("sign", (p) => Sign.Execute(p));
            callTable.Add("getseed", (p) => RandomMethods.GetSeed(p));
            callTable.Add("setseed", (p) => RandomMethods.SetSeed(p));
            callTable.Add("srand", (p) => RandomMethods.SRand(p));
            callTable.Add("random", (p) => RandomMethods.Random(p));
            callTable.Add("bitor", (p) => MathBitOr.Execute(p));
            callTable.Add("bitxor", (p) => MathBitXOr.Execute(p));
            callTable.Add("bitand", (p) => MathBitXOr.Execute(p));
            callTable.Add("bitshift", (p) => MathBitShift.Execute(p));
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
            callTable.Add("smatch", (p) => SMatch.Execute(p));
            callTable.Add("instr", (p) => Instr.Execute(p));
            callTable.Add("rinstr", (p) => RInstr.Execute(p));
            callTable.Add("strcut", (p) => StrCut.Execute(p));
            callTable.Add("midstr", (p) => MidStr.Execute(p));
            callTable.Add("split", (p) => Split.Execute(p));
            callTable.Add("rsplit", (p) => RSplit.Execute(p));

            callTable.Add("subst", (p) => Subst.Execute(p));

            callTable.Add("intostr", (p) => IntoStr.Execute(p));

            callTable.Add("toupper", (p) => ToUpper.Execute(p));
            callTable.Add("tolower", (p) => ToLower.Execute(p));
            callTable.Add("striplead", (p) => StripLead.Execute(p));
            callTable.Add("striptail", (p) => StripTail.Execute(p));

            callTable.Add("unparseobj", (p) => UnparseObj.ExecuteAsync(p).Result);

            // PROPERTY MANIPULATION
            callTable.Add("getprop", (p) => GetProp.ExecuteAsync(p).Result);
            callTable.Add("getpropstr", (p) => GetPropStr.ExecuteAsync(p).Result);
            callTable.Add("getpropval", (p) => GetPropVal.ExecuteAsync(p).Result);
            callTable.Add("getpropfval", (p) => GetPropFVal.ExecuteAsync(p).Result);
            callTable.Add("addprop", (p) => AddProp.ExecuteAsync(p).Result);
            callTable.Add("setprop", (p) => SetProp.ExecuteAsync(p).Result);

            // Database Related Operators
            callTable.Add("dbref", (p) => DbrefConvert.Execute(p));
            // TODO: PROG
            callTable.Add("trig", (p) => Trig.Execute(p));
            // TODO: CALLER
            // TODO: DBTOP
            callTable.Add("dbcmp", (p) => DbCmp.Execute(p));
            callTable.Add("owner", (p) => Owner.ExecuteAsync(p).Result);
            callTable.Add("location", (p) => Location.ExecuteAsync(p).Result);
            callTable.Add("contents", (p) => Contents.ExecuteAsync(p).Result);
            callTable.Add("next", (p) => Next.ExecuteAsync(p).Result);
            callTable.Add("match", (p) => Match.ExecuteAsync(p).Result);
            callTable.Add("pmatch", (p) => PMatch.Execute(p));
            callTable.Add("part_pmatch", (p) => PartPMatch.Execute(p));
            callTable.Add("pennies", (p) => Pennies.ExecuteAsync(p).Result);
            callTable.Add("flag?", (p) => HasFlag.ExecuteAsync(p).Result);
            callTable.Add("ok?", (p) => IsOk.ExecuteAsync(p).Result);
            callTable.Add("player?", (p) => IsPlayer.ExecuteAsync(p).Result);
            callTable.Add("room?", (p) => IsRoom.ExecuteAsync(p).Result);
            callTable.Add("thing?", (p) => IsThing.ExecuteAsync(p).Result);
            callTable.Add("exit?", (p) => IsExit.ExecuteAsync(p).Result);
            callTable.Add("program?", (p) => IsProgram.ExecuteAsync(p).Result);
            callTable.Add("sysparm", (p) => SysParm.Execute(p));
            callTable.Add("name", (p) => Name.ExecuteAsync(p).Result);
            callTable.Add("getlink", (p) => GetLink.ExecuteAsync(p).Result);

            // TIME MANIPULATION
            callTable.Add("time", (p) => Time.Execute(p));
            callTable.Add("date", (p) => Date.Execute(p));
            callTable.Add("systime", (p) => SysTime.Execute(p));
            callTable.Add("systime_precise", (p) => SysTimePrecise.Execute(p));
            callTable.Add("gmtoffset", (p) => GmtOffset.Execute(p));
            callTable.Add("timesplit", (p) => TimeSplit.Execute(p));
            callTable.Add("timefmt", (p) => TimeFormat.Execute(p));

            // PROCESS MANAGEMENT OPERATORS
            callTable.Add("setmode", (p) => SetMode.Execute(p));

            // CONNECTION MANAGEMENT OPERATORS
            callTable.Add("awake?", (p) => Awake.Execute(p));
            callTable.Add("conidle", (p) => ConIdle.Execute(p));
            callTable.Add("descrcon", (p) => DescRcon.Execute(p));
            callTable.Add("descriptors", (p) => Descriptors.Execute(p));

            // MISCELLANEOUS
            callTable.Add("force", (p) => Force.ExecuteAsync(p).Result);
            callTable.Add("version", (p) => ForthPrimatives.Version.Execute(p));
        }

        public ForthWord(string name, List<ForthDatum> programData)
        {
            this.name = name ?? throw new ArgumentNullException(nameof(name));
            this.programData = programData ?? throw new ArgumentNullException(nameof(programData));
            functionScopedVariables = new Dictionary<string, ForthVariable>();
        }

        public static ICollection<string> GetPrimatives() => callTable.Keys;

        public async Task<ForthWordResult> RunAsync(
            ForthProcess process,
            Stack<ForthDatum> stack,
            PlayerConnection connection,
            Dbref trigger,
            string command,
            Dbref? lastListItem,
            CancellationToken cancellationToken)
        {
            // For each line
            var verbosity = 0;
            var lineCount = 0;
            var controlFlow = new Stack<ControlFlowMarker>();

            int x = -1;
            while (x < programData.Count - 1)
            {
                x++;
                var datum = programData[x];
                lineCount++;

                if (cancellationToken.IsCancellationRequested)
                    return new ForthWordResult(ForthErrorResult.INTERRUPTED, "Cancellation received");

                if (process.State == ForthProcess.ProcessState.Pausing)
                    process.State = ForthProcess.ProcessState.Paused;

                if (process.State == ForthProcess.ProcessState.Preempting)
                {
                    await Server.GetInstance().PreemptProcess(process.ProcessId, cancellationToken);
                    process.State = ForthProcess.ProcessState.RunningPreempt;
                }

                while (process.State == ForthProcess.ProcessState.Paused)
                {
                    Thread.Yield();
                    Thread.Sleep(1000);
                }

                if (verbosity > 5)
                    Console.WriteLine($"DATUM: {datum.Value} \tSTACK: {stack.Reverse().Select(x => x.Value?.ToString() ?? string.Empty).Aggregate((c, n) => c + "," + n)}");

                // If I'm pre-empted, then spin until 
                if (Server.GetInstance().PreemptProcessId != 0 && Server.GetInstance().PreemptProcessId != process.ProcessId)
                {
                    process.State = ForthProcess.ProcessState.Preempted;
                    while (process.State == ForthProcess.ProcessState.Preempted)
                    {
                        Thread.Yield();
                        Thread.Sleep(100);
                    }
                    process.State = ForthProcess.ProcessState.Running;
                }

                // Line-level items

                /*/
                // VAR
                if (line.Length == 2 && string.Compare(line[0].Value.ToString(), "VAR", true) == 0)
                {
                    var varKey = line[1].Value.ToString().ToLowerInvariant();
                    if (functionScopedVariables.ContainsKey(varKey))
                        return new ForthProgramResult(ForthErrorResult.VARIABLE_ALREADY_DEFINED, $"Variable '{varKey}' is already defined.");

                    functionScopedVariables.Add(varKey, null);
                    await connection.sendOutput(line.Select(d => d.Value.ToString()).Aggregate((c, n) => c + " " + n));
                    continue;
                }

                // VAR!
                if (line.Length == 2 && string.Compare(line[0].Value.ToString(), "VAR!", true) == 0)
                {
                    var varKey = line[1].Value.ToString().ToLowerInvariant();
                    if (functionScopedVariables.ContainsKey(varKey))
                        return new ForthProgramResult(ForthErrorResult.VARIABLE_ALREADY_DEFINED, $"Variable '{varKey}' is already defined.");

                    functionScopedVariables.Add(varKey, stack.Count > 0 ? (object)stack.Pop() : null);
                    await connection.sendOutput(line.Select(d => d.Value.ToString()).Aggregate((c, n) => c + " " + n));
                    continue;
                }
                */

                // For each element in line
                var datumLiteral = datum.Value?.ToString();

                // Do we have something unknown on the top of the stack?
                var topOfStack = stack.Count > 0 ? stack.Peek() : default;
                if (stack.Count > 0 && topOfStack.Type == DatumType.Unknown)
                    return new ForthWordResult(ForthErrorResult.SYNTAX_ERROR, $"Unable to handle datum on top of stack: {topOfStack}({topOfStack.FileLineNumber},{topOfStack.ColumnNumber})");

                // Execution Control
                if (datum.Type == DatumType.Unknown)
                {
                    // IF
                    if (string.Compare("if", datumLiteral, true) == 0)
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
                                if (verbosity >= 2)
                                    await DumpStackToDebugAsync(stack, connection, lineCount, datum, "(skipped)");
                                controlFlow.Push(new ControlFlowMarker(ControlFlowElement.SkippedBranch, x));
                                continue;
                            }
                        }

                        // Debug, print stack
                        if (verbosity >= 2) await DumpStackToDebugAsync(stack, connection, lineCount, datum);

                        if (stack.Count == 0)
                        {
                            if (verbosity >= 2)
                                await DumpVariablesToDebugAsync(process, connection);
                            return new ForthWordResult(ForthErrorResult.STACK_UNDERFLOW, "IF had no value on the stack to evaluate");
                        }

                        var eval = stack.Pop();
                        if (eval.isTrue())
                            controlFlow.Push(new ControlFlowMarker(ControlFlowElement.InIfAndContinue, x));
                        else
                            controlFlow.Push(new ControlFlowMarker(ControlFlowElement.InIfAndSkip, x));
                        continue;
                    }

                    // ELSE
                    if (string.Compare("else", datumLiteral, true) == 0)
                    {
                        // I could be an 'else' inside a skipped branch.
                        if (controlFlow.Count > 0)
                        {
                            var controlCurrent = controlFlow.Peek();
                            if (controlCurrent.Element == ControlFlowElement.SkippedBranch
                             || controlCurrent.Element == ControlFlowElement.SkipToAfterNextUntilOrRepeat)
                            {
                                if (verbosity >= 2)
                                    await DumpStackToDebugAsync(stack, connection, lineCount, datum, "(skipped)");
                                continue;
                            }
                        }

                        // Debug, print stack
                        if (verbosity >= 2)
                            await DumpStackToDebugAsync(stack, connection, lineCount, datum);

                        if (controlFlow.Count == 0)
                            return new ForthWordResult(ForthErrorResult.STACK_UNDERFLOW, "ELSE encountered without preceding IF");

                        var currentControl = controlFlow.Pop();
                        if (currentControl.Element == ControlFlowElement.InIfAndContinue)
                            controlFlow.Push(new ControlFlowMarker(ControlFlowElement.InElseAndSkip, x));
                        else
                            controlFlow.Push(new ControlFlowMarker(ControlFlowElement.InElseAndContinue, x));

                        continue;
                    }

                    // THEN
                    if (string.Compare("then", datumLiteral, true) == 0)
                    {
                        // I could be an 'else' inside a skipped branch.
                        if (controlFlow.Count > 0)
                        {
                            var controlCurrent = controlFlow.Peek();
                            if (controlCurrent.Element == ControlFlowElement.SkippedBranch
                             || controlCurrent.Element == ControlFlowElement.SkipToAfterNextUntilOrRepeat)
                            {
                                if (verbosity >= 2)
                                    await DumpStackToDebugAsync(stack, connection, lineCount, datum, "(skipped)");
                                // A skipped if will push a SkippedBranch, so we should pop it.
                                controlFlow.Pop();
                                continue;
                            }
                        }

                        // Debug, print stack
                        if (verbosity >= 2) await DumpStackToDebugAsync(stack, connection, lineCount, datum);

                        if (controlFlow.Count == 0)
                            return new ForthWordResult(ForthErrorResult.STACK_UNDERFLOW, "THEN encountered without preceding IF");
                        controlFlow.Pop();
                        continue;
                    }

                    // EXIT
                    if (string.Compare("exit", datumLiteral, true) == 0)
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
                                if (verbosity >= 2)
                                    await DumpStackToDebugAsync(stack, connection, lineCount, datum, "(skipped)");
                                continue;
                            }
                        }

                        // Debug, print stack
                        if (verbosity >= 2)
                            await DumpStackToDebugAsync(stack, connection, lineCount, datum);

                        return new ForthWordResult($"Word {name} completed via exit");
                    }

                    // BEGIN
                    if (string.Compare("begin", datumLiteral, true) == 0)
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
                                if (verbosity >= 2)
                                    await DumpStackToDebugAsync(stack, connection, lineCount, datum, "(skipped)");
                                continue;
                            }
                        }

                        // Debug, print stack
                        if (verbosity >= 2)
                            await DumpStackToDebugAsync(stack, connection, lineCount, datum);

                        controlFlow.Push(new ControlFlowMarker(ControlFlowElement.BeginMarker, x));
                        continue;
                    }

                    // WHILE
                    if (string.Compare("while", datumLiteral, true) == 0)
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
                                if (verbosity >= 2)
                                    await DumpStackToDebugAsync(stack, connection, lineCount, datum, "(skipped)");
                                continue;
                            }
                        }

                        if (verbosity >= 2)
                            await DumpStackToDebugAsync(stack, connection, lineCount, datum);

                        if (stack.Count == 0)
                        {
                            return new ForthWordResult(ForthErrorResult.STACK_UNDERFLOW, "WHILE had no value on the stack to evaluate");
                        }

                        var eval = stack.Pop();
                        if (eval.isFalse())
                        {
                            controlFlow.Push(new ControlFlowMarker(ControlFlowElement.SkipToAfterNextUntilOrRepeat, x));
                            continue;
                        }
                        else
                            continue;
                    }

                    // BREAK
                    if (string.Compare("break", datumLiteral, true) == 0)
                    {
                        // I could be a 'break' inside a skipped branch.
                        if (controlFlow.Count > 0)
                        {
                            var controlCurrent = controlFlow.Peek();

                            if (controlCurrent.Element == ControlFlowElement.SkippedBranch
                             || controlCurrent.Element == ControlFlowElement.SkipToAfterNextUntilOrRepeat)
                            {
                                if (verbosity >= 2)
                                    await DumpStackToDebugAsync(stack, connection, lineCount, datum, "(skipped)");
                                continue;
                            }
                        }

                        await DumpStackToDebugAsync(stack, connection, lineCount, datum);
                        controlFlow.Push(new ControlFlowMarker(ControlFlowElement.SkipToAfterNextUntilOrRepeat, x));
                        continue;
                    }

                    // REPEAT
                    if (string.Compare("repeat", datumLiteral, true) == 0)
                    {
                        if (verbosity >= 2)
                            await DumpStackToDebugAsync(stack, connection, lineCount, datum);

                        if (controlFlow.Count == 0)
                            return new ForthWordResult(ForthErrorResult.STACK_UNDERFLOW, "REPEAT but no previous BEGIN, FOR, or FOREACH on the stack");

                        // I could be a 'repeat' inside a skipped branch.
                        var controlCurrent = controlFlow.Peek();
                        if (controlCurrent.Element == ControlFlowElement.InIfAndSkip
                         || controlCurrent.Element == ControlFlowElement.InElseAndSkip
                         || controlCurrent.Element == ControlFlowElement.SkippedBranch)
                        {
                            if (verbosity >= 2)
                                await DumpStackToDebugAsync(stack, connection, lineCount, datum, "(skipped)");
                            continue;
                        }

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

                        return new ForthWordResult(ForthErrorResult.STACK_UNDERFLOW, "REPEAT but no previous BEGIN, FOR, or FOREACH");
                    }

                    // UNTIL
                    if (string.Compare("until", datumLiteral, true) == 0)
                    {
                        // I could be an 'until' inside a skipped branch.
                        if (controlFlow.Count == 0)
                            return new ForthWordResult(ForthErrorResult.STACK_UNDERFLOW, "UNTIL but no previous BEGIN, FOR, or FOREACH on the stack");

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
                            if (verbosity >= 2)
                                await DumpStackToDebugAsync(stack, connection, lineCount, datum, "(skipped)");
                            continue;
                        }

                        // Debug, print stack
                        await DumpStackToDebugAsync(stack, connection, lineCount, datum);

                        if (stack.Count == 0)
                        {
                            if (verbosity >= 2)
                                await DumpStackToDebugAsync(stack, connection, lineCount, datum);
                            return new ForthWordResult(ForthErrorResult.STACK_UNDERFLOW, "UNTIL had no value on the stack to evaluate");
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

                        if (verbosity >= 2)
                            await DumpStackToDebugAsync(stack, connection, lineCount, datum);
                        return new ForthWordResult(ForthErrorResult.STACK_UNDERFLOW, "UNTIL but no previous BEGIN or FOR");
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
                        if (verbosity >= 2)
                            await DumpStackToDebugAsync(stack, connection, lineCount, datum, "(skipped)");
                        continue;
                    }
                }

                // Debug, print stack
                if (verbosity >= 2)
                    await DumpStackToDebugAsync(stack, connection, lineCount, datum);

                // Function calls
                if ((datum.Type == DatumType.Primitive || datum.Type == ForthDatum.DatumType.Unknown) &&
                    process.HasWord(datum.Value.ToString()))
                {
                    // Yield to other word.
                    var wordResult = await process.RunWordAsync(datum.Value.ToString(), trigger, command, lastListItem, process.EffectiveMuckerLevel, cancellationToken);
                    if (!wordResult.IsSuccessful)
                        return wordResult;
                    continue;
                }

                // Variables
                var variables = process.GetProgramLocalVariables()
                                .Union(functionScopedVariables)
                                .ToDictionary(k => k.Key, v => v.Value);

                if ((datum.Type == DatumType.Unknown || datum.Type == DatumType.Variable) &&
                    (string.Compare("me", datumLiteral, true) == 0
                    || string.Compare("here", datumLiteral, true) == 0
                    || string.Compare("loc", datumLiteral, true) == 0
                    || string.Compare("trigger", datumLiteral, true) == 0
                    || string.Compare("command", datumLiteral, true) == 0))
                {
                    stack.Push(new ForthDatum(datum.Value, DatumType.Variable, datum.FileLineNumber, datum.ColumnNumber, datum.WordName, datum.WordLineNumber));
                    continue;
                }

                if ((datum.Type == DatumType.Unknown || datum.Type == DatumType.Variable)
                    && variables.ContainsKey(datumLiteral))
                {
                    var v = variables[datumLiteral];
                    if (v.IsConstant)
                        stack.Push(new ForthDatum(v.Value, v.Type == VariableType.String ? DatumType.String : (v.Type == VariableType.Float ? DatumType.Float : (v.Type == VariableType.Integer ? DatumType.Integer : (v.Type == VariableType.DbRef ? DatumType.DbRef : DatumType.Unknown)))));
                    else
                        stack.Push(new ForthDatum(datum.Value, DatumType.Variable, datum.FileLineNumber, datum.ColumnNumber, datum.WordName, datum.WordLineNumber));
                    continue;
                }

                // Literals
                switch (datum.Type)
                {
                    case DatumType.Float:
                    case DatumType.Integer:
                    case DatumType.String:
                    case DatumType.Unknown:
                        stack.Push(datum);
                        continue;
                    case DatumType.DbRef:
                        // Correct type for built-in's.
                        if (((Dbref)datum.Value).ToInt32() == 0)
                            stack.Push(new ForthDatum(Dbref.AETHER, datum.FileLineNumber, datum.ColumnNumber, datum.WordName, datum.WordLineNumber));
                        else
                            stack.Push(datum);
                        continue;
                }

                // Primatives
                if (datum.Type == DatumType.Primitive)
                {
                    if (callTable.Keys.Contains(datumLiteral, StringComparer.InvariantCultureIgnoreCase))
                    {
                        var p = new ForthPrimativeParameters(process, stack, variables, connection, trigger, command,
                            async (d, s) => await Server.NotifyAsync(d, s),
                            async (d, s, e) => await Server.NotifyRoomAsync(d, s, e),
                            lastListItem,
                            cancellationToken);

                        var result = callTable.Single(c => string.Compare(c.Key, datumLiteral, true) == 0).Value.Invoke(p);
                        if (result.LastListItem.HasValue)
                            lastListItem = result.LastListItem.Value;

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
                                        return new ForthWordResult(ForthErrorResult.VARIABLE_IS_CONSTANT, $"Variable {dirty.Key} is a constant in this scope and cannot be changed.");

                                    programLocalVariables[dirty.Key] = dirty.Value;
                                }

                                if (functionScopedVariables.ContainsKey(dirty.Key))
                                    functionScopedVariables[dirty.Key] = dirty.Value;
                            }
                        }

                        if (ForthPrimativeResult.SUCCESS.Equals(result))
                            continue;
                        if (!result.IsSuccessful)
                            return new ForthWordResult((ForthErrorResult)result.Result, result.Reason);

                        continue;
                    }

                    // Unable to handle!
                    return new ForthWordResult(ForthErrorResult.SYNTAX_ERROR, $"Unable to handle datum primative: {datum}");
                }

                // Unable to handle!
                return new ForthWordResult(ForthErrorResult.INTERNAL_ERROR, $"Unable to handle datum: {datum}");
            }

            // Debug, print stack at end of program
            if (verbosity >= 1)
                await DumpStackToDebugAsync(stack, connection, lineCount);

            if (verbosity > 5)
                Console.WriteLine($"Word {name} completed. \tSTACK: {stack.Reverse().Select(x => x.Value?.ToString() ?? string.Empty).Aggregate((c, n) => c + "," + n)}");
            return new ForthWordResult($"Word {name} completed");
        }

        private static async Task DumpStackToDebugAsync(Stack<ForthDatum> stack, PlayerConnection connection, int lineCount, ForthDatum currentDatum = default, string? extra = null)
        {
            // Debug, print stack
            if (stack.Count == 0)
                await connection.sendOutput($"DEBUG ({lineCount}): () {extra}");
            else
                await connection.sendOutput($"DEBUG ({lineCount}): (" +
                stack.Reverse().Select(s =>
                {
                    return (s.Type == DatumType.String) ? $"\"{s.Value}\"" : s.Value.ToString();
                }).Aggregate((c, n) => c + " " + n) + ") " + (default(ForthDatum).Equals(currentDatum) ? "" : ((currentDatum.Type == DatumType.String) ? $"\"{currentDatum.Value}\"" : currentDatum.Value.ToString())) + " " + extra);
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
}
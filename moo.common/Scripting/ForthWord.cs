using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using moo.common.Models;
using moo.common.Scripting.ForthPrimatives;
using static moo.common.Scripting.ForthDatum;
using static moo.common.Scripting.ForthVariable;

namespace moo.common.Scripting
{
    public readonly struct ForthWord(string name, List<ForthDatum> programData, List<(string type, string name)> inputs, List<(string type, string name)> outputs)
    {
        private static readonly Dictionary<string, Func<ForthPrimativeParameters, ValueTask<ForthPrimativeResult>>> callTable = new(StringComparer.OrdinalIgnoreCase);
        public readonly string name = name ?? throw new ArgumentNullException(nameof(name));
        public readonly ImmutableArray<ForthDatum> programData = [.. programData];
        public readonly ImmutableList<(string type, string name)> inputs = [.. inputs];
        public readonly ImmutableList<(string type, string name)> outputs = [.. outputs];

        static ForthWord()
        {
            // Setup call table
            callTable.Add("pop", (p) => new ValueTask<ForthPrimativeResult>(Pop.Execute(p)));
            callTable.Add("popn", (p) => new ValueTask<ForthPrimativeResult>(PopN.Execute(p)));
            callTable.Add("dup", (p) =>
            {
                // DUP is the same as 1 pick.
                p.Stack.Push(new ForthDatum(1));
                return new ValueTask<ForthPrimativeResult>(Pick.Execute(p));
            });
            callTable.Add("dupn", (p) => new ValueTask<ForthPrimativeResult>(DupN.Execute(p)));
            callTable.Add("ldup", (p) => new ValueTask<ForthPrimativeResult>(LDup.Execute(p)));
            callTable.Add("swap", (p) => new ValueTask<ForthPrimativeResult>(Swap.Execute(p)));
            callTable.Add("over", (p) =>
            {
                // OVER is the same as 2 pick.
                p.Stack.Push(new ForthDatum(2));
                return new ValueTask<ForthPrimativeResult>(Pick.Execute(p));
            });
            callTable.Add("rot", (p) =>
            {
                // ROT is the same as 3 rotate
                p.Stack.Push(new ForthDatum(3));
                return new ValueTask<ForthPrimativeResult>(Rotate.Execute(p));
            });
            callTable.Add("rotate", (p) => new ValueTask<ForthPrimativeResult>(Rotate.Execute(p)));
            callTable.Add("pick", (p) => new ValueTask<ForthPrimativeResult>(Pick.Execute(p)));
            callTable.Add("put", (p) => new ValueTask<ForthPrimativeResult>(Put.Execute(p)));
            callTable.Add("reverse", (p) => new ValueTask<ForthPrimativeResult>(Reverse.Execute(p)));
            callTable.Add("lreverse", (p) => new ValueTask<ForthPrimativeResult>(LReverse.Execute(p)));
            callTable.Add("depth", (p) =>
            {
                // DEPTH ( -- i ) 
                // Returns the number of items currently on the stack.
                p.Stack.Push(new ForthDatum(p.Stack.Count));
                return new ValueTask<ForthPrimativeResult>(ForthPrimativeResult.SUCCESS);
            });
            callTable.Add("{", (p) =>
            {
                // { ( -- marker) 
                // Pushes a marker onto the stack, to be used with } or }list or }dict.
                p.Stack.Push(new ForthDatum("{", DatumType.Marker));
                return ValueTask.FromResult(ForthPrimativeResult.SUCCESS);
            });
            callTable.Add("}", (p) => new ValueTask<ForthPrimativeResult>(MarkerEnd.Execute(p)));
            callTable.Add("@", (p) => new ValueTask<ForthPrimativeResult>(At.Execute(p)));
            callTable.Add("!", (p) => new ValueTask<ForthPrimativeResult>(Bang.Execute(p)));
            callTable.Add("<", (p) => new ValueTask<ForthPrimativeResult>(OpLessThan.Execute(p)));
            callTable.Add(">", (p) => new ValueTask<ForthPrimativeResult>(OpGreaterThan.Execute(p)));
            callTable.Add("=", (p) => new ValueTask<ForthPrimativeResult>(OpEquals.Execute(p)));
            callTable.Add("<=", (p) => new ValueTask<ForthPrimativeResult>(OpLessThanOrEqual.Execute(p)));
            callTable.Add(">=", (p) => new ValueTask<ForthPrimativeResult>(OpGreaterThanOrEqual.Execute(p)));
            callTable.Add("not", (p) => new ValueTask<ForthPrimativeResult>(OpNot.Execute(p)));
            callTable.Add("and", (p) => new ValueTask<ForthPrimativeResult>(OpAnd.Execute(p)));
            callTable.Add("or", (p) => new ValueTask<ForthPrimativeResult>(OpOr.Execute(p)));
            callTable.Add("xor", (p) => new ValueTask<ForthPrimativeResult>(OpXor.Execute(p)));
            callTable.Add("string?", (p) => new ValueTask<ForthPrimativeResult>(OpIsString.Execute(p)));
            callTable.Add("int?", (p) => new ValueTask<ForthPrimativeResult>(OpIsInt.Execute(p)));
            callTable.Add("float?", (p) => new ValueTask<ForthPrimativeResult>(OpIsFloat.Execute(p)));
            callTable.Add("dbref?", (p) => new ValueTask<ForthPrimativeResult>(OpIsDbRef.Execute(p)));
            callTable.Add("array?", (p) => new ValueTask<ForthPrimativeResult>(OpIsArray.Execute(p)));

            // TODO ARRAY?
            callTable.Add("array_count", (p) => new ValueTask<ForthPrimativeResult>(ArrayCount.Execute(p)));
            callTable.Add("array_keys", (p) => new ValueTask<ForthPrimativeResult>(ArrayKeys.Execute(p)));
            callTable.Add("array_make", (p) => new ValueTask<ForthPrimativeResult>(ArrayMake.Execute(p)));
            callTable.Add("array_make_dict", (p) => new ValueTask<ForthPrimativeResult>(ArrayMakeDict.Execute(p)));
            callTable.Add("array_reverse", (p) => new ValueTask<ForthPrimativeResult>(ArrayReverse.Execute(p)));
            callTable.Add("array_union", (p) => new ValueTask<ForthPrimativeResult>(ArrayUnion.Execute(p)));
            callTable.Add("array_vals", (p) => new ValueTask<ForthPrimativeResult>(ArrayVals.Execute(p)));

            // TODO DICTIONARY
            // TODO ADDRESS?
            // TODO LOCK?

            // I/O OPERATORS
            callTable.Add("notify", (p) => new ValueTask<ForthPrimativeResult>(Notify.ExecuteAsync(p)));
            callTable.Add("notify_except", (p) => 
            {
                // NOTIFY_EXCEPT is the same as 1 swap notify_exclude
                p.Stack.Push(new ForthDatum(1));
                var swapResult = Swap.Execute(p);
                if (swapResult.IsSuccessful)
                    return new ValueTask<ForthPrimativeResult>(NotifyExclude.ExecuteAsync(p));
                else
                    return new ValueTask<ForthPrimativeResult>(swapResult);
            });
            callTable.Add("notify_exclude", (p) => new ValueTask<ForthPrimativeResult>(NotifyExclude.ExecuteAsync(p)));

            // MATHEMATICAL OPERATORS
            callTable.Add("abs", (p) => new ValueTask<ForthPrimativeResult>(Abs.Execute(p)));
            callTable.Add("int", (p) => new ValueTask<ForthPrimativeResult>(MathInt.Execute(p)));
            callTable.Add("sign", (p) => new ValueTask<ForthPrimativeResult>(Sign.Execute(p)));
            callTable.Add("getseed", (p) => new ValueTask<ForthPrimativeResult>(RandomMethods.GetSeed(p)));
            callTable.Add("setseed", (p) => new ValueTask<ForthPrimativeResult>(RandomMethods.SetSeed(p)));
            callTable.Add("srand", (p) => new ValueTask<ForthPrimativeResult>(RandomMethods.SRand(p)));
            callTable.Add("random", (p) => new ValueTask<ForthPrimativeResult>(RandomMethods.Random(p)));
            callTable.Add("bitor", (p) => new ValueTask<ForthPrimativeResult>(MathBitOr.Execute(p)));
            callTable.Add("bitxor", (p) => new ValueTask<ForthPrimativeResult>(MathBitXOr.Execute(p)));
            callTable.Add("bitand", (p) => new ValueTask<ForthPrimativeResult>(MathBitAnd.Execute(p)));
            callTable.Add("bitshift", (p) => new ValueTask<ForthPrimativeResult>(MathBitShift.Execute(p)));
            callTable.Add("+", (p) => new ValueTask<ForthPrimativeResult>(MathAdd.Execute(p)));
            callTable.Add("-", (p) => new ValueTask<ForthPrimativeResult>(MathSubtract.Execute(p)));
            callTable.Add("*", (p) => new ValueTask<ForthPrimativeResult>(MathMultiply.Execute(p)));
            callTable.Add("/", (p) => new ValueTask<ForthPrimativeResult>(MathDivide.Execute(p)));
            callTable.Add("%", (p) => new ValueTask<ForthPrimativeResult>(MathModulo.Execute(p)));

            // STRING MANIPULATION OPERATIONS
            callTable.Add("atoi", (p) => new ValueTask<ForthPrimativeResult>(AtoI.Execute(p)));
            callTable.Add("ctoi", (p) => new ValueTask<ForthPrimativeResult>(CtoI.Execute(p)));
            callTable.Add("strlen", (p) => new ValueTask<ForthPrimativeResult>(StrLen.Execute(p)));
            callTable.Add("strcat", (p) => new ValueTask<ForthPrimativeResult>(StrCat.Execute(p)));
            callTable.Add("strcmp", (p) => new ValueTask<ForthPrimativeResult>(StrCmp.Execute(p)));
            callTable.Add("strncmp", (p) => new ValueTask<ForthPrimativeResult>(StrNCmp.Execute(p)));
            callTable.Add("stringcmp", (p) => new ValueTask<ForthPrimativeResult>(StringCmp.Execute(p)));
            callTable.Add("stringpfx", (p) => new ValueTask<ForthPrimativeResult>(StringPfx.Execute(p)));
            callTable.Add("smatch", (p) => new ValueTask<ForthPrimativeResult>(SMatch.Execute(p)));
            callTable.Add("instr", (p) => new ValueTask<ForthPrimativeResult>(Instr.Execute(p)));
            callTable.Add("rinstr", (p) => new ValueTask<ForthPrimativeResult>(RInstr.Execute(p)));
            callTable.Add("strcut", (p) => new ValueTask<ForthPrimativeResult>(StrCut.Execute(p)));
            callTable.Add("midstr", (p) => new ValueTask<ForthPrimativeResult>(MidStr.Execute(p)));
            callTable.Add("split", (p) => new ValueTask<ForthPrimativeResult>(Split.Execute(p)));
            callTable.Add("rsplit", (p) => new ValueTask<ForthPrimativeResult>(RSplit.Execute(p)));

            callTable.Add("subst", (p) => new ValueTask<ForthPrimativeResult>(Subst.Execute(p)));

            callTable.Add("intostr", (p) => new ValueTask<ForthPrimativeResult>(IntoStr.Execute(p)));

            callTable.Add("toupper", (p) => new ValueTask<ForthPrimativeResult>(ToUpper.Execute(p)));
            callTable.Add("tolower", (p) => new ValueTask<ForthPrimativeResult>(ToLower.Execute(p)));
            callTable.Add("striplead", (p) => new ValueTask<ForthPrimativeResult>(StripLead.Execute(p)));
            callTable.Add("striptail", (p) => new ValueTask<ForthPrimativeResult>(StripTail.Execute(p)));

            callTable.Add("unparseobj", (p) => new ValueTask<ForthPrimativeResult>(UnparseObj.ExecuteAsync(p)));

            // PROPERTY MANIPULATION
            callTable.Add("getprop", (p) => new ValueTask<ForthPrimativeResult>(GetProp.ExecuteAsync(p)));
            callTable.Add("getpropstr", (p) => new ValueTask<ForthPrimativeResult>(GetPropStr.ExecuteAsync(p)));
            callTable.Add("getpropval", (p) => new ValueTask<ForthPrimativeResult>(GetPropVal.ExecuteAsync(p)));
            callTable.Add("getpropfval", (p) => new ValueTask<ForthPrimativeResult>(GetPropFVal.ExecuteAsync(p)));
            callTable.Add("addprop", (p) => new ValueTask<ForthPrimativeResult>(AddProp.ExecuteAsync(p)));
            callTable.Add("setprop", (p) => new ValueTask<ForthPrimativeResult>(SetProp.ExecuteAsync(p)));
            callTable.Add("array_get_reflist", (p) => new ValueTask<ForthPrimativeResult>(ArrayGetReflist.ExecuteAsync(p)));
            callTable.Add("array_put_reflist", (p) => new ValueTask<ForthPrimativeResult>(ArrayPutReflist.ExecuteAsync(p)));
            callTable.Add("array_getitem", (p) => new ValueTask<ForthPrimativeResult>(ArrayGetItem.ExecuteAsync(p)));

            // Database Related Operators
            callTable.Add("dbref", (p) => new ValueTask<ForthPrimativeResult>(DbrefConvert.Execute(p)));
            // TODO: PROG
            callTable.Add("trig", (p) => new ValueTask<ForthPrimativeResult>(Trig.Execute(p)));
            // TODO: CALLER
            // TODO: DBTOP
            callTable.Add("dbcmp", (p) => new ValueTask<ForthPrimativeResult>(DbCmp.Execute(p)));
            callTable.Add("owner", (p) => new ValueTask<ForthPrimativeResult>(Owner.ExecuteAsync(p)));
            callTable.Add("location", (p) => new ValueTask<ForthPrimativeResult>(Location.ExecuteAsync(p)));
            callTable.Add("contents", (p) => new ValueTask<ForthPrimativeResult>(Contents.ExecuteAsync(p)));
            callTable.Add("next", (p) => new ValueTask<ForthPrimativeResult>(Next.ExecuteAsync(p)));
            callTable.Add("match", (p) => new ValueTask<ForthPrimativeResult>(Match.ExecuteAsync(p)));
            callTable.Add("pmatch", (p) => new ValueTask<ForthPrimativeResult>(PMatch.Execute(p)));
            callTable.Add("part_pmatch", (p) => new ValueTask<ForthPrimativeResult>(PartPMatch.Execute(p)));
            callTable.Add("pennies", (p) => new ValueTask<ForthPrimativeResult>(Pennies.ExecuteAsync(p)));
            callTable.Add("flag?", (p) => new ValueTask<ForthPrimativeResult>(HasFlag.ExecuteAsync(p)));
            callTable.Add("ok?", (p) => new ValueTask<ForthPrimativeResult>(IsOk.ExecuteAsync(p)));
            callTable.Add("player?", (p) => new ValueTask<ForthPrimativeResult>(IsPlayer.ExecuteAsync(p)));
            callTable.Add("room?", (p) => new ValueTask<ForthPrimativeResult>(IsRoom.ExecuteAsync(p)));
            callTable.Add("thing?", (p) => new ValueTask<ForthPrimativeResult>(IsThing.ExecuteAsync(p)));
            callTable.Add("exit?", (p) => new ValueTask<ForthPrimativeResult>(IsExit.ExecuteAsync(p)));
            callTable.Add("program?", (p) => new ValueTask<ForthPrimativeResult>(IsProgram.ExecuteAsync(p)));
            callTable.Add("sysparm", (p) => new ValueTask<ForthPrimativeResult>(SysParm.Execute(p)));
            callTable.Add("name", (p) => new ValueTask<ForthPrimativeResult>(Name.ExecuteAsync(p)));
            callTable.Add("getlink", (p) => new ValueTask<ForthPrimativeResult>(GetLink.ExecuteAsync(p)));

            // TIME MANIPULATION
            callTable.Add("time", (p) => new ValueTask<ForthPrimativeResult>(Time.Execute(p)));
            callTable.Add("date", (p) => new ValueTask<ForthPrimativeResult>(Date.Execute(p)));
            callTable.Add("systime", (p) => new ValueTask<ForthPrimativeResult>(SysTime.Execute(p)));
            callTable.Add("systime_precise", (p) => new ValueTask<ForthPrimativeResult>(SysTimePrecise.Execute(p)));
            callTable.Add("gmtoffset", (p) => new ValueTask<ForthPrimativeResult>(GmtOffset.Execute(p)));
            callTable.Add("timesplit", (p) => new ValueTask<ForthPrimativeResult>(TimeSplit.Execute(p)));
            callTable.Add("timefmt", (p) => new ValueTask<ForthPrimativeResult>(TimeFormat.Execute(p)));

            // PROCESS MANAGEMENT OPERATORS
            callTable.Add("setmode", (p) => new ValueTask<ForthPrimativeResult>(SetMode.Execute(p)));

            // CONNECTION MANAGEMENT OPERATORS
            callTable.Add("awake?", (p) => new ValueTask<ForthPrimativeResult>(Awake.Execute(p)));
            callTable.Add("conidle", (p) => new ValueTask<ForthPrimativeResult>(ConIdle.Execute(p)));
            callTable.Add("descrcon", (p) => new ValueTask<ForthPrimativeResult>(DescRcon.Execute(p)));
            callTable.Add("descriptors", (p) => new ValueTask<ForthPrimativeResult>(Descriptors.Execute(p)));
            callTable.Add("online", (p) => new ValueTask<ForthPrimativeResult>(Online.Execute(p)));
            callTable.Add("online_array", (p) => new ValueTask<ForthPrimativeResult>(OnlineArray.Execute(p)));

            // MISCELLANEOUS
            callTable.Add("force", (p) => new ValueTask<ForthPrimativeResult>(Force.ExecuteAsync(p)));
            callTable.Add("version", (p) => new ValueTask<ForthPrimativeResult>(ForthPrimatives.Version.Execute(p)));
        }

        public static ICollection<string> GetPrimatives() => callTable.Keys;

        public async Task<ForthWordResult> RunAsync(
            ForthProcess process,
            Stack<ForthDatum> stack,
            Dbref player,
            Dbref location,
            Dbref trigger,
            string? command,
            Dbref? lastListItem,
            ILogger? logger, 
            CancellationToken cancellationToken)
        {
            // For each line
            var verbosity = 0;
            var lineCount = 0;
            var controlFlow = new Stack<ControlFlowMarker>();
            Dictionary<string, ForthVariable> functionScopedVariables = [];
            // For debugging only: string? lastPrimative = null;

            // Prepopulate inputs on stack per prototype, if defined.
            foreach (var input in inputs.Reverse())
            {
                var value = stack.Pop();
                functionScopedVariables.Add(input.name, new ForthVariable(value));
            }

            int x = -1;
            while (x < programData.Length - 1)
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
                {
                    if (stack.Count == 0)
                        logger?.LogTrace("DATUM: {value}",datum.Value);
                    else
                        Console.WriteLine($"DATUM: {datum.Value} \tSTACK: {stack.Reverse().Select(x => x.Value?.ToString() ?? string.Empty).Aggregate((c, n) => $"{c},{n}")}");
                }

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

                // For each element in line
                var datumLiteral = datum.Value?.ToString();

                // Do we have something unknown on the top of the stack?
                var topOfStack = stack.Count > 0 ? stack.Peek() : default;
                if (stack.Count > 0 && topOfStack.Type == DatumType.Unknown)
                    return new ForthWordResult(ForthErrorResult.SYNTAX_ERROR, $"Unable to handle datum on top of stack: {topOfStack}({topOfStack.FileLineNumber},{topOfStack.ColumnNumber})");

                // Execution Control
                if (datum.Type == DatumType.Unknown)
                {
                    // VAR
                    if (string.Compare("var", datumLiteral, true) == 0)
                    {
                        if (x + 1 >= programData.Length)
                            return new ForthWordResult(ForthErrorResult.SYNTAX_ERROR, "var must be followed by a variable name");

                        var rawName = programData[x + 1].Value?.ToString();
                        if (string.IsNullOrWhiteSpace(rawName))
                            return new ForthWordResult(ForthErrorResult.SYNTAX_ERROR, "var name must be a non-empty identifier");

                        functionScopedVariables[rawName.ToLowerInvariant()] = UNINITIALIZED;
                        x++; // Advance past the variable name since it's ahead.
                        continue;
                    }

                    // VAR!
                    if (string.Compare("var!", datumLiteral, true) == 0)
                    {
                        if (x + 1 >= programData.Length)
                            return new ForthWordResult(ForthErrorResult.SYNTAX_ERROR, "var! must be followed by a variable name");
                        if (stack.Count < 1)
                            return new ForthWordResult(ForthErrorResult.STACK_UNDERFLOW, "var! requires one value on the stack");

                        var rawName = programData[x + 1].Value?.ToString();
                        if (string.IsNullOrWhiteSpace(rawName))
                            return new ForthWordResult(ForthErrorResult.SYNTAX_ERROR, "var! name must be a non-empty identifier");

                        var functionScopedVariableValue = stack.Pop();
                        functionScopedVariables[rawName.ToLowerInvariant()] = new ForthVariable(functionScopedVariableValue);
                        x++; // Advance past the variable name since it's ahead.
                        continue;
                    }

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
                                    await DumpStackToDebugAsync(stack, player, lineCount, datum, "(skipped)");
                                controlFlow.Push(new ControlFlowMarker(ControlFlowElement.SkippedBranch, x));
                                continue;
                            }
                        }

                        // Debug, print stack
                        if (verbosity >= 2) await DumpStackToDebugAsync(stack, player, lineCount, datum);

                        if (stack.Count == 0)
                        {
                            if (verbosity >= 2)
                                await DumpVariablesToDebugAsync(process, player, functionScopedVariables);
                            return new ForthWordResult(ForthErrorResult.STACK_UNDERFLOW, "IF had no value on the stack to evaluate");
                        }

                        var eval = stack.Pop();
                        if (eval.IsTrue())
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
                                    await DumpStackToDebugAsync(stack, player, lineCount, datum, "(skipped)");
                                continue;
                            }
                        }

                        // Debug, print stack
                        if (verbosity >= 2)
                            await DumpStackToDebugAsync(stack, player, lineCount, datum);

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
                                    await DumpStackToDebugAsync(stack, player, lineCount, datum, "(skipped)");
                                // A skipped if will push a SkippedBranch, so we should pop it.
                                controlFlow.Pop();
                                continue;
                            }
                        }

                        // Debug, print stack
                        if (verbosity >= 2) await DumpStackToDebugAsync(stack, player, lineCount, datum);

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
                                    await DumpStackToDebugAsync(stack, player, lineCount, datum, "(skipped)");
                                continue;
                            }
                        }

                        // Debug, print stack
                        if (verbosity >= 2)
                            await DumpStackToDebugAsync(stack, player, lineCount, datum);

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
                                    await DumpStackToDebugAsync(stack, player, lineCount, datum, "(skipped)");
                                continue;
                            }
                        }

                        // Debug, print stack
                        if (verbosity >= 2)
                            await DumpStackToDebugAsync(stack, player, lineCount, datum);

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
                                    await DumpStackToDebugAsync(stack, player, lineCount, datum, "(skipped)");
                                continue;
                            }
                        }

                        if (verbosity >= 2)
                            await DumpStackToDebugAsync(stack, player, lineCount, datum);

                        if (stack.Count == 0)
                        {
                            return new ForthWordResult(ForthErrorResult.STACK_UNDERFLOW, "WHILE had no value on the stack to evaluate");
                        }

                        var eval = stack.Pop();
                        if (eval.IsFalse())
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
                                    await DumpStackToDebugAsync(stack, player, lineCount, datum, "(skipped)");
                                continue;
                            }
                        }

                        //await DumpStackToDebugAsync(stack, connection, lineCount, datum);
                        controlFlow.Push(new ControlFlowMarker(ControlFlowElement.SkipToAfterNextUntilOrRepeat, x));
                        continue;
                    }

                    // REPEAT
                    if (string.Compare("repeat", datumLiteral, true) == 0)
                    {
                        if (verbosity >= 2)
                            await DumpStackToDebugAsync(stack, player, lineCount, datum);

                        if (controlFlow.Count == 0)
                            return new ForthWordResult(ForthErrorResult.STACK_UNDERFLOW, "REPEAT but no previous BEGIN, FOR, or FOREACH on the stack");

                        // I could be a 'repeat' inside a skipped branch.
                        var controlCurrent = controlFlow.Peek();
                        if (controlCurrent.Element == ControlFlowElement.InIfAndSkip
                         || controlCurrent.Element == ControlFlowElement.InElseAndSkip
                         || controlCurrent.Element == ControlFlowElement.SkippedBranch)
                        {
                            if (verbosity >= 2)
                                await DumpStackToDebugAsync(stack, player, lineCount, datum, "(skipped)");
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
                                break;
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
                            await DumpStackToDebugAsync(stack, player, lineCount, datum);
                            controlFlow.Pop(); // Pop the skip
                            controlFlow.Pop(); // Pop the opening begin
                            continue;
                        }

                        if (controlCurrent.Element == ControlFlowElement.InIfAndSkip || controlCurrent.Element == ControlFlowElement.InElseAndSkip || controlCurrent.Element == ControlFlowElement.SkippedBranch)
                        {
                            if (verbosity >= 2)
                                await DumpStackToDebugAsync(stack, player, lineCount, datum, "(skipped)");
                            continue;
                        }

                        // Debug, print stack
                        await DumpStackToDebugAsync(stack, player, lineCount, datum);

                        if (stack.Count == 0)
                        {
                            if (verbosity >= 2)
                                await DumpStackToDebugAsync(stack, player, lineCount, datum);
                            return new ForthWordResult(ForthErrorResult.STACK_UNDERFLOW, "UNTIL had no value on the stack to evaluate");
                        }

                        var eval = stack.Pop();
                        if (eval.IsTrue())
                            continue;

                        // Go back to previous BEGIN or FOR
                        var found = false;
                        while (controlFlow.Count > 0)
                        {
                            var nextControl = controlFlow.Pop();
                            if (nextControl.Element == ControlFlowElement.BeginMarker
                             || nextControl.Element == ControlFlowElement.ForMarker)
                            {
                                x = nextControl.Index - 1;
                                found = true;
                                break;
                            }
                        }

                        if (found)
                            continue;

                        if (verbosity >= 2)
                            await DumpStackToDebugAsync(stack, player, lineCount, datum);
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
                            await DumpStackToDebugAsync(stack, player, lineCount, datum, "(skipped)");
                        continue;
                    }
                }

                // Debug, print stack
                if (verbosity >= 2)
                    await DumpStackToDebugAsync(stack, player, lineCount, datum);

                // Function calls
                if ((datum.Type == DatumType.Primitive || datum.Type == DatumType.Unknown)
                     && datum != default
                     && datum.Value != null
                     && process.HasWord(datum.Value.ToString()))
                {
                    // Yield to other word.
                    var wordResult = await process.RunWordAsync(datum.Value.ToString()!, lastListItem, process.EffectiveMuckerLevel, logger, cancellationToken);
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
                    //|| string.Compare("here", datumLiteral, true) == 0
                    || string.Compare("loc", datumLiteral, true) == 0
                    || string.Compare("trigger", datumLiteral, true) == 0
                    || string.Compare("command", datumLiteral, true) == 0))
                {
                    stack.Push(new ForthDatum(datum.Value, DatumType.Variable, datum.FileLineNumber, datum.ColumnNumber, datum.WordName, datum.WordLineNumber));
                    continue;
                }

                if ((datum.Type == DatumType.Unknown || datum.Type == DatumType.Variable)
                    && variables.TryGetValue(datumLiteral?.ToLowerInvariant() ?? string.Empty, out ForthVariable v))
                {
                    if (v.IsConstant)
                    {
                        var datumType = v.Type switch
                        {
                            VariableType.String  => DatumType.String,
                            VariableType.Float   => DatumType.Float,
                            VariableType.Integer => DatumType.Integer,
                            VariableType.DbRef   => DatumType.DbRef,
                            VariableType.Array   => DatumType.Array,
                            _                    => DatumType.Unknown,
                        };
                        stack.Push(new ForthDatum(v.Value, datumType));
                    }
                    else
                        stack.Push(new ForthDatum(datum.Value, DatumType.Variable, datum.FileLineNumber, datum.ColumnNumber, datum.WordName, datum.WordLineNumber));
                    continue;
                }

                // An Unknown datum that wasn't recognized as a control-flow
                // keyword, callable word, built-in, or variable is a parse-time
                // identifier the runtime cannot resolve. Silently pushing it as
                // a literal hides the error; reject it here so the caller sees
                // a clean SYNTAX_ERROR instead of either a successful no-op or
                // a confusing failure on a later instruction.
                if (datum.Type == DatumType.Unknown)
                    return new ForthWordResult(ForthErrorResult.SYNTAX_ERROR, $"Unknown identifier: {datumLiteral}");

                // Literals
                switch (datum.Type)
                {
                    case DatumType.Float:
                    case DatumType.Integer:
                    case DatumType.String:
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
                    if (datumLiteral != null && callTable.TryGetValue(datumLiteral, out var matchingPrimative))
                    {
                        // For debugging only: var stackCopy = stack.ClonePreservingOrder();
                        var p = new ForthPrimativeParameters(process, stack, variables, player, location, trigger, command,
                            async (d, s) => await Server.NotifyAsync(d, s),
                            async (d, s, e) => await Server.NotifyRoomAsync(d, s, e),
                            lastListItem,
                            logger,
                            cancellationToken);

                        var result = await matchingPrimative.Invoke(p);

                        // For debugging only: lastPrimative = datumLiteral;

                        if (result.LastListItem.HasValue)
                            lastListItem = result.LastListItem.Value;

                        // Push dirty variables where they may need to go.
                        if (result.dirtyVariables != null && result.dirtyVariables.Count > 0)
                        {
                            var programLocalVariables = process.GetProgramLocalVariables();
                            foreach (var dirty in result.dirtyVariables)
                            {
                                var key = dirty.Key; // already lowercase from Bang
                                if (programLocalVariables.TryGetValue(key, out var vv))
                                {
                                    if (vv.IsConstant)
                                        return new ForthWordResult(ForthErrorResult.VARIABLE_IS_CONSTANT, $"Variable {key} is a constant in this scope and cannot be changed.");

                                    programLocalVariables[key] = dirty.Value;
                                }

                                if (functionScopedVariables.TryGetValue(key, out var fv))
                                {
                                    if (fv.IsConstant)
                                        return new ForthWordResult(ForthErrorResult.VARIABLE_IS_CONSTANT,  $"Variable {key} is a constant in this scope and cannot be changed.");

                                    functionScopedVariables[key] = dirty.Value;
                                }
                            }
                        }

                        if (ForthPrimativeResult.SUCCESS.Equals(result))
                            continue;
                        if (!result.IsSuccessful)
                        {
                            var fer = result.Result as ForthErrorResult?;
                            if (fer != null)
                                return new ForthWordResult((ForthErrorResult)fer!, result.Reason ?? "UNKNOWN ERROR");
                            else
                                return new ForthWordResult("UNKNOWN ERROR");
                        }

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
                await DumpStackToDebugAsync(stack, player, lineCount);

            if (verbosity > 5)
                Console.WriteLine($"Word {name} completed.");

            return new ForthWordResult($"Word {name} completed");
        }

        private static async Task DumpStackToDebugAsync(Stack<ForthDatum> stack, Dbref player, int lineCount, ForthDatum currentDatum = default, string? extra = null)
        {
            // Debug, print stack
            if (stack.Count == 0)
                await Server.NotifyAsync(player, $"DEBUG ({lineCount}): () {extra}");
            else
                await Server.NotifyAsync(player, $"DEBUG ({lineCount}): (" +
                stack.Reverse().Select(s =>
                {
                    return (s.Type == DatumType.String) ? $"\"{s.Value}\"" : (s.Value?.ToString() ?? "(null)");
                }).Aggregate((c, n) => $"{c} {n}") + ") " + (default(ForthDatum).Equals(currentDatum) ? "" : ((currentDatum.Type == DatumType.String) ? $"\"{currentDatum.Value}\"" : currentDatum.Value.ToString())) + " " + extra);
        }
        private static async Task DumpVariablesToDebugAsync(
            ForthProcess process,
           Dbref player,
            Dictionary<string, ForthVariable> functionScopedVariables)
        {
            foreach (var lvar in process.GetProgramLocalVariables())
            {
                await Server.NotifyAsync(player, $"LVAR {lvar.Key}={lvar.Value}");
            }

            foreach (var local in functionScopedVariables)
            {
                await Server.NotifyAsync(player, $"VAR {local.Key}={local.Value}");
            }
        }
    }
}
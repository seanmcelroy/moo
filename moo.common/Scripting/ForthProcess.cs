using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using moo.common.Connections;
using moo.common.Models;
using moo.common.Scripting.ForthPrimatives;

namespace moo.common.Scripting
{
    public class ForthProcess
    {
        public enum MultitaskingMode
        {
            Preempt = 0,
            Foreground = 1,
            Background = 2
        }

        public enum ProcessState
        {
            Initializing,
            Parsing,
            Running,
            WaitingForInput,
            Pausing,
            Paused,
            Preempting,
            Preempted,
            RunningPreempt,
            Complete
        }

        private static int nextPid = 1;

        private readonly int processId;
        private MultitaskingMode mode;
        public ProcessState State;
        private IEnumerable<ForthWord> words;
        private readonly Stack<ForthDatum> stack = new();
        // Variables that came from a caller program
        private readonly Dictionary<string, ForthVariable> outerScopeVariables = new();
        // Variables local to my process context, which I could pass on to programs I call and all my words can see ($DEF, LVAR)
        private readonly ConcurrentDictionary<string, ForthVariable> programLocalVariables = new();

        private readonly Dbref player;
        private readonly Dbref location;
        private readonly Dbref trigger;
        private readonly string? command;
        private readonly byte effectiveMuckerLevel;
        private readonly string scopeId;
        private readonly string outerScopeId;
        private bool hasRan;

        public int ProcessId => processId;
        public MultitaskingMode Mode => mode;

        public byte EffectiveMuckerLevel => effectiveMuckerLevel;

        public ForthProcess(
            Dbref player,
            Dbref location,
            Dbref trigger,
            string? command,
            byte effectiveMuckerLevel,
            string? outerScopeId = null,
            Dictionary<string, ForthVariable>? outerScopeVariables = null)
        {
            this.player = player;
            this.location = location;
            this.trigger = trigger;
            this.command = command;
            processId = GetNextPid();
            State = ProcessState.Initializing;
            mode = MultitaskingMode.Foreground;
            this.effectiveMuckerLevel = effectiveMuckerLevel;
            scopeId = Guid.NewGuid().ToString();

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

        public static int GetNextPid()
        {
            var pid = Interlocked.Increment(ref nextPid);
            if (pid > 0 && pid < int.MaxValue)
                return pid;
            Interlocked.CompareExchange(ref nextPid, 0, pid);

            return 1;
        }

        public ConcurrentDictionary<string, ForthVariable> GetProgramLocalVariables()
        {
            return this.programLocalVariables;
        }

        // Exposed for tests so end-to-end programs can be inspected after RunAsync.
        public Stack<ForthDatum> Stack => stack;

        public void SetProgramLocalVariable(string name, ForthVariable value) => programLocalVariables[name.ToLowerInvariant()] = value;

        public bool HasWord(string? wordName) => !string.IsNullOrWhiteSpace(wordName) && words.Any(w => string.Compare(w.name, wordName, true) == 0);

        internal async Task<ForthWordResult> RunWordAsync(
            string wordName,
            Dbref? lastListItem,
            byte effectiveMuckerLevel,
            ILogger? logger,
             CancellationToken cancellationToken)
        {
            var match = words.FirstOrDefault(w => string.Compare(w.name, wordName, true) == 0);
            if (default(ForthWord).Equals(match))
                return new ForthWordResult(ForthErrorResult.SYNTAX_ERROR, $"No such word: {wordName}");
            return await match.RunAsync(this, stack, player, location, trigger, command, lastListItem, logger, cancellationToken);
        }

        public void Pause()
        {
            if (this.State == ProcessState.Running)
                this.State = ProcessState.Pausing;
        }

        public void Background()
        {
            this.mode = MultitaskingMode.Background;
        }

        public void Foreground()
        {
            if (this.Mode == MultitaskingMode.Background)
            {
                // Programs cannot be moved out of background mode.
                return;
            }

            this.mode = MultitaskingMode.Foreground;
        }

        public void Preempt()
        {
            if (this.Mode == MultitaskingMode.Background)
            {
                // Programs cannot be moved out of background mode.
                return;
            }

            if (this.State == ProcessState.Running)
                this.State = ProcessState.Preempting;
        }

        public async Task<ForthWordResult> RunAsync(
            IEnumerable<ForthWord> words,
            Dbref trigger,
            string command,
            object[] args,
            ILogger? logger,
            CancellationToken cancellationToken)
        {
            if (hasRan)
            {
                return new ForthWordResult(ForthErrorResult.INTERNAL_ERROR, $"Execution scope {scopeId} tried to run twice.");
            }
            hasRan = true;

            this.words = words;

            // Execute the last word.
            if (args != null && args.Length > 0 && args[0] != null)
            {
                if (args[0].GetType() == typeof(string))
                    stack.Push(new ForthDatum((string)args[0]));
            }

            if (args != null)
            {
                foreach (var arg in args)
                {
                    if (arg != null) 
                    {
                        switch (arg)
                        {
                            case string s:
                                stack.Push(new ForthDatum(s));
                                break;
                            case int i:
                                stack.Push(new ForthDatum(i));
                                break;
                            case float f:
                                stack.Push(new ForthDatum(f));
                                break;
                            case Dbref d:
                                stack.Push(new ForthDatum(d));
                                break;
                            case ForthDatum dat:
                                stack.Push(dat);
                                break;
                            default:
                                return new ForthWordResult(ForthErrorResult.TYPE_MISMATCH, $"Unsupported arg type {arg.GetType().Name}");
                        }
                    }
                }
            }

            State = ProcessState.Running;
            var result = await words.Last().RunAsync(this, stack, player, location, trigger, command, null, logger, cancellationToken);
            State = ProcessState.Complete;

            if (Server.GetInstance().PreemptProcessId == this.processId)
                Server.GetInstance().PreemptProcessId = 0;

            return result;
        }

        public void Unpaused()
        {
            this.State = ProcessState.Running;
        }
    }
}
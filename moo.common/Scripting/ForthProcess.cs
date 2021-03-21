using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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
        private readonly static object nextPidLock = new();

        private readonly int processId;
        private MultitaskingMode mode;
        public ProcessState State;
        private IEnumerable<ForthWord> words;
        private readonly Stack<ForthDatum> stack = new();
        // Variables that came from a caller program
        private readonly Dictionary<string, ForthVariable> outerScopeVariables = new();
        // Variables local to my process context, which I could pass on to programs I call and all my words can see ($DEF, LVAR)
        private readonly ConcurrentDictionary<string, ForthVariable> programLocalVariables = new();

        private readonly Dbref scriptId;
        private readonly byte effectiveMuckerLevel;
        private readonly PlayerConnection connection;
        private readonly string scopeId;
        private readonly string outerScopeId;
        private bool hasRan;

        public int ProcessId => processId;
        public MultitaskingMode Mode => mode;

        public byte EffectiveMuckerLevel => effectiveMuckerLevel;

        public ForthProcess(
            Dbref scriptId,
            byte effectiveMuckerLevel,
            PlayerConnection connection,
            string? outerScopeId = null,
            Dictionary<string, ForthVariable>? outerScopeVariables = null)
        {
            this.processId = GetNextPid();
            this.State = ProcessState.Initializing;
            this.mode = MultitaskingMode.Foreground;
            this.scriptId = scriptId;
            this.effectiveMuckerLevel = effectiveMuckerLevel;
            this.connection = connection;
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

        public static int GetNextPid()
        {
            lock (nextPidLock)
            {
                var pid = Interlocked.Increment(ref nextPid);
                if (pid < Int32.MaxValue)
                    return pid;
                Interlocked.Exchange(ref nextPid, 1);
                return 1;
            }
        }

        public ConcurrentDictionary<string, ForthVariable> GetProgramLocalVariables()
        {
            return this.programLocalVariables;
        }

        public void SetProgramLocalVariable(string name, ForthVariable value)
        {
            if (!programLocalVariables.TryAdd(name, value))
            {
                programLocalVariables[name] = value;
            }
        }

        public bool HasWord(string? wordName) => !string.IsNullOrWhiteSpace(wordName) && words.Any(w => string.Compare(w.name, wordName, true) == 0);

        public async Task<ForthWordResult> RunWordAsync(string wordName, Dbref trigger,
            string command, Dbref? lastListItem,
            byte effectiveMuckerLevel,
             CancellationToken cancellationToken)
        {
            return await this.words
                .Single(w => string.Compare(w.name, wordName, true) == 0)
                .RunAsync(this, stack, connection, trigger, command, lastListItem, cancellationToken);
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

            State = ProcessState.Running;
            var result = await words.Last().RunAsync(this, stack, connection, trigger, command, null, cancellationToken);
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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using moo.common.Database;
using moo.common.Models;
using moo.common.Scripting;

namespace moo.common.Connections
{
    public abstract class PlayerConnection
    {
        private static int nextConnectorDescriptor;
        private readonly HumanPlayer player;
        private readonly int connectorDescriptor;
        private readonly DateTime connectionTime;
        private DateTime? lastInput;
        private readonly StringBuilder buffer = new();
        private readonly object bufferLock = new();

        public Dbref Dbref => player.id;

        public string? Name => player.name;

        public Dbref Location => player.Location;

        public int ConnectorDescriptor => connectorDescriptor;

        public DateTime ConnectionTime => connectionTime;

        public DateTime? LastInput => lastInput;

        public bool Unattended => unattended;

        private Editor? editor;
        private string? editorTag;
        private Action<string>? onEditorModeExit;
        private bool unattended;

        public bool IsIdle => editor == null && buffer.Length == 0 && !unattended;

        protected PlayerConnection(HumanPlayer player)
        {
            this.player = player;
            var next = Interlocked.Increment(ref nextConnectorDescriptor);
            if (next > int.MaxValue - 100000)
            {
                nextConnectorDescriptor = 1;
                next = 1;
            }
            this.connectorDescriptor = next;
            this.connectionTime = DateTime.Now;
        }

        public abstract Task sendOutput(string output);

        //public async Task<Dbref> MatchAsync(string name, CancellationToken cancellationToken) => await this.player.MatchAsync(name, cancellationToken);

        public async Task MoveToAsync(Thing target, CancellationToken cancellationToken) => await this.player.MoveToAsync(target, cancellationToken);
        public async Task MoveToAsync(Dbref targetId, CancellationToken cancellationToken) => await this.player.MoveToAsync(targetId, cancellationToken);

        public void ReceiveInput(string line)
        {
            lastInput = DateTime.Now;
            buffer.AppendLine(line);
        }

        public void ReceiveInputUnattended(IEnumerable<string> lines)
        {
            unattended = true;
            lastInput = DateTime.Now;
            buffer.AppendLine(lines.Aggregate((c, n) => $"{c}\n{n}"));

            while (buffer.Length > 0)
            {
                Thread.Yield();
                Thread.Sleep(100);

                if (buffer.Length == 1 && buffer.ToString()[0] == '\n')
                    buffer.Remove(0, 1);
                else if (buffer.Length == 2 && buffer.ToString()[0] == '\r' && buffer.ToString()[1] == '\n')
                    buffer.Remove(0, 2);
            }

            unattended = false;
        }

        private CommandResult PopCommand()
        {
            lock (bufferLock)
            {
                while (buffer.Length > 0 && buffer[0] == '\n')
                    buffer.Remove(0, 1);

                if (buffer.Length < 2)
                    return default;

                var bufferString = buffer.ToString();
                int firstBreak = bufferString.IndexOfAny(new[] { '\r', '\n' });
                if (firstBreak < 1) // None, or zero.
                    return default;

                var raw = bufferString.Substring(0, firstBreak);
                buffer.Remove(0, raw.Length + "\n".Length);

                if (raw.Length == 0)
                    return default;

                return new CommandResult(raw);
            }
        }

        public void EnterEditMode(Script? script, string tag, Action<string> onEditorModeExit)
        {
            this.onEditorModeExit = onEditorModeExit;
            this.editor = new Editor(this, script);
            this.editorTag = tag;
        }

        public Player GetPlayer() => this.player;

        public bool HasFlag(Thing.Flag flag) => this.player.HasFlag(flag);

        public async Task<Dbref> FindThingForThisPlayerAsync(string s, CancellationToken cancellationToken) => await this.player.FindThingForThisPlayerAsync(s, cancellationToken);

        public void SetPropertyPathValue(string path, Dbref value) => this.player.SetPropertyPathValue(path, value);

        public async Task RunNextCommand(IEnumerable<IRunnable> globalActions, CancellationToken cancellationToken)
        {
            var command = PopCommand();

            if (default(CommandResult).Equals(command) || command.raw.Length == 0)
            {
                Thread.Sleep(200);
                return;
            }

            if (editor != null)
            {
                var editorResult = await editor.HandleInputAsync(command.raw, cancellationToken);

                if (!editorResult.IsSuccessful)
                    await sendOutput($"ERROR: {editorTag}: {editorResult.Reason}");

                if (editorResult.ShouldExit)
                {
                    if (onEditorModeExit != null && editor.ProgramText != null)
                        onEditorModeExit.Invoke(editor.ProgramText);
                    editor = null;
                    onEditorModeExit = null;
                }

                return;
            }

            //if (Unattended)
            //    await sendOutput($"AUTO> {command.raw}");

            // Global actions
            VerbResult actionResult;
            foreach (var action in globalActions)
            {
                if (action.CanProcess(this, command).Item1)
                {
                    // TODO: Right now we block on programs
                    actionResult = await action.Process(this, command, cancellationToken);
                    if (!actionResult.isSuccess)
                    {
                        await sendOutput($"ERROR: {actionResult.reason}");
                        return;
                    }

                    return;
                }
            }

            var matchResult = await Matcher
                .InitObjectSearch(this.GetPlayer(), command.getVerb(), Dbref.DbrefObjectType.Unknown, cancellationToken)
                .MatchEverything()
                .Result();

            // Todo, everything below here needs to be checked against Fuzzball
            if (matchResult.IsValid())
            {
                var matchedLookup = await ThingRepository.Instance.GetAsync(matchResult, cancellationToken);
                if (!matchedLookup.isSuccess || matchedLookup.value == null)
                {
                    await sendOutput($"Cannot retrieve {matchResult}: {matchedLookup.reason}");
                    return;
                }

                if (matchResult.Type == Dbref.DbrefObjectType.Exit)
                {
                    var exit = (Exit)matchedLookup.value!;
                    if (!exit.CanProcess(this, command).Item1)
                    {
                        await sendOutput($"Locked.");
                        return;
                    }

                    await exit.Process(this, command, cancellationToken);
                    return;
                }

                await sendOutput($"I don't know how to process {matchedLookup.value.UnparseObject()}");
            }

            if (command.raw.StartsWith("@"))
                Console.WriteLine($"Unknown at-command: {command.raw}");

            await sendOutput("Huh?");
            actionResult = new VerbResult(false, $"Command not found for verb {command.getVerb()}");
        }
    }
}
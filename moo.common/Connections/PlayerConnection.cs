using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
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

        public abstract Task SendOutput(string output);

        //public async Task<Dbref> MatchAsync(string name, CancellationToken cancellationToken) => await this.player.MatchAsync(name, cancellationToken);

        public void ReceiveInput(string line)
        {
            lastInput = DateTime.Now;
            lock (bufferLock)
            {
                buffer.AppendLine(line);
            }
        }

        public void ReceiveInputUnattended(IEnumerable<string> lines)
        {
            unattended = true;
            lastInput = DateTime.Now;
            lock (bufferLock)
            {
                buffer.AppendLine(
                        lines
                            .Where(l => !(l.StartsWith('(') && l.EndsWith(')')))
                            .Aggregate((c, n) => $"{c}\n{n}"));
            }

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
                while (buffer.Length > 1 && buffer[0] == '\r' && buffer[1] == '\n')
                    buffer.Remove(0, 2);

                if (buffer.Length < 2)
                    return default;

                var bufferString = buffer.ToString();

                int firstBreak = bufferString.IndexOfAny(new[] { '\r', '\n' });
                var breakChar = bufferString[firstBreak];
                if (firstBreak < 1) // None, or zero.
                    return default;

                var raw = bufferString.Substring(0, firstBreak);
                switch (breakChar)
                {
                    case '\n':
                        buffer.Remove(0, raw.Length + 1);
                        break;
                    case '\r':
                        buffer.Remove(0, raw.Length + 1);
                        if (buffer.Length > 0 && buffer[0] == '\n')
                            buffer.Remove(0, 1);
                        break;
                }

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

        public async Task RunNextCommand(CancellationToken cancellationToken)
        {
            var command = PopCommand();

            if (default(CommandResult).Equals(command) || command.Raw.Length == 0)
            {
                Thread.Sleep(200);
                return;
            }

            if (editor != null)
            {
                var editorResult = await editor.HandleInputAsync(command.Raw, cancellationToken);

                if (!editorResult.IsSuccessful)
                    await SendOutput($"ERROR: {editorTag}: {editorResult.Reason}");

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

            await Server.RunCommand(this.Dbref, this, command, cancellationToken);
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

public abstract class PlayerConnection
{
    private static int nextConnectorDescriptor;

    private HumanPlayer player;
    private int connectorDescriptor;
    private DateTime connectionTime;
    private DateTime? lastInput;
    private readonly StringBuilder buffer = new StringBuilder();

    public Dbref Dbref => player.id;

    public string Name => player.name;

    public Dbref Location => player.location;

    public int ConnectorDescriptor => connectorDescriptor;

    public DateTime ConnectionTime => connectionTime;

    public DateTime? LastInput => lastInput;

    private Editor editor;
    private string editorTag;
    private Action<string> onEditorModeExit;

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

    public async Task<Dbref> MatchAsync(string name, CancellationToken cancellationToken) => await this.player.MatchAsync(name, cancellationToken);

    public async Task MoveToAsync(Container target, CancellationToken cancellationToken) => await this.player.MoveToAsync(target, cancellationToken);
    public async Task MoveToAsync(Dbref targetId, CancellationToken cancellationToken) => await this.player.MoveToAsync(targetId, cancellationToken);

    public void ReceiveInput(string line)
    {
        lastInput = DateTime.Now;
        buffer.AppendLine(line);
    }

    public void ReceiveInput(IEnumerable<string> lines)
    {
        lastInput = DateTime.Now;
        buffer.AppendLine(lines.Aggregate((c, n) => $"{c}\n{n}"));
    }

    private CommandResult PopCommand()
    {
        if (buffer.Length < 2)
            return default(CommandResult);

        var bufferString = buffer.ToString();
        int firstBreak = bufferString.IndexOf("\n");
        if (firstBreak == -1)
            return default(CommandResult);

        var raw = bufferString.Substring(0, firstBreak);
        buffer.Remove(0, raw.Length + "\n".Length);
        return new CommandResult(raw);
    }

    public void EnterEditMode(string tag, Action<string> onEditorModeExit)
    {
        this.onEditorModeExit = onEditorModeExit;
        this.editor = new Editor();
        this.editorTag = tag;
    }

    public Player GetPlayer() => this.player;

    public async Task RunNextCommand(IEnumerable<IRunnable> globalActions, CancellationToken cancellationToken)
    {
        var command = PopCommand();

        if (default(CommandResult).Equals(command))
        {
            Thread.Sleep(500);
            return;
        }

        if (editor != null)
        {
            var editorResult = await editor.HandleInput(command.raw);

            if (!editorResult.IsSuccessful)
                await sendOutput($"ERROR: {editorTag}: {editorResult.Reason}");

            if (editorResult.ShouldExit)
            {
                onEditorModeExit.Invoke(editor.ProgramText);
                editor = null;
                onEditorModeExit = null;

                await sendOutput(command.raw);
            }

            return;
        }

        // Exits in current location
        var locationLookup = await ThingRepository.GetAsync<Container>(this.Location, cancellationToken);
        if (locationLookup.isSuccess)
        {
            var location = locationLookup.value;
            var matched = await location.MatchAsync(command.getVerb(), cancellationToken);
            if (matched.ToInt32() >= 0)
            {
                var matchedLookup = await ThingRepository.GetAsync(matched, cancellationToken);
                if (!matchedLookup.isSuccess)
                {
                    await sendOutput($"Cannot retrieve {matched}: {matchedLookup.reason}");
                    return;
                }

                var matchedObject = matchedLookup.value;
                if (matchedObject.GetType() == typeof(Exit))
                {
                    var exit = (Exit)matchedObject;
                    if (!exit.CanProcess(this, command).Item1)
                    {
                        await sendOutput($"Locked.");
                        return;
                    }

                    await exit.Process(this, command, cancellationToken);
                    return;
                }

                await sendOutput($"I don't know how to process {matchedObject.UnparseObject()}");
                //                    ((Exit)matchedObject);
                return;
            }
        }

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

        if (command.raw.StartsWith("@"))
            Console.WriteLine($"Unknown at-command: {command.raw}");

        await sendOutput("Huh?");
        actionResult = new VerbResult(false, $"Command not found for verb {command.getVerb()}");
    }
}
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

public class Script : Action
{
    public override ActionType Type => ActionType.Script;

    public string programText;

    private ForthInterpreter forth;

    public override async Task<VerbResult> Process(Player player, CommandResult command, CancellationToken cancellationToken)
    {
        if (player == null)
            throw new ArgumentNullException(nameof(player));

        if (forth == null)
            forth = new ForthInterpreter(programText);

        // TODO: Right now we block on programs
        var result = await forth.SpawnAsync(player, cancellationToken);
        return new VerbResult(result.isSuccessful, result.result?.ToString());
    }

    public static Script Make(string name, string programText)
    {
        if (name == null)
            throw new System.ArgumentNullException(nameof(name));
        if (programText == null)
            throw new System.ArgumentNullException(nameof(programText));

        var script = ThingRepository.Make<Script>();
        script.name = name;
        script.programText = programText;

        CommandHandler.actions.TryAdd(script.id, script);

        Console.WriteLine($"Created new script {name}(#{script.id})");
        return script;
    }
}
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

public static class CommandHandler {

    public static readonly ConcurrentDictionary<int, Action> actions = new ConcurrentDictionary<int, Action>();

    public static async Task<VerbResult> HandleHumanCommandAsync(Player player, CommandResult command, CancellationToken cancellationToken) {

        foreach (Action action in actions.Values) {
            if (action.CanProcess(player, command))
            {
                return await action.Process(player, command, cancellationToken);
            }
        }

        await player.sendOutput("Huh?");
        return new VerbResult(false, "Command not found for verb " + command.getVerb());
    }
}
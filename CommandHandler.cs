using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

public static class CommandHandler {

    public static ConcurrentDictionary<int, Action> actions = new ConcurrentDictionary<int, Action>();

    public static async Task<VerbResult> handleHumanCommand(Player player, CommandResult command, CancellationToken cancellationToken) {

        foreach (Action action in actions.Values) {
            if (action.canProcess(player, command))
            {
                return await action.process(player, command, cancellationToken);
            }
        }

        await player.sendOutput("Huh?");
        return new VerbResult(false, "Command not found for verb " + command.getVerb());
    }
}
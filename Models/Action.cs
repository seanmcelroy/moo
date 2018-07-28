using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

public abstract class Action : Thing {
    public enum ActionType {
        BuiltIn = 1,
        Script = 2
    }

    public ActionType type;

    public HashSet<string> aliases = new HashSet<string>();

    public abstract bool canProcess(Player player, CommandResult command);

    public abstract Task<VerbResult> process(Player player, CommandResult command, CancellationToken cancellationToken);
}
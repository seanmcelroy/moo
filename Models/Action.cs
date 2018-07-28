using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

public abstract class Action : Thing {
    public enum ActionType {
        BuiltIn = 1,
        Script = 2
    }

    public abstract ActionType Type { get; }

    public HashSet<string> aliases = new HashSet<string>();
   
    public virtual bool CanProcess(Player player, CommandResult command)
    {
        string verb = command.getVerb().ToLowerInvariant();

        return string.Compare(verb, name, true) == 0 || aliases.Any(a => string.Compare(verb, a, true) == 0);
    }

    public abstract Task<VerbResult> Process(Player player, CommandResult command, CancellationToken cancellationToken);

    protected override Dictionary<string, object> GetSerializedElements() {
        var result = base.GetSerializedElements();
        result.Add("aliases", aliases.ToArray());
        return result;
    }
}
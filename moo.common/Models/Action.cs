using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

public abstract class Action : Thing
{
    public enum ActionType
    {
        BuiltIn = 1,
        Script = 2
    }

    public abstract ActionType Type { get; }

    public HashSet<string> aliases = new HashSet<string>();

    public virtual Tuple<bool, string> CanProcess(Player player, CommandResult command)
    {
        string verb = command.getVerb().ToLowerInvariant();

        foreach (var key in aliases.Union(new[] { name }))
        {
            if (string.Compare(key, verb, true) == 0)
                return new Tuple<bool, string>(true, verb);
        }

        return new Tuple<bool, string>(false, null);
    }

    public abstract Task<VerbResult> Process(Player player, CommandResult command, CancellationToken cancellationToken);

    protected override Dictionary<string, object> GetSerializedElements()
    {
        var result = base.GetSerializedElements();
        result.Add("aliases", aliases.ToArray());
        return result;
    }
}
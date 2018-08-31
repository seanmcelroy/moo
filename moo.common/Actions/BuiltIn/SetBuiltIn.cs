using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static ThingRepository;

public class SetBuiltIn : IRunnable
{
    public Tuple<bool, string> CanProcess(PlayerConnection connection, CommandResult command)
    {
        var verb = command.getVerb().ToLowerInvariant();
        if (verb == "@set" && command.hasDirectObject())
            return new Tuple<bool, string>(true, verb);

        return new Tuple<bool, string>(false, null);
    }

    public async Task<VerbResult> Process(PlayerConnection connection, CommandResult command, CancellationToken cancellationToken)
    {
        var str = command.getNonVerbPhrase();
        if (str == null || str.Length < 3)
            return new VerbResult(false, "@SET <object> = [!]<flag>\r\n@SET <object> = <property>:<string>\r\n@SET <object> = <property>:\r\n@SET <object> = :clear\r\n\r\n@set does one of three things, it can modify flags, add properties to an object, or remove properties from an object.\r\n\r\nUsing the first format, you may set flags, which are: ABODE (AUTOSTART) BUILDER (BOUND) CHOWN_OK (COLOR) DARK (DEBUG) HAVEN (HARDUID) JUMP_OK KILL_OK LINK_OK MUCKER QUELL STICKY (SETUID) VEHICLE (VIEWABLE) WIZARD XFORCIBLE ZOMBIE\r\n\r\nYou can also set the MUCKER (or Priority) Level of an object by using 0, 1, 2, or 3 as the flag name.\r\n\r\nThe second format sets <property> on <object> to <string>\r\n\r\nThe third format will remove <property> and any sub-properties under it.\r\n\r\nThe fourth format removes all properties from an object.");

        var parts = str.Split("=");
        if (parts.Length < 2)
            return new VerbResult(false, "@SET <object> = [!]<flag>\r\n@SET <object> = <property>:<string>\r\n@SET <object> = <property>:\r\n@SET <object> = :clear\r\n\r\n@set does one of three things, it can modify flags, add properties to an object, or remove properties from an object.\r\n\r\nUsing the first format, you may set flags, which are: ABODE (AUTOSTART) BUILDER (BOUND) CHOWN_OK (COLOR) DARK (DEBUG) HAVEN (HARDUID) JUMP_OK KILL_OK LINK_OK MUCKER QUELL STICKY (SETUID) VEHICLE (VIEWABLE) WIZARD XFORCIBLE ZOMBIE\r\n\r\nYou can also set the MUCKER (or Priority) Level of an object by using 0, 1, 2, or 3 as the flag name.\r\n\r\nThe second format sets <property> on <object> to <string>\r\n\r\nThe third format will remove <property> and any sub-properties under it.\r\n\r\nThe fourth format removes all properties from an object.");

        var name = parts[0].Trim();
        var predicate = parts[1].Trim();

        var targetDbref = await connection.FindThingForThisPlayerAsync(name, cancellationToken);
        if (targetDbref.Equals(Dbref.NOT_FOUND))
            return new VerbResult(false, $"Can't find '{name}' here");
        if (targetDbref.Equals(Dbref.AMBIGUOUS))
            return new VerbResult(false, $"Which one?");

        var targetLookup = await ThingRepository.GetAsync<Thing>(targetDbref, cancellationToken);
        if (!targetLookup.isSuccess)
        {
            await connection.sendOutput($"You can't seem to find that.  {targetLookup.reason}");
            return new VerbResult(false, "Target not found");
        }

        var target = targetLookup.value;

        if (predicate.Length == 1 || (predicate.Length == 2 && predicate[0] == '!') && predicate.IndexOf(':') == -1)
        {
            var negate = predicate.StartsWith("!");
            var flagChar = negate ? predicate[1] : predicate[0];
            var flagValue = (Thing.Flag)Enum.ToObject(typeof(Thing.Flag), (UInt16)flagChar.ToString().ToUpperInvariant()[0]);
            if (default(Thing.Flag).Equals(flagValue))
                return new VerbResult(false, $"@SET understands no flag named {flagChar}");

            if (negate)
                target.ClearFlag(flagValue);
            else
                target.SetFlag(flagValue);
            return new VerbResult(true, $"Object {target.UnparseObject()} modified.");
        }

        if (string.Compare(predicate, ":clear", true) == 0)
        {
            target.ClearProperties();
            return new VerbResult(true, $"Object {target.UnparseObject()} cleared of properties.");
        }

        foreach (var part in predicate.Split(' '))
        {
            var subpart = part.Trim();

            if (subpart.EndsWith(':') && subpart.Length > 1)
            {
                // Remove property at directory
                target.ClearPropertyPath(subpart.Substring(subpart.Length - 1));
            }
            else if (subpart.IndexOf(':') > -1 && !subpart.EndsWith(':') && subpart.Length > 2)
            {
                // Add property at directory
                var subpartSplit = subpart.Split(':');
                if (Property.TryInferType(subpartSplit[1], out Tuple<Property.PropertyType, object> result))
                    target.SetPropertyPathValue(subpartSplit[0], result.Item1, result.Item2);
            }
        }

        return new VerbResult(true, $"Object {target.UnparseObject()} updated");
    }
}
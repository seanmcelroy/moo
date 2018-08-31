using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static ThingRepository;

public class PropSetBuiltIn : IRunnable
{
    public Tuple<bool, string> CanProcess(PlayerConnection connection, CommandResult command)
    {
        var verb = command.getVerb().ToLowerInvariant();
        if (verb == "@propset" && command.hasDirectObject())
            return new Tuple<bool, string>(true, verb);
        return new Tuple<bool, string>(false, null);
    }

    public async Task<VerbResult> Process(PlayerConnection connection, CommandResult command, CancellationToken cancellationToken)
    {
        var str = command.getNonVerbPhrase();
        if (str == null || str.Length < 3)
            return new VerbResult(false, "@PROPSET <object>=<type>:<property>:<value> -or-\r\n@PROPSET <object>=erase:<property>\r\n\r\n@propset can set and clear properties from an object.\r\n\r\nIf the first format above is specified, the @propset command sets <property> on <object> to <value>, where <value> is of type <type>. <type> can be one of 'string', 'integer', 'float, 'dbref', or 'lock'. A string can be any set of characters the MUCK recognizes. An integer must be composed solely of numerals with the possible exception of a leading sign indicator (+ or -). A float must be a valid floating point number. A dbref must be of the form # followed by a positive integer, and it must be a valid dbref (i.e., the object must exist). A lock value must be a key that would be accepted by @lock or a similar command (see the help for @lock for more details).\r\n\r\nThe second format removes <property> on object. Note that if <property> is a propdir, it removes all properties below <property> as well. If you wish to clear the value of a propdir without removing the properties below it, use '@propset <object> = integer:<property>:0'.");

        var parts = str.Split("=");
        if (parts.Length < 2)
            return new VerbResult(false, "@PROPSET <object>=<type>:<property>:<value> -or-\r\n@PROPSET <object>=erase:<property>\r\n\r\n@propset can set and clear properties from an object.\r\n\r\nIf the first format above is specified, the @propset command sets <property> on <object> to <value>, where <value> is of type <type>. <type> can be one of 'string', 'integer', 'float, 'dbref', or 'lock'. A string can be any set of characters the MUCK recognizes. An integer must be composed solely of numerals with the possible exception of a leading sign indicator (+ or -). A float must be a valid floating point number. A dbref must be of the form # followed by a positive integer, and it must be a valid dbref (i.e., the object must exist). A lock value must be a key that would be accepted by @lock or a similar command (see the help for @lock for more details).\r\n\r\nThe second format removes <property> on object. Note that if <property> is a propdir, it removes all properties below <property> as well. If you wish to clear the value of a propdir without removing the properties below it, use '@propset <object> = integer:<property>:0'.");

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
        var predicateParts = predicate.Split(':');
        if (predicateParts.Length < 2 || (predicateParts.Length == 2 && string.Compare("erase", predicateParts[0], true) != 0) || predicateParts.Length > 3)
            return new VerbResult(false, "@PROPSET <object>=<type>:<property>:<value> -or-\r\n@PROPSET <object>=erase:<property>\r\n\r\n@propset can set and clear properties from an object.\r\n\r\nIf the first format above is specified, the @propset command sets <property> on <object> to <value>, where <value> is of type <type>. <type> can be one of 'string', 'integer', 'float, 'dbref', or 'lock'. A string can be any set of characters the MUCK recognizes. An integer must be composed solely of numerals with the possible exception of a leading sign indicator (+ or -). A float must be a valid floating point number. A dbref must be of the form # followed by a positive integer, and it must be a valid dbref (i.e., the object must exist). A lock value must be a key that would be accepted by @lock or a similar command (see the help for @lock for more details).\r\n\r\nThe second format removes <property> on object. Note that if <property> is a propdir, it removes all properties below <property> as well. If you wish to clear the value of a propdir without removing the properties below it, use '@propset <object> = integer:<property>:0'.");

        if (string.Compare("erase", predicateParts[0], true) == 0)
        {
            target.ClearPropertyPath(predicateParts[1]);
            return new VerbResult(true, $"Cleared property path {predicateParts[1]} on {target.UnparseObject()}");
        }

        Property.PropertyType type;
        if (string.Compare(predicateParts[0], "dbref", true) == 0)
            type = Property.PropertyType.DbRef;
        else if (string.Compare(predicateParts[0], "float", true) == 0)
            type = Property.PropertyType.Float;
        else if (string.Compare(predicateParts[0], "int", true) == 0
             || string.Compare(predicateParts[0], "integer", true) == 0)
            type = Property.PropertyType.Integer;
        else if (string.Compare(predicateParts[0], "str", true) == 0
             || string.Compare(predicateParts[0], "string", true) == 0)
            type = Property.PropertyType.String;
        else if (string.Compare(predicateParts[0], "lock", true) == 0)
            type = Property.PropertyType.Lock;
        else
            return new VerbResult(false, "@PROPSET <object>=<type>:<property>:<value> -or-\r\n@PROPSET <object>=erase:<property>\r\n\r\n@propset can set and clear properties from an object.\r\n\r\nIf the first format above is specified, the @propset command sets <property> on <object> to <value>, where <value> is of type <type>. <type> can be one of 'string', 'integer', 'float, 'dbref', or 'lock'. A string can be any set of characters the MUCK recognizes. An integer must be composed solely of numerals with the possible exception of a leading sign indicator (+ or -). A float must be a valid floating point number. A dbref must be of the form # followed by a positive integer, and it must be a valid dbref (i.e., the object must exist). A lock value must be a key that would be accepted by @lock or a similar command (see the help for @lock for more details).\r\n\r\nThe second format removes <property> on object. Note that if <property> is a propdir, it removes all properties below <property> as well. If you wish to clear the value of a propdir without removing the properties below it, use '@propset <object> = integer:<property>:0'.");

        target.SetPropertyPathValue(predicateParts[1], type, predicateParts[2]);
        return new VerbResult(true, $"Object {target.UnparseObject()} updated");
    }
}
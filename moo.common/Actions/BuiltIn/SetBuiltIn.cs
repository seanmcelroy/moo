using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using moo.common.Connections;
using moo.common.Database;
using moo.common.Models;

namespace moo.common.Actions.BuiltIn
{
    public class SetBuiltIn : IRunnable
    {
        public Tuple<bool, string?> CanProcess(Dbref player, CommandResult command)
        {
            var verb = command.GetVerb().ToLowerInvariant();
            if (string.Compare(verb, "@set", StringComparison.OrdinalIgnoreCase) == 0 && command.HasDirectObject())
                return new Tuple<bool, string?>(true, verb);

            return new Tuple<bool, string?>(false, null);
        }

        public async Task<VerbResult> Process(Dbref player, PlayerConnection? connection, CommandResult command, ILogger? logger, CancellationToken cancellationToken)
        {
            var str = command.GetNonVerbPhrase();
            if (str == null || str.Length < 3)
                return new VerbResult(false, "@SET <object> = [!]<flag>\r\n@SET <object> = <property>:<string>\r\n@SET <object> = <property>:\r\n@SET <object> = :clear\r\n\r\n@set does one of three things, it can modify flags, add properties to an object, or remove properties from an object.\r\n\r\nUsing the first format, you may set flags, which are: ABODE (AUTOSTART) BUILDER (BOUND) CHOWN_OK (COLOR) DARK (DEBUG) HAVEN (HARDUID) JUMP_OK KILL_OK LINK_OK MUCKER QUELL STICKY (SETUID) VEHICLE (VIEWABLE) WIZARD XFORCIBLE ZOMBIE\r\n\r\nYou can also set the MUCKER (or Priority) Level of an object by using 0, 1, 2, or 3 as the flag name.\r\n\r\nThe second format sets <property> on <object> to <string>\r\n\r\nThe third format will remove <property> and any sub-properties under it.\r\n\r\nThe fourth format removes all properties from an object.");

            var parts = str.Split("=");
            if (parts.Length < 2)
                return new VerbResult(false, "@SET <object> = [!]<flag>\r\n@SET <object> = <property>:<string>\r\n@SET <object> = <property>:\r\n@SET <object> = :clear\r\n\r\n@set does one of three things, it can modify flags, add properties to an object, or remove properties from an object.\r\n\r\nUsing the first format, you may set flags, which are: ABODE (AUTOSTART) BUILDER (BOUND) CHOWN_OK (COLOR) DARK (DEBUG) HAVEN (HARDUID) JUMP_OK KILL_OK LINK_OK MUCKER QUELL STICKY (SETUID) VEHICLE (VIEWABLE) WIZARD XFORCIBLE ZOMBIE\r\n\r\nYou can also set the MUCKER (or Priority) Level of an object by using 0, 1, 2, or 3 as the flag name.\r\n\r\nThe second format sets <property> on <object> to <string>\r\n\r\nThe third format will remove <property> and any sub-properties under it.\r\n\r\nThe fourth format removes all properties from an object.");

            var name = parts[0].Trim();
            var predicate = str[(name.Length + 1)..].Trim();

            var targetDbref = await Matcher.MatchControlled(player, name, cancellationToken);
            var target = await targetDbref.Get(cancellationToken);
            if (target == null)
            {
                await Server.NotifyAsync(player, "You can't seem to find that.");
                return new VerbResult(false, "Target not found");
            }

            if (!predicate.Contains(':'))
            {
                var negate = predicate.StartsWith("!");
                var flagChars = negate ? predicate.ToCharArray().Skip(1) : predicate.ToCharArray();
                foreach (var flagChar in flagChars)
                {
                    var flagValue = (Thing.Flag)Enum.ToObject(typeof(Thing.Flag), (UInt16)flagChar.ToString().ToUpperInvariant()[0]);
                    if (default(Thing.Flag).Equals(flagValue))
                        return new VerbResult(false, $"@SET understands no flag named {flagChar}");
                }

                foreach (var flagChar in flagChars)
                {
                    var flagValue = (Thing.Flag)Enum.ToObject(typeof(Thing.Flag), (UInt16)flagChar.ToString().ToUpperInvariant()[0]);
                    if (negate)
                        target.ClearFlag(flagValue);
                    else
                        target.SetFlag(flagValue);
                }

                return new VerbResult(true, $"Object {target.UnparseObjectInternal()} modified.");
            }

            if (string.Compare(predicate, ":clear", true) == 0)
            {
                target.ClearProperties();
                return new VerbResult(true, $"Object {target.UnparseObjectInternal()} cleared of properties.");
            }

            if (predicate.EndsWith(':'))
            {
                // Remove property at directory
                var propertyPath = predicate[..(predicate.IndexOf(':') - 1)];
                target.ClearPropertyPath(propertyPath);
                return new VerbResult(true, $"Object {target.UnparseObjectInternal()} cleared of properties under {propertyPath}.");
            }
            else if (predicate.Contains(':'))
            {
                var predicateColonIndex = predicate.IndexOf(':');
                var propertyPath = predicate[..(predicateColonIndex - 1)];
                var propertyValue = predicate[(predicateColonIndex + 1)..];

                // Add property at directory
                if (Property.TryInferType(propertyValue, out Tuple<Property.PropertyType, object>? result))
                {
                    target.SetPropertyPathValue(propertyPath, result.Item1, result.Item2);
                    return new VerbResult(true, $"Object property {propertyPath} set.");
                }
                else
                {
                    target.SetPropertyPathValue(propertyPath, Property.PropertyType.String, propertyValue);
                    return new VerbResult(true, $"Object property {propertyPath} set.");
                }
            }

            return new VerbResult(false, $"I don't see that here.");
        }
    }
}
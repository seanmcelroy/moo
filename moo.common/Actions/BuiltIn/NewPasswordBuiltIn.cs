using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using moo.common.Connections;
using moo.common.Database;
using moo.common.Models;
using static moo.common.Models.Dbref;

namespace moo.common.Actions.BuiltIn
{
    public class NewPasswordBuiltIn : IRunnable
    {
        public Tuple<bool, string?> CanProcess(Dbref player, CommandResult command)
        {
            var verb = command.GetVerb().ToLowerInvariant();
            if (string.Compare(verb, "@newpassword", StringComparison.OrdinalIgnoreCase) == 0 && command.HasDirectObject())
                return new Tuple<bool, string?>(true, verb);
            return new Tuple<bool, string?>(false, null);
        }

        public async Task<VerbResult> Process(Dbref player, PlayerConnection? connection, CommandResult command, ILogger? logger, CancellationToken cancellationToken)
        {
            var str = command.GetNonVerbPhrase();
            if (str == null || str.Length < 3)
                return new VerbResult(false, "@NEWPASSWORD <player> [=<password>]\r\nOnly wizards may use this command. Changes <player>'s password, informing <player> that you changed it. Must be typed in full.");

            var parts = str.Split(new[] { ' ', '=' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length != 2)
                return new VerbResult(false, "@NEWPASSWORD <player> [=<password>]\r\nOnly wizards may use this command. Changes <player>'s password, informing <player> that you changed it. Must be typed in full.");

            var playerName = parts[0];
            var newPassword = parts[1];

            if (string.IsNullOrWhiteSpace(playerName))
                return new VerbResult(false, "No such player.");
            if (string.IsNullOrWhiteSpace(newPassword))
                return new VerbResult(false, "Invalid password.");
            // TODO: Password ok? https://github.com/fuzzball-muck/fuzzball/blob/e8c6e70c91098d8b7ba7f96a10c88b58f34acd1f/src/wiz.c#L970

            var victimDbref = await Matcher.InitObjectSearch(player, playerName, DbrefObjectType.Player, cancellationToken)
                .MatchPlayer()
                .Result();

            if (victimDbref == NOT_FOUND)
                return new VerbResult(false, "No such player.");

            var victimLookup = await ThingRepository.Instance.GetAsync<HumanPlayer>(victimDbref, cancellationToken);
            if (!victimLookup.isSuccess || victimLookup.value == null)
                return new VerbResult(false, $"Cannot retrieve {victimDbref}.  {victimLookup.reason}");

            var victimObj = victimLookup.value;
            victimObj.SetPassword(newPassword);

            var playerObj = await Server.NotifyAsync(player, "Password changed.");
            await Server.NotifyAsync(victimDbref, $"Your password has been changed by {playerObj?.name}");
            return new VerbResult(true, $"NEWPASS'ED: {victimObj.name}({victimObj.id}) by {playerObj?.name}({player})");
        }
    }
}
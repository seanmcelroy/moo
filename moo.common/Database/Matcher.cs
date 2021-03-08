using System;
using System.Threading;
using System.Threading.Tasks;
using moo.common.Models;
using static moo.common.Models.Dbref;

namespace moo.common.Database
{
    public static class Matcher
    {
        public static MatchResult InitObjectSearch(Player player, string name, DbrefObjectType preferredType, CancellationToken cancellationToken) => MatchResult.InitObjectSearch(player, name, preferredType, cancellationToken);

        public static MatchResult CheckKeys(this MatchResult match)
        {
            match.CheckKeys = true;
            return match;
        }

        public static async Task<Dbref> MatchControlled(Player player, string name, CancellationToken cancellationToken)
        {
            var match = await InitObjectSearch(player, name, DbrefObjectType.Unknown, cancellationToken)
                .MatchEverything()
                .NoisyResult();

            if (match != NOT_FOUND && !await player.id.Controls(match, cancellationToken))
            {
                await Server.NotifyAsync(player.id, "Permission denied. (You don't control what was matched)");
                return NOT_FOUND;
            }

            return match;
        }

        public static async Task<MatchResult> MatchEverything(this MatchResult match)
        {
            return await match
            .MatchAllExits()
            .MatchNeighbor()
            .MatchPossession()
            .MatchMe()
            .MatchHere()
            .MatchRegistered()
            .MatchAbsolute();

            /*
            if (Wizard(OWNER(md->match_from)) || Wizard(md->match_who))
            {
                match_player(md);
            }
            */
        }

        public static async Task<Dbref> Result(this Task<MatchResult> matchTask) => await (await matchTask).Result();

        public static async Task<Dbref> Result(this MatchResult match)
        {
            if (match.ExactMatch != Dbref.NOT_FOUND)
                return match.ExactMatch;

            return (match.MatchCount) switch
            {
                0 => NOT_FOUND,
                1 => match.LastMatch,
                _ => AMBIGUOUS,
            };
        }

        public static async Task<Dbref> NoisyResult(this Task<MatchResult> matchTask) => await (await matchTask).NoisyResult();

        public static async Task<Dbref> NoisyResult(this MatchResult match)
        {
            var result = await match.Result();
            if (result.Equals(Dbref.NOT_FOUND))
            {
                await Server.NotifyAsync(match.Player.id, $"I don't understand '{match.MatchName}'.");
                return NOT_FOUND;
            }

            if (result.Equals(Dbref.AMBIGUOUS))
            {
                await Server.NotifyAsync(match.Player.id, $"I don't know which '{match.MatchName}' you mean!");
                return NOT_FOUND;
            }
            return result;
        }

        public static async Task<MatchResult> MatchAllExits(this MatchResult match)
        {
            var result = match;
            var loc = match.MatchFrom.Location;
            if (loc.IsValid())
            {
                result = await match.MatchRoomExits(loc);
            }

            result = await result.MatchPlayerActions();
            // TODO NOT COMPLETE!
            return result;
        }

        private static async Task<MatchResult> MatchRoomExits(this MatchResult match, Dbref location) => location.Type switch
        {
            DbrefObjectType.Player or DbrefObjectType.Room or DbrefObjectType.Thing => await match.MatchExits(location),
            _ => match,
        };

        private static async Task<MatchResult> MatchPlayerActions(this MatchResult match) => match.MatchFrom.Type switch
        {
            DbrefObjectType.Player or DbrefObjectType.Room or DbrefObjectType.Thing => await match.MatchExits(match.MatchFrom.id),
            _ => match,
        };

        private static async Task<MatchResult> MatchExits(this MatchResult match, Dbref objDbref)
        {
            var obj = await ThingRepository.Instance.GetAsync(objDbref, match.CancellationToken);
            if (!obj.isSuccess || obj.value == null)
                return match;

            if (obj.value.Contents == null || obj.value.Contents.Count == 0)
                return match; // Easy fail match

            if (match.MatchFrom.Location == NOT_FOUND)
                return match;

            Dbref absolute = await match.AbsoluteName();
            // If the owner of the match-from doesn't control the resolved absolute-name, then don't match it.
            if (!await match.MatchFrom.Owner.Controls(absolute, match.CancellationToken))
                absolute = NOT_FOUND;

            foreach (var exitDbref in obj.value.Contents)
            {
                if (exitDbref.Type != DbrefObjectType.Exit)
                    continue;

                // If we resolved it absolutely, then we have an exact match.
                if (exitDbref == absolute)
                {
                    match.ExactMatch = exitDbref;
                    continue;
                }

                var exitLookup = await ThingRepository.Instance.GetAsync<Exit>(exitDbref, match.CancellationToken);
                if (!exitLookup.isSuccess || exitLookup.value == null)
                    continue;
                Exit exit = exitLookup.value;

                // For all exit aliases
                if (!string.IsNullOrWhiteSpace(exit.name))
                    foreach (var targetName in exit.name.Split(';'))
                    {
                        // If this is an exact string match
                        if (string.Compare(match.MatchName, targetName.Trim(), StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            match.ExactMatch = exitDbref;
                            continue;
                        }
                    }

                // TODO: Fuzzball is much more complex here... https://github.com/fuzzball-muck/fuzzball/blob/e8c6e70c91098d8b7ba7f96a10c88b58f34acd1f/src/match.c
            }

            return match;
        }

        private static async Task<MatchResult> MatchRegistered(this Task<MatchResult> match) => await (await match).MatchRegistered();

        public static async Task<MatchResult> MatchRegistered(this MatchResult match)
        {
            var dbref = await FindRegisteredObject(match.Player.id, match.MatchName, match.CancellationToken);
            if (dbref != NOT_FOUND)
                match.ExactMatch = dbref;
            return match;
        }

        private static async Task<Dbref> FindRegisteredObject(Dbref player, string matchName, CancellationToken cancellationToken)
        {
            if (matchName.StartsWith('$'))
                matchName = matchName[1..];
            var prop = await Property.ScanEnvironmentForProperty(player, $"_reg/{matchName}", Property.PropertyType.Unknown, cancellationToken);

            if (default(Property).Equals(prop.Item1))
                return NOT_FOUND;

            switch (prop.Item1.Type)
            {
                case Property.PropertyType.String:
                    if (prop.Item1.Value != null && Dbref.TryParse((string)prop.Item1.Value, out Dbref result))
                        return result;
                    break;
                case Property.PropertyType.DbRef:
                    if (prop.Item1.Value != null)
                        return (Dbref)prop.Item1.Value;
                    break;
                case Property.PropertyType.Integer:
                    if (prop.Item1.Value != null)
                        return new Dbref((int)prop.Item1.Value, DbrefObjectType.Unknown);
                    break;
                case Property.PropertyType.Lock:
                    return NOT_FOUND;
            }

            return NOT_FOUND;
        }

        private static async Task<Dbref> AbsoluteName(this MatchResult match)
        {
            if (!Dbref.TryParse(match.MatchName, out Dbref dbref))
                return Dbref.NOT_FOUND;

            var obj = await ThingRepository.Instance.GetAsync(dbref, match.CancellationToken);
            if (!obj.isSuccess || obj.value == null)
                return Dbref.NOT_FOUND;

            return dbref;
        }

        public static async Task<MatchResult> MatchNeighbor(this Task<MatchResult> match) => await (await match).MatchNeighbor();

        private static async Task<MatchResult> MatchNeighbor(this MatchResult match) => match.MatchFrom.Location != NOT_FOUND ? await match.MatchContents(match.MatchFrom.Location) : match;

        public static async Task<MatchResult> MatchPossession(this Task<MatchResult> match) => await (await match).MatchPossession();

        private static async Task<MatchResult> MatchPossession(this MatchResult match) => await match.MatchContents(match.MatchFrom.id);

        private static async Task<MatchResult> MatchContents(this MatchResult match, Dbref objectDbref)
        {
            var absolute = await match.AbsoluteName();
            // If the owner of the match-from doesn't control the resolved absolute-name, then don't match it.
            if (!await match.MatchFrom.Owner.Controls(absolute, match.CancellationToken))
                absolute = NOT_FOUND;

            var objectlookup = await ThingRepository.Instance.GetAsync<Thing>(objectDbref, match.CancellationToken);
            if (!objectlookup.isSuccess || objectlookup.value == null)
                return match;

            foreach (var content in objectlookup.value.Contents)
            {
                if (content == absolute)
                {
                    match.ExactMatch = absolute;
                    return match;
                }

                var contentLookup = await ThingRepository.Instance.GetAsync<Thing>(content, match.CancellationToken);
                if (!contentLookup.isSuccess || contentLookup.value == null)
                    continue;

                if (string.Compare(match.MatchName, contentLookup.value.name, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    /*
                    * If there are multiple exact matches, randomly choose one.
                    * This works becaus md->exact_match will have been populated
                    * (potentially) by past iterations of this loop.
                    */
                    match.ExactMatch = await match.ChooseOne(content, match.ExactMatch);
                }
                else if (StringUtility.StringMatch(contentLookup.value.name, match.MatchName) >= 0)
                {
                    match.LastMatch = content;
                    match.MatchCount++;
                }
            }

            return match;
        }

        public static async Task<MatchResult> MatchMe(this Task<MatchResult> match) => (await match).MatchMe();

        private static MatchResult MatchMe(this MatchResult match)
        {
            if (string.Compare("me", match.MatchName, true) == 0)
                match.ExactMatch = match.Player.id;
            return match;
        }

        public static async Task<MatchResult> MatchHere(this Task<MatchResult> match) => (await match).MatchHere();

        private static MatchResult MatchHere(this MatchResult match)
        {
            if (string.Compare("here", match.MatchName, true) == 0 && match.Player.Location != NOT_FOUND)
                match.ExactMatch = match.Player.Location;
            return match;
        }

        public static async Task<MatchResult> MatchHome(this Task<MatchResult> match) => await (await match).MatchHome();

        private static async Task<MatchResult> MatchHome(this MatchResult match)
        {
            if (string.Compare("home", match.MatchName, true) == 0)
                match.ExactMatch = HOME;
            return match;
        }

        public static async Task<MatchResult> MatchNil(this Task<MatchResult> match) => await (await match).MatchNil();

        private static async Task<MatchResult> MatchNil(this MatchResult match)
        {
            if (string.Compare("nil", match.MatchName, true) == 0)
                match.ExactMatch = NIL;
            return match;
        }


        private static async Task<MatchResult> MatchAbsolute(this Task<MatchResult> matchTask) => await (await matchTask).MatchAbsolute();

        private static async Task<MatchResult> MatchAbsolute(this MatchResult match)
        {
            var abs = await AbsoluteName(match);
            if (abs != Dbref.NOT_FOUND)
                match.ExactMatch = abs;
            return match;
        }
    }
}
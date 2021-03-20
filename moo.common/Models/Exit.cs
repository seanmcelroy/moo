using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using moo.common.Connections;
using moo.common.Database;
using static moo.common.Models.Dbref;

namespace moo.common.Models
{
    public class Exit : Thing, IRunnable
    {
        public HashSet<string> aliases = new();

        public Exit() => this.type = (int)DbrefObjectType.Exit;

        public static Exit Make(string name, Dbref owner)
        {
            // Our caller is responsible for MoveToAsync()
            var exit = ThingRepository.Instance.Make<Exit>();
            exit.name = name;
            exit.owner = owner;
            Console.WriteLine($"Created new exit {exit.UnparseObjectInternal()}");
            return exit;
        }

        private async static Task<bool> ExitLoopCheck(Exit source, Thing dest, CancellationToken cancellationToken)
        {
            if (source == dest)
                return true;

            //var destLookup = await ThingRepository.Instance.GetAsync<Thing>(dest, cancellationToken);
            //if (!destLookup.isSuccess || destLookup.value == null || destLookup.value.Type != DbrefObjectType.Exit)
            //    return false;

            foreach (var exitLink in dest.LinkTargets)
            {
                if (exitLink == source.id)
                    return true;

                var linkLookup = await exitLink.Get(cancellationToken);
                if (linkLookup?.Type == DbrefObjectType.Exit)
                {
                    if (await ExitLoopCheck(source, linkLookup, cancellationToken))
                        return true;
                }
            }

            return false;
        }

        public async static Task<List<Dbref>> ParseLinks(Dbref player, Exit exit, string destinations, bool dryRun, CancellationToken cancellationToken)
        {
            var newLinks = new List<Dbref>();
            var newNonNullLinks = 0;

            foreach (var targetName in destinations.Split(';', StringSplitOptions.RemoveEmptyEntries))
            {
                var targetDbref = await Matcher.InitObjectSearch(player, targetName, DbrefObjectType.Unknown, cancellationToken)
                    .MatchEverything()
                    .MatchHome()
                    .MatchNil()
                    .NoisyResult();

                if (targetDbref.Equals(NOT_FOUND))
                    continue;

                if (targetDbref != NIL && targetDbref.Type == DbrefObjectType.Player)
                {
                    await Server.NotifyAsync(player, $"You can't link to players.  Destination {targetDbref} ignored.");
                    continue;
                }

                if (!await CanLink(player, targetDbref, cancellationToken))
                {
                    await Server.NotifyAsync(player, "You can't link to that.");
                    continue;
                }

                if (!await CanLinkTo(player, DbrefObjectType.Exit, targetDbref, cancellationToken))
                {
                    await Server.NotifyAsync(player, $"You can't link to {targetDbref}.");
                    continue;
                }

                if (targetDbref == NIL)
                {
                    if (!dryRun)
                        await Server.NotifyAsync(player, "Linked to NIL.");
                    newLinks.Add(NIL);
                    continue;
                }

                var target = await targetDbref.Get(cancellationToken);
                if (target == null)
                {
                    await Server.NotifyAsync(player, $"UNABLE TO LOAD {targetDbref}!");
                    continue;
                }

                switch (target.Type)
                {
                    case DbrefObjectType.Player:
                    case DbrefObjectType.Room:
                    case DbrefObjectType.Program:
                        if (newNonNullLinks > 0)
                        {
                            await Server.NotifyAsync(player, $"Only one player, room, or program destination allowed. Destination {await target.UnparseObject(player, cancellationToken)} ignored.");
                            continue;
                        }

                        newLinks.Add(targetDbref);
                        newNonNullLinks++;
                        break;
                    case DbrefObjectType.Thing:
                        newLinks.Add(targetDbref);
                        break;
                    case DbrefObjectType.Exit:
                        if (await ExitLoopCheck(exit, target, cancellationToken))
                        {
                            await Server.NotifyAsync(player, $"Destination {await target.UnparseObject(player, cancellationToken)}  would create a loop, ignored.");
                            continue;
                        }
                        newLinks.Add(targetDbref);
                        break;
                    default:
                        await Server.NotifyAsync(player, "Internal error: weird object type.");
                        break;
                }

                if (!dryRun)
                {
                    if (targetDbref == HOME)
                        await Server.NotifyAsync(player, "Linked to HOME.");
                    else
                        await Server.NotifyAsync(player, $"Linked to {await target.UnparseObject(player, cancellationToken)}.");
                }

            }

            return newLinks;
        }

        public virtual Tuple<bool, string?> CanProcess(Dbref player, CommandResult command)
        {
            // TODO: Test lock!
            return new Tuple<bool, string?>(true, null);
        }

        public async Task<VerbResult> Process(Dbref player, PlayerConnection? connection, CommandResult command, CancellationToken cancellationToken)
        {
            if (this == null || LinkTargets.Count == 0 || !LinkTargets.Any(l => l.IsValid()))
                return new VerbResult(false, "Unlinked.");

            var linkToLookups = LinkTargets.Select(l => new { id = l, lookupResult = ThingRepository.Instance.GetAsync(l, cancellationToken) }).ToArray();
            await Task.WhenAll(linkToLookups.Select(l => l.lookupResult));
            if (linkToLookups.Any(l => !l.lookupResult.IsCompletedSuccessfully || !l.lookupResult.Result.isSuccess))
                return new VerbResult(false, $"Unable to lookup link {linkToLookups.First(l => !l.lookupResult.IsCompletedSuccessfully).id}");

            // TODO: Handle multiple link locations
            var linkTo = linkToLookups[0].lookupResult.Result.value;
            if (linkTo == null)
                return new VerbResult(false, "Error looking up link.");

            switch (linkTo.Type)
            {
                case DbrefObjectType.Room:
                    {
                        if (connection != null)
                            await connection.MoveToAsync(linkTo, cancellationToken);
                        return new VerbResult(true, "Moved.");
                    }
                case DbrefObjectType.Program:
                    {
                        var actionResult = await ((Script)linkTo).Process(player, connection, command, cancellationToken);
                        if (!actionResult.isSuccess && connection != null)
                            await connection.SendOutput($"ERROR: {actionResult.reason}");
                        return actionResult;
                    }
                default:
                    if (connection != null)
                        await connection.SendOutput($"Cannot process exit linked to {await linkTo.UnparseObject(player, cancellationToken)}");
                    return new VerbResult(false, $"Cannot process exit linked to {linkTo.UnparseObjectInternal()}");
            }
        }

        protected override Dictionary<string, object?> GetSerializedElements()
        {
            var result = base.GetSerializedElements();
            result.Add("aliases", aliases.ToArray());
            return result;
        }
    }
}
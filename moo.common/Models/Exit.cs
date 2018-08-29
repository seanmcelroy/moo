using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

public class Exit : Thing, IRunnable
{
    public HashSet<string> aliases = new HashSet<string>();

    public Exit()
    {
        this.type = (int)Dbref.DbrefObjectType.Exit;
    }

    public static Exit Make(string name, Dbref owner)
    {
        // Our caller is responsible for MoveToAsync()
        var exit = ThingRepository.Make<Exit>();
        exit.name = name;
        exit.owner = owner;
        Console.WriteLine($"Created new exit {exit.UnparseObject()}");
        return exit;
    }

    public virtual Tuple<bool, string> CanProcess(PlayerConnection player, CommandResult command)
    {
        // TODO: Test lock!
        return new Tuple<bool, string>(true, null);
    }

    public async Task<VerbResult> Process(PlayerConnection connection, CommandResult command, CancellationToken cancellationToken)
    {
        if (Links == null || Links.Length == 0 || !Links.Any(l => l.IsValid()))
            return new VerbResult(false, "Unlinked.");

        var linkToLookups = Links.Select(l => new { id = l, lookupResult = ThingRepository.GetAsync(l, cancellationToken) }).ToArray();
        await Task.WhenAll(linkToLookups.Select(l => l.lookupResult));
        if (linkToLookups.Any(l => !l.lookupResult.IsCompletedSuccessfully || !l.lookupResult.Result.isSuccess))
            return new VerbResult(false, $"Unable to lookup link {linkToLookups.First(l => !l.lookupResult.IsCompletedSuccessfully).id}");

        // TODO: Handle multiple link locations
        var linkTo = linkToLookups[0].lookupResult.Result.value;
        switch (linkTo.Type)
        {
            case Dbref.DbrefObjectType.Room:
                {
                    await connection.MoveToAsync((Container)linkTo, cancellationToken);
                    return new VerbResult(true, "Moved.");
                }
            case Dbref.DbrefObjectType.Program:
                {
                    var actionResult = await ((Script)linkTo).Process(connection, command, cancellationToken);
                    if (!actionResult.isSuccess)
                        await connection.sendOutput($"ERROR: {actionResult.reason}");
                    return actionResult;
                }
            default:
                await connection.sendOutput($"Cannot process exit linked to {linkTo.UnparseObject()}");
                return new VerbResult(false, $"Cannot process exit linked to {linkTo.UnparseObject()}");
        }
    }

    protected override Dictionary<string, object> GetSerializedElements()
    {
        var result = base.GetSerializedElements();
        result.Add("aliases", aliases.ToArray());
        return result;
    }
}
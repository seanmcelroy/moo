using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using static ThingRepository;

public class Container : Thing
{

    private ConcurrentDictionary<int, byte> contents = new ConcurrentDictionary<int, byte>();

    public string internalDescription;

    public VerbResult add(int id)
    {
        if (id == this.id)
        {
            return new VerbResult(false, "A thing cannot go into itself");
        }

        // TODO: Where I'm going can't be inside of me.

        if (this.contents.TryAdd(id, 0))
        {
            return new VerbResult(true, "");
        }

        return new VerbResult(false, "Item is already in that container");
    }

    public bool contains(int id)
    {
        return contents.ContainsKey(id);
    }
    public bool contains(Thing thing)
    {
        return contents.ContainsKey(thing.id);
    }

    public IEnumerable<HumanPlayer> GetVisibleHumanPlayersForAsync(Player subject, CancellationToken cancellationToken)
    {
        var results = new ConcurrentBag<HumanPlayer>();

        var loopResult = Parallel.ForEach(contents.Keys, async peer =>
        {
            GetResult<Thing> thing = await ThingRepository.GetAsync<Thing>(peer, cancellationToken);
            if (thing.isSuccess && typeof(HumanPlayer).IsAssignableFrom(thing.value.GetType()))
                results.Add((HumanPlayer)thing.value);
        });

        return results;
    }

    public VerbResult remove(int id)
    {
        if (id == this.id)
        {
            return new VerbResult(false, "A thing cannot come from itself");
        }

        byte dud;
        if (this.contents.TryRemove(id, out dud))
        {
            return new VerbResult(true, "");
        }

        return new VerbResult(false, "Item was not in that container");
    }
}
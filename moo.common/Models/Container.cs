using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using static ThingRepository;
using static Dbref;

public class Container : Thing
{
    private ConcurrentDictionary<Dbref, int> contents = new ConcurrentDictionary<Dbref, int>();

    public string internalDescription;

    public VerbResult Add(Dbref id)
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

    public VerbResult Add(Thing thing)
    {
        return this.Add(thing.id);
    }

    public bool Contains(Dbref id)
    {
        return contents.ContainsKey(id);
    }

    public bool Contains(Thing thing)
    {
        return contents.ContainsKey(thing.id);
    }

    public async Task<Dbref> MatchAsync(string name, CancellationToken cancellationToken)
    {

        var tasks = this.contents.Keys
            .Select(async k => await ThingRepository.GetAsync<Thing>(k, cancellationToken));
        var results = await Task.WhenAll(tasks);

        var result = results
            .Where(t => t.isSuccess && string.Compare(name, t.value.name) == 0)
            .Select(t => t.value.id)
            .DefaultIfEmpty(Dbref.NOT_FOUND)
            .Aggregate((c, n) => c | n);

        return result;
    }

    public IEnumerable<HumanPlayer> GetVisibleHumanPlayersForAsync(Player subject, CancellationToken cancellationToken)
    {
        var results = new ConcurrentBag<HumanPlayer>();

        var loopResult = Parallel.ForEach(contents.Keys, async peer =>
        {
            var thing = await ThingRepository.GetAsync<Thing>(peer, cancellationToken);
            if (thing.isSuccess && typeof(HumanPlayer).IsAssignableFrom(thing.value.GetType()))
                results.Add((HumanPlayer)thing.value);
        });

        return results;
    }
    public Dbref FirstContent(DbrefObjectType[] filterTypes)
    {
        return contents.Keys.Count == 0
            ? Dbref.NOT_FOUND
            : contents.Keys
                .Where(d => filterTypes == null || filterTypes.Contains(d.Type))
                .OrderBy(d => d.ToInt32())
                .First();
    }

    public Dbref NextContent(Dbref lastContent, DbrefObjectType[] filterTypes)
    {
        if (this.contents.Keys.Count == 0)
            return Dbref.NOT_FOUND;

        return this.contents.Keys
            .Where(d => filterTypes == null || filterTypes.Contains(d.Type))
            .OrderBy(k => k.ToInt32())
            .SkipWhile(k => k <= lastContent)
            .DefaultIfEmpty(Dbref.NOT_FOUND)
            .FirstOrDefault();
    }

    public VerbResult Remove(Dbref id)
    {
        if (id == this.id)
        {
            return new VerbResult(false, "A thing cannot come from itself");
        }

        int dud;
        if (this.contents.TryRemove(id, out dud))
        {
            return new VerbResult(true, "");
        }

        return new VerbResult(false, "Item was not in that container");
    }
}
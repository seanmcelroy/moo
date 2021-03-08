/*using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using static Dbref;

public class Container : Thing
{
    public async Task<Dbref> MatchAsync(string search, CancellationToken cancellationToken)
    {
        var tasks = this.contents.Keys
            .Select(async k => await ThingRepository.Instance.GetAsync<Thing>(k, cancellationToken));
        var results = await Task.WhenAll(tasks);

        var result = results
            .Where(t => t.isSuccess && t.value?.name != null && (t.value.name.IndexOf(';') == -1 ? string.Compare(search, t.value.name) == 0 : t.value.name.Split(';').Any(sub => string.Compare(search, sub) == 0)))
            .Select(t => t.value!.id)
            .DefaultIfEmpty(Dbref.NOT_FOUND)
            .Aggregate((c, n) => c | n);

        return result;
    }

    public IEnumerable<HumanPlayer> GetVisibleHumanPlayersForAsync(CancellationToken cancellationToken)
    {
        var results = new ConcurrentBag<HumanPlayer>();

        var loopResult = Parallel.ForEach(contents.Keys, async peer =>
        {
            var thing = await ThingRepository.Instance.GetAsync<Thing>(peer, cancellationToken);
            if (thing.isSuccess && thing.value != null && typeof(HumanPlayer).IsAssignableFrom(thing.value.GetType()))
                results.Add((HumanPlayer)thing.value);
        });

        return results;
    }
    
}*/
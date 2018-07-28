using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Threading.Tasks;

public abstract class Player : Container {

    private static ConcurrentDictionary<int, Player> _cache = new ConcurrentDictionary<int, Player>();

    public static IEnumerable<Player> all() {
        foreach (Player player in _cache.Values) {
            yield return player;
        }
    }

    public static void playerConnected(Player player) {
        _cache.TryAdd(player.id, player);
    }

    public abstract void receiveInput(String input);

    public abstract Task<CommandResult> popCommand();

    public abstract Task sendOutput(String output);
}
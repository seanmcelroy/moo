using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using static Dbref;

public static class ThingRepository
{
    public struct GetResult<T> where T : Thing
    {
        public T value;

        public bool isSuccess;

        public string reason;

        public GetResult(T value, string reason)
        {
            this.value = value;
            this.isSuccess = true;
            this.reason = reason;
        }

        public GetResult(string reason)
        {
            this.value = null;
            this.isSuccess = false;
            this.reason = reason;
        }
    }

    private static ConcurrentDictionary<Dbref, Thing> _cache = new ConcurrentDictionary<Dbref, Thing>();
    private static int nextThingId = -1;
    private static IStorageProvider storageProvider;

    public static bool IsCached(Dbref dbref) => _cache.ContainsKey(dbref);

    public static T Insert<T>(T subject) where T : Thing
    {
        DbrefObjectType type;
        if (typeof(T) == typeof(Exit))
            type = DbrefObjectType.Exit;
        else if (typeof(T) == typeof(Room))
            type = DbrefObjectType.Room;
        else if (typeof(T) == typeof(HostPlayer))
            type = DbrefObjectType.Player;
        else if (typeof(T) == typeof(HumanPlayer))
            type = DbrefObjectType.Player;
        else if (typeof(T) == typeof(Script))
            type = DbrefObjectType.Program;
        else if (typeof(T) == typeof(Thing))
            type = DbrefObjectType.Thing;
        else
            throw new InvalidOperationException($"Unkonwn type: {typeof(T).Name}");

        subject.id = new Dbref(Interlocked.Increment(ref nextThingId), type);
        if (_cache.TryAdd(subject.id, subject))
            return subject;

        return null;
    }

    public static T Make<T>() where T : Thing, new()
    {
        return Insert(new T());
    }

    public static T GetFromCacheOnly<T>(Dbref id) where T : Thing, new()
    {

        // Is it in cache?
        if (_cache.ContainsKey(id))
        {
            Thing ret;
            if (_cache.TryGetValue(id, out ret))
            {
                if (typeof(T).IsAssignableFrom(ret.GetType()))
                    return (T)ret;

                return null;
            }
        }

        return null;
    }

    public static async Task<GetResult<Thing>> GetAsync(Dbref id, CancellationToken cancellationToken)
    {
        bool isSuccess;
        Thing thing;
        string reason;

        switch (id.Type)
        {
            case DbrefObjectType.Unknown:
            case DbrefObjectType.Thing:
                return await GetAsync<Thing>(id, cancellationToken);
            case DbrefObjectType.Exit:
                {
                    var result = await GetAsync<Exit>(id, cancellationToken);
                    isSuccess = result.isSuccess;
                    thing = result.value;
                    reason = result.reason;
                    break;
                }
            case DbrefObjectType.Player:
                {
                    var result = await GetAsync<HumanPlayer>(id, cancellationToken);
                    isSuccess = result.isSuccess;
                    thing = result.value;
                    reason = result.reason;
                    break;
                }
            case DbrefObjectType.Program:
                {
                    var result = await GetAsync<Script>(id, cancellationToken);
                    isSuccess = result.isSuccess;
                    thing = result.value;
                    reason = result.reason;
                    break;
                }
            case DbrefObjectType.Room:
                {
                    var result = await GetAsync<Room>(id, cancellationToken);
                    isSuccess = result.isSuccess;
                    thing = result.value;
                    reason = result.reason;
                    break;
                }
            default:
                throw new InvalidOperationException($"Unable to handle id.Type={id.Type}");
        }

        if (isSuccess)
            return new GetResult<Thing>(thing, reason);
        else
            return new GetResult<Thing>(null, reason);
    }

    public static async Task<GetResult<T>> GetAsync<T>(Dbref id, CancellationToken cancellationToken) where T : Thing, new()
    {
        if ((int)id < 0)
            return new GetResult<T>($"Invalid dbref: {id}");

        // Is it in cache?
        if (_cache.ContainsKey(id))
        {
            Thing ret;
            if (_cache.TryGetValue(id, out ret))
            {
                if (typeof(T).IsAssignableFrom(ret.GetType()))
                    return new GetResult<T>((T)ret, $"{id} found in cache");

                return new GetResult<T>($"Requested type {typeof(T).Name}, but object {id} in cache is type {ret.GetType().Name}");
            }
        }

        return await LoadFromDatabaseAsync<T>(id, cancellationToken);
    }

    public static async Task<GetResult<T>> LoadFromDatabaseAsync<T>(Dbref id, CancellationToken cancellationToken) where T : Thing, new()
    {
        if ((int)id < 0)
            return new GetResult<T>($"Invalid dbref: {id}");

        if (storageProvider == null)
            return new GetResult<T>("No storage provider is loaded");

        var providerResult = await storageProvider.LoadAsync(id, cancellationToken);
        if (!providerResult.isSuccess)
            return new GetResult<T>($"{id} not found in storage provider");

        var loadedType = Type.GetType(providerResult.type);

        if (!typeof(T).IsAssignableFrom(loadedType))
            return new GetResult<T>($"{id} found in storage provider with type {providerResult.type}, but cannot be cast to requested type {typeof(T).Name}");

        // Deserialize
        var x = (T)typeof(Thing).GetMethod("Deserialize").MakeGenericMethod(loadedType).Invoke(null, new object[] { providerResult.serialized });

        if (_cache.ContainsKey(x.id))
        {
            Thing oldThing;
            if (_cache.TryRemove(x.id, out oldThing))
                _cache.TryAdd(x.id, x);
        }
        else
        {
            _cache.TryAdd(x.id, x);
        }

        return new GetResult<T>(x, $"{id} retrieved from storage");
    }

    public static async Task<bool> FlushToDatabaseAsync<T>(T obj, CancellationToken cancellationToken) where T : Thing
    {
        if (storageProvider == null)
            return false;

        return await storageProvider.SaveAsync(obj.id, obj.GetType().Name, obj.Serialize(), cancellationToken);
    }

    public static void setStorageProvider(IStorageProvider storageProvider)
    {
        ThingRepository.storageProvider = storageProvider;
    }
}
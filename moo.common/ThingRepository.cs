using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using moo.common.Database;
using moo.common.Models;
using static moo.common.Models.Dbref;

namespace moo.common
{
    public class ThingRepository : IThingRepository
    {
        private static readonly Lazy<ThingRepository> _repository = new(() => new ThingRepository());

        public static IThingRepository Instance => _repository.Value;

        private static readonly ConcurrentDictionary<Dbref, Thing> _cache = new();
        private static int nextThingId = -1;
        private static IStorageProvider? storageProvider;

        public bool IsCached(Dbref dbref) => _cache.ContainsKey(dbref);

        public T? Insert<T>(T subject) where T : Thing
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
                throw new InvalidOperationException($"Unknown type: {typeof(T).Name}");

            subject.id = new Dbref(Interlocked.Increment(ref nextThingId), type);
            if (_cache.TryAdd(subject.id, subject))
                return subject;

            return null;
        }

        public T Make<T>() where T : Thing, new()
        {
            var newObj = new T();
            Insert(newObj);
            return newObj;
        }

        public async Task<Dbref> FindLibrary(string libName, CancellationToken cancellationToken)
        {
            var aetherLookup = await GetAsync<Room>(Dbref.AETHER, cancellationToken);
            if (!aetherLookup.isSuccess || aetherLookup.value == null)
                return NOT_FOUND;

            var prop = await aetherLookup.value.GetPropertyPathValueAsync($"_reg/{libName}", cancellationToken);
            if (prop.Equals(default(Property)))
                return NOT_FOUND;

            return (Dbref?)prop.Value ?? NOT_FOUND;
        }

        public T? GetFromCacheOnly<T>(Dbref id) where T : Thing, new()
        {
            // Is it in cache?
            if (_cache.ContainsKey(id))
            {
                if (_cache.TryGetValue(id, out Thing? ret))
                {
                    if (typeof(T).IsAssignableFrom(ret.GetType()))
                        return (T)ret;

                    return null;
                }
            }

            return null;
        }

        public async Task<bool> Exists(Dbref id, CancellationToken cancellationToken)
        {
            var thing = await GetAsync(id, cancellationToken);
            return thing.isSuccess && thing.value != null && thing.value.id != NOT_FOUND;
        }

        public async Task<RepositoryGetResult<Thing>> GetAsync(Dbref id, CancellationToken cancellationToken)
        {
            bool isSuccess;
            Thing? thing;
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
                return new RepositoryGetResult<Thing>(thing, reason);
            else
                return new RepositoryGetResult<Thing>(null, reason);
        }

        public async Task<RepositoryGetResult<T>> GetAsync<T>(Dbref id, CancellationToken cancellationToken) where T : Thing, new()
        {
            if ((int)id < 0)
                return new RepositoryGetResult<T>($"Invalid dbref: {id}");

            // Is it in cache?
            if (_cache.ContainsKey(id))
            {
                if (_cache.TryGetValue(id, out Thing? ret))
                {
                    if (typeof(T).IsAssignableFrom(ret.GetType()))
                        return new RepositoryGetResult<T>((T)ret, $"{id} found in cache");

                    return new RepositoryGetResult<T>($"Requested type {typeof(T).Name}, but object {id} in cache is type {ret.GetType().Name}");
                }
            }

            return await LoadFromDatabaseAsync<T>(id, cancellationToken);
        }

        public async Task<RepositoryGetResult<T>> LoadFromDatabaseAsync<T>(Dbref id, CancellationToken cancellationToken) where T : Thing, new()
        {
            if ((int)id < 0)
                return new RepositoryGetResult<T>($"Invalid dbref: {id}");

            if (storageProvider == null)
                return new RepositoryGetResult<T>("No storage provider is loaded");

            var providerResult = await storageProvider.LoadAsync(id, cancellationToken);
            if (!providerResult.isSuccess || providerResult.type == null)
                return new RepositoryGetResult<T>($"{id} not found in storage provider");

            var loadedType = Type.GetType(providerResult.type);

            if (!typeof(T).IsAssignableFrom(loadedType))
                return new RepositoryGetResult<T>($"{id} found in storage provider with type {providerResult.type}, but cannot be cast to requested type {typeof(T).Name}");

            // Deserialize
            var x = (T)typeof(Thing).GetMethod("Deserialize").MakeGenericMethod(loadedType).Invoke(null, new object[] { providerResult.serialized });

            if (_cache.ContainsKey(x.id))
            {
                if (_cache.TryRemove(x.id, out _))
                    _cache.TryAdd(x.id, x);
            }
            else
            {
                _cache.TryAdd(x.id, x);
            }

            return new RepositoryGetResult<T>(x, $"{id} retrieved from storage");
        }

        public async Task<bool> FlushToDatabaseAsync<T>(T obj, CancellationToken cancellationToken) where T : Thing
        {
            if (storageProvider == null)
                return false;

            return await storageProvider.SaveAsync(obj.id, obj.GetType().Name, obj.Serialize(), cancellationToken);
        }

        public void SetStorageProvider(IStorageProvider storageProvider)
        {
            ThingRepository.storageProvider = storageProvider;
        }
    }
}
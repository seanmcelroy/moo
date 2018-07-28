using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;

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

    private static ConcurrentDictionary<int, Thing> _cache = new ConcurrentDictionary<int, Thing>();
    private static int nextThingId = -1;
    private static IStorageProvider storageProvider;


    public static T Insert<T>(T subject) where T : Thing
    {
        subject.id = Interlocked.Increment(ref nextThingId);
        if (_cache.TryAdd(subject.id, subject))
            return subject;

        return null;
    }

    public static T Make<T>() where T : Thing, new()
    {
        T subject = new T();
        subject.id = Interlocked.Increment(ref nextThingId);
        if (_cache.TryAdd(subject.id, subject))
            return subject;

        return null;
    }

    public static async Task<GetResult<T>> GetAsync<T>(int id, CancellationToken cancellationToken) where T : Thing, new()
    {
        // Is it in cache?
        if (_cache.ContainsKey(id))
        {
            Thing ret;
            if (_cache.TryGetValue(id, out ret))
            {
                if (typeof(T).IsAssignableFrom(ret.GetType()))
                    return new GetResult<T>((T)ret, $"#{id} found in cache");

                return new GetResult<T>($"Requested type {typeof(T).Name}, but object #{id} in cache is type {ret.GetType().Name}");
            }
        }

        return await LoadFromDatabaseAsync<T>(id, cancellationToken);
    }

    public static async Task<GetResult<T>> LoadFromDatabaseAsync<T>(int id, CancellationToken cancellationToken) where T : Thing, new()
    {
        if (storageProvider == null)
            return new GetResult<T>("No storage provider is loaded");

        StorageProviderRetrieveResult providerResult = await storageProvider.LoadAsync(id, cancellationToken);
        if (!providerResult.isSuccess)
            return new GetResult<T>($"#{id} not found in storage provider");

        Type loadedType = Type.GetType(providerResult.type);

        if (!typeof(T).IsAssignableFrom(loadedType))
            return new GetResult<T>($"#{id} found in storage provider with type {providerResult.type}, but cannot be cast to requested type {typeof(T).Name}");

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

        return new GetResult<T>(x, $"#{id} retrieved from storage");
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
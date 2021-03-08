using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using moo.common.Database;
using moo.common.Models;

namespace moo.test
{
    public class MockRepository : IThingRepository
    {
        private readonly List<Thing> _things = new();

        public Task<bool> FlushToDatabaseAsync<T>(T obj, CancellationToken cancellationToken) where T : Thing => Task.FromResult(true);

        public Task<RepositoryGetResult<Thing>> GetAsync(Dbref id, CancellationToken cancellationToken)
        {
            var thing = _things.FirstOrDefault(t => t.id == id);
            if (thing == null)
                return Task.FromResult(new RepositoryGetResult<Thing>("not found"));
            return Task.FromResult(new RepositoryGetResult<Thing>(thing, "found"));
        }

        public Task<RepositoryGetResult<T>> GetAsync<T>(Dbref id, CancellationToken cancellationToken) where T : Thing, new()
        {
            var thing = _things.FirstOrDefault(t => t.id == id);
            if (thing == null)
                return Task.FromResult(new RepositoryGetResult<T>("not found"));
            return Task.FromResult(new RepositoryGetResult<T>((T)thing, "found"));
        }

        public T? GetFromCacheOnly<T>(Dbref id) where T : Thing, new() => (T?)_things.FirstOrDefault(t => t.id == id);

        public T? Insert<T>(T subject) where T : Thing
        {
            _things.Add(subject);
            return subject;
        }

        public bool IsCached(Dbref dbref)
        {
            var thing = _things.FirstOrDefault(t => t.id == dbref);
            return thing != null;
        }

        public Task<RepositoryGetResult<T>> LoadFromDatabaseAsync<T>(Dbref id, CancellationToken cancellationToken) where T : Thing, new()
        {
            throw new System.NotImplementedException();
        }

        public T Make<T>() where T : Thing, new()
        {
            throw new System.NotImplementedException();
        }

        public void SetStorageProvider(IStorageProvider storageProvider) { }
    }
}
using System.Threading;
using System.Threading.Tasks;
using moo.common.Models;

namespace moo.common.Database
{
    public interface IThingRepository
    {
        bool IsCached(Dbref dbref);

        T? Insert<T>(T subject) where T : Thing;

        T Make<T>() where T : Thing, new();

        Task<Dbref> FindLibrary(string libName, CancellationToken cancellationToken);

        T? GetFromCacheOnly<T>(Dbref id) where T : Thing, new();

        Task<bool> Exists(Dbref id, CancellationToken cancellationToken);

        Task<RepositoryGetResult<Player>> FindOnePlayerByName(string name, CancellationToken cancellationToken);

        Task<RepositoryGetResult<Thing>> GetAsync(Dbref id, CancellationToken cancellationToken);

        Task<RepositoryGetResult<T>> GetAsync<T>(Dbref id, CancellationToken cancellationToken) where T : Thing, new();

        Task<RepositoryGetResult<T>> LoadFromDatabaseAsync<T>(Dbref id, CancellationToken cancellationToken) where T : Thing, new();

        Task<RepositoryGetResult<Player>> LoadPlayerFromDatabaseByNameAsync(string name, CancellationToken cancellationToken);

        Task<bool> FlushToDatabaseAsync<T>(T obj, CancellationToken cancellationToken) where T : Thing;

        void SetStorageProvider(IStorageProvider storageProvider);
    }
}
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

public interface IStorageProvider {

    void Initialize();

    void Overwrite(Dictionary<int, string> serialized);

    Task<StorageProviderRetrieveResult> LoadAsync(int id, CancellationToken cancellationToken);

    Task<bool> SaveAsync(int id, string type, string serialized, CancellationToken cancellationToken);
}
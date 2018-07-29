using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

public interface IStorageProvider {

    void Initialize();

    void Overwrite(Dictionary<int, string> serialized);

    Task<StorageProviderRetrieveResult> LoadAsync(Dbref id, CancellationToken cancellationToken);

    Task<bool> SaveAsync(Dbref id, string type, string serialized, CancellationToken cancellationToken);
}
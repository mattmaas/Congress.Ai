using CongressDataCollector.Core.Interfaces;
using CongressDataCollector.Core.Models;
using CongressDataCollector.Infrastructure; // Ensure this is the correct namespace for BlobStorageManager
using System.Threading.Tasks;

namespace CongressDataCollector.Services
{
    public class StateService : IStateService
    {
        private readonly BlobStorageManager _blobStorageManager;

        public StateService(BlobStorageManager blobStorageManager)
        {
            _blobStorageManager = blobStorageManager;
        }

        public async Task SaveStateAsync(FetchState state)
        {
            await _blobStorageManager.UploadBlobAsync("state", state);
        }

        public async Task<FetchState?> LoadStateAsync()
        {
            return await _blobStorageManager.DownloadBlobAsync<FetchState>("state") ?? new FetchState();
        }
    }
}
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Newtonsoft.Json;

namespace CongressDataCollector.Infrastructure
{
    public class BlobStorageManager
    {
        private readonly BlobServiceClient _blobServiceClient;
        private readonly string _containerName;

        public BlobStorageManager(string connectionString, string containerName = "fetchstate")
        {
            _blobServiceClient = new BlobServiceClient(connectionString);
            _containerName = containerName;
        }

        public async Task UploadBlobAsync<T>(string blobName, T data)
        {
            var blobContainerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
            await blobContainerClient.CreateIfNotExistsAsync();
            var blobClient = blobContainerClient.GetBlobClient(blobName);

            var dataJson = JsonConvert.SerializeObject(data);
            await using var stream = new MemoryStream(Encoding.UTF8.GetBytes(dataJson));
            await blobClient.UploadAsync(stream, overwrite: true);
        }

        public async Task<T?> DownloadBlobAsync<T>(string blobName)
        {
            var blobContainerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
            var blobClient = blobContainerClient.GetBlobClient(blobName);

            if (!await blobClient.ExistsAsync()) return default;
            var downloadInfo = await blobClient.DownloadAsync();
            using var reader = new StreamReader(downloadInfo.Value.Content);
            var dataJson = await reader.ReadToEndAsync();
            return JsonConvert.DeserializeObject<T>(dataJson);

        }
    }
}

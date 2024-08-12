namespace RagChatLogic.ServiceWrappers
{
    using Azure.Storage.Blobs;
    using System.Threading.Tasks;

    /// <summary>
    /// Blob service client wrapper.
    /// </summary>
    public class BlobServiceClientWrapper : IBlobServiceClientWrapper
    {
        private readonly BlobServiceClient _blobServiceClient;

        /// <summary>
        /// Blob service client wrapper.
        /// </summary>
        /// <param name="blobServiceClient">Blob service client.</param>
        public BlobServiceClientWrapper(string connectionString)
        {
            _blobServiceClient = new BlobServiceClient(connectionString);   
        }

        /// <summary>
        /// Creates blob container.
        /// </summary>
        /// <param name="containerName">Blob container name.</param>
        public async Task<BlobContainerClient> CreateBlobContainerAsync(string containerName)
        {
            return await _blobServiceClient.CreateBlobContainerAsync(containerName);
        }

        /// <summary>
        /// Gets blob container.
        /// </summary>
        /// <param name="containerName">Blob container name.</param>
        public BlobContainerClient GetBlobContainerClient(string containerName)
        {
            return _blobServiceClient.GetBlobContainerClient(containerName);
        }

        /// <summary>
        /// Deletes blob container.
        /// </summary>
        /// <param name="containerName">Container name.</param>
        public async Task DeleteBlobContainer(string containerName)
        {
            await _blobServiceClient.DeleteBlobContainerAsync(containerName);
        }
    }
}

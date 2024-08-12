namespace RagChatLogic.ServiceWrappers
{
    using Azure.Storage.Blobs;
    using System.Threading.Tasks;

    /// <summary>
    /// Blob service client wrapper.
    /// </summary>
    public interface IBlobServiceClientWrapper
    {
        /// <summary>
        /// Creates blob container.
        /// </summary>
        /// <param name="containerName">Blob container name.</param>
        Task<BlobContainerClient> CreateBlobContainerAsync(string containerName);

        /// <summary>
        /// Gets blob container.
        /// </summary>
        /// <param name="containerName">Blob container name.</param>
        BlobContainerClient GetBlobContainerClient(string containerName);

        /// <summary>
        /// Deletes blob container.
        /// </summary>
        /// <param name="containerName">Container name.</param>
        Task DeleteBlobContainer(string containerName);
    }
}

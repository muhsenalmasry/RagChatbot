namespace RagChatLogic.BlobService
{
    using Microsoft.AspNetCore.Http;
    using System.Threading.Tasks;

    /// <summary>
    /// Blob service related logic.
    /// </summary>
    public interface IBlobServiceLogic
    {
        /// <summary>
        /// Upload file to a new blob in a new container.
        /// </summary>
        /// <param name="files">Files to be uploaded to blob.</param>
        /// <param name="containerName">Container name, of where files will be located.</param>
        Task CreateBlobs(List<IFormFile> files, string containerName);

        /// <summary>
        /// Upload file to a new blob in an existing container.
        /// </summary>
        /// <param name="files">Files to be uploaded to blob.</param>
        /// <param name="containerName">Name of the blob container.</param>
        Task AddBlobs(List<IFormFile> files, string containerName);

        /// <summary>
        /// Deletes blob container.
        /// </summary>
        /// <param name="containerName">Container name.</param>
        Task DeleteBlobContainer(string containerName);
    }
}

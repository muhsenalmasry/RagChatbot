namespace RagChatLogic.StorageService
{
    using Azure.Storage.Blobs;
    using Microsoft.AspNetCore.Http;
    using RagChatLogic.BlobService;
    using RagChatLogic.ServiceWrappers;
    using System;
    using System.IO;
    using System.Threading.Tasks;

    /// <summary>
    /// Blob service related logic.
    /// </summary>
    public class BlobServiceLogic : IBlobServiceLogic
    {
        private readonly IBlobServiceClientWrapper _blobServiceClientWrapper;

        /// <summary>
        /// Blob service related logic.
        /// </summary>
        /// <param name="blobServiceClientWrapper">Blob service client wrapper instance.</param>
        public BlobServiceLogic(IBlobServiceClientWrapper blobServiceClientWrapper)
        {
            _blobServiceClientWrapper = blobServiceClientWrapper;
        }

        /// <summary>
        /// Upload file to a new blob in a new container.
        /// </summary>
        /// <param name="files">Files to be uploaded to blob.</param>
        /// <param name="containerName">Container name, of where files will be located.</param>
        public async Task CreateBlobs(List<IFormFile> files, string containerName)
        {
            if (files is null || files.Count <= 0)
            {
                throw new ArgumentException("No files to be uploaded.");
            }

            string fileName = string.Empty;
            try
            {
                BlobContainerClient containerClient = await _blobServiceClientWrapper.CreateBlobContainerAsync(containerName);

                foreach (IFormFile file in files)
                {
                    fileName = file.FileName;
                    using (Stream fileStream = file.OpenReadStream())
                    {
                        await containerClient.UploadBlobAsync(fileName, fileStream);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(fileName, ex);
            }
        }

        /// <summary>
        /// Upload file to a new blob in an existing container.
        /// </summary>
        /// <param name="files">Files to be uploaded to blob.</param>
        /// <param name="containerName">Name of the blob container.</param>
        public async Task AddBlobs(List<IFormFile> files, string containerName)
        {
            if (string.IsNullOrEmpty(containerName))
            {
                throw new ArgumentException(nameof(containerName));
            }

            if (files is null || files.Count <= 0)
            {
                throw new ArgumentException(nameof(files));
            }

            BlobContainerClient containerClient = _blobServiceClientWrapper.GetBlobContainerClient(containerName);

            foreach (IFormFile file in files)
            {
                string fileName = file.FileName;
                try
                {
                    using (Stream fileStream = file.OpenReadStream())
                    {
                        await containerClient.UploadBlobAsync(fileName, fileStream);
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception(fileName, ex);
                }
            }
        }

        /// <summary>
        /// Deletes blob container.
        /// </summary>
        /// <param name="containerName">Container name.</param>
        public async Task DeleteBlobContainer(string containerName)
        {
            await _blobServiceClientWrapper.DeleteBlobContainer(containerName);
        }
    }
}

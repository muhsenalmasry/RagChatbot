namespace RagChatLogicTests
{
    using Azure.Storage.Blobs;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Http.Internal;
    using Moq;
    using RagChatLogic.ServiceWrappers;
    using RagChatLogic.StorageService;
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Threading.Tasks;
    using Xunit;

    public class BlobServiceLogicTests
    {
        private readonly Mock<IConfigurationWrapper> _configuration;
        private readonly Mock<IBlobServiceClientWrapper> _blobServiceClientWrapper;
        private readonly Mock<BlobContainerClient> _blobContainerClient;

        private readonly BlobServiceLogic target;

        public BlobServiceLogicTests()
        {
            _configuration = new Mock<IConfigurationWrapper>();
            _configuration.Setup(config => config.GetConnectionString("BlobStorage")).Returns("BlobStorageConnectionString");
            _configuration.Setup(config => config.GetValue("Secrets:BlobStorageUri")).Returns("BlobStorageUri");

            _blobServiceClientWrapper = new Mock<IBlobServiceClientWrapper>();
            _blobContainerClient = new Mock<BlobContainerClient>();

            target = new BlobServiceLogic(_blobServiceClientWrapper.Object);
        }

        [Fact]
        public async Task CreateBlobs_NoFiles_ThrowsAnException()
        {
            // Arrange
            List<IFormFile> files = null;
            var containerName = "TestContainer";

            // Act && Assert
            await Assert.ThrowsAsync<ArgumentException>(() => target.CreateBlobs(files, containerName));
        }

        [Fact]
        public async Task CreateBlobs_WithFiles_UploadsFilesToBlob()
        {
            // Arrange
            var files = new List<IFormFile>
            {
                new FormFile(new MemoryStream(Encoding.UTF8.GetBytes("This is a test file")), 0, 0, "Data", "Data.txt")
            };

            var containerName = "TestContainer";
            _blobServiceClientWrapper.Setup(client => client.CreateBlobContainerAsync(containerName)).ReturnsAsync(_blobContainerClient.Object);


            // Act
            await target.CreateBlobs(files, containerName);

            // Assert
            Assert.True(true);
            _blobServiceClientWrapper.Verify(x => x.CreateBlobContainerAsync(containerName), Times.Once);
            _blobContainerClient.Verify(x => x.UploadBlobAsync(It.IsAny<string>(), It.IsAny<Stream>(), default), Times.Once);
        }

        [Fact]
        public async Task AddFiles_NoFiles_ThrowsAnException()
        {
            // Arrange
            List<IFormFile> files = null;
            var containerName = "TestContainer";

            // Act && Assert
            await Assert.ThrowsAsync<ArgumentException>(() => target.AddBlobs(files, containerName));
        }

        [Fact]
        public async Task AddFiles_ContainerNameNullOrEmpty_ThrowsAnException()
        {
            // Arrange
            List<IFormFile> files = new List<IFormFile>
            {
                new FormFile(new MemoryStream(Encoding.UTF8.GetBytes("This is a test file")), 0, 0, "Data", "Data.txt")
            };
            var containerName = string.Empty;

            // Act && Assert
            await Assert.ThrowsAsync<ArgumentException>(() => target.AddBlobs(files, containerName));
        }

        [Fact]
        public async Task AddFiles_WithFiles_UploadsFilesToBlob()
        {
            // Arrange
            var files = new List<IFormFile>
            {
                new FormFile(new MemoryStream(Encoding.UTF8.GetBytes("This is a test file")), 0, 0, "Data", "Data.txt")
            };

            var containerName = "TestContainer";
            _blobServiceClientWrapper.Setup(client => client.GetBlobContainerClient(containerName)).Returns(_blobContainerClient.Object);

            // Act
            await target.AddBlobs(files, containerName);

            // Assert
            Assert.True(true);
            _blobServiceClientWrapper.Verify(x => x.GetBlobContainerClient(containerName), Times.Once);
            _blobContainerClient.Verify(x => x.UploadBlobAsync(It.IsAny<string>(), It.IsAny<Stream>(), default), Times.Once);
        }

        [Fact]
        public async Task DeleteBlobContainer_LogicCalled()
        {
            // Arrange
            string containerName = "TestContainer";

            // Act
            await target.DeleteBlobContainer(containerName);

            // Assert
            _blobServiceClientWrapper.Verify(x => x.DeleteBlobContainer(containerName), Times.Once);
        }
    }
}

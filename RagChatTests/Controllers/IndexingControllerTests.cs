namespace RagChatTests.Controllers
{
    using Azure;
    using Azure.Search.Documents.Indexes.Models;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Moq;
    using RagChat.Controllers;
    using RagChat.Logic;
    using RagChat.Models;
    using RagChatLogic.BlobService;
    using RagChatLogic.DTOs;
    using RagChatLogic.SearchService;
    using System.Security.Claims;
    using System.Threading.Tasks;
    using Xunit;

    public class IndexingControllerTests
    {
        private readonly Mock<IAccountLogic> _accountLogic;
        private readonly IConfiguration _configuration;
        private readonly Mock<IBlobServiceLogic> _blobServiceLogic;
        private readonly Mock<ISearchServiceLogic> _searchServiceLogic;
        private readonly RagChatbotDbContext _dbContext;

        private IndexingController target;

        public IndexingControllerTests()
        {
            _accountLogic = new Mock<IAccountLogic>();
            _blobServiceLogic = new Mock<IBlobServiceLogic>();
            _searchServiceLogic = new Mock<ISearchServiceLogic>();

            _configuration = new ConfigurationBuilder().Build();
            var dbContextOptions = new DbContextOptionsBuilder<RagChatbotDbContext>()
                .UseInMemoryDatabase(databaseName: "IndexingControllerTestDatabase")
                .Options;

            _dbContext = new RagChatbotDbContext(dbContextOptions);

            target = new IndexingController(_accountLogic.Object, _configuration, _blobServiceLogic.Object, _searchServiceLogic.Object, _dbContext)
            {
                ControllerContext = new ControllerContext()
                {
                    HttpContext = new DefaultHttpContext
                    {
                        User = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
                        {
                            new Claim(ClaimTypes.Name, "testUser"),
                            new Claim(ClaimTypes.NameIdentifier, "userId")
                        }, "TestAuthentication"))
                    }
                }
            };
        }

        [Fact]
        public async Task GetUserIndexes_UserNotFound_ReturnsEmptyContent()
        {
            // Act
            var result = await target.GetUserIndexes();

            // Clear DbContext.
            this.CleanDbContext();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedIndexes = Assert.IsAssignableFrom<IEnumerable<UserIndex>>(okResult.Value);
            Assert.Empty(returnedIndexes);
        }

        [Fact]
        public async Task GetUserIndexes_UserFound_ReturnsOkWithIndexes()
        {
            // Arrange
            var userId = "testUserId";
            var userIndexes = new List<UserIndex>
            {
                new UserIndex { UserId = userId, IndexName = "Index1", ContainerName = "Container1", DisplayName = "Index 1", IsAvailableForPublic = false },
                new UserIndex { UserId = userId, IndexName = "Index2", ContainerName = "Container1", DisplayName = "Index 1", IsAvailableForPublic = true }
            };

            _accountLogic.Setup(x => x.GetUserId(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(userId);

            _dbContext.UserIndexes.AddRange(userIndexes);
            _dbContext.SaveChanges();

            // Act
            var result = await target.GetUserIndexes();

            // Clear DbContext.
            this.CleanDbContext();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedIndexes = Assert.IsAssignableFrom<IEnumerable<UserIndex>>(okResult.Value);
            Assert.Equal(2, returnedIndexes.Count());
            _accountLogic.Verify(x => x.GetUserId(It.IsAny<ClaimsPrincipal>()), Times.Once);
        }

        [Fact]
        public async Task UploadFileToNewIndex_IndexCreationFailed_ReturnsNoContent()
        {
            // Arrange
            var files = new List<IFormFile> { new FormFile(Stream.Null, 0, 0, "file", "file.txt") };
            var indexName = "Index1";
            var containerName = "Container1";
            var displayName = "Index 1";
            var isAvailableForPublic = false;

            var mockResponse = new Mock<Response<SearchIndexer>>();
            mockResponse.SetupGet(r => r.GetRawResponse().IsError).Returns(true);
            Response<SearchIndexer> indexCreationResponse = mockResponse.Object;

            _blobServiceLogic.Setup(x => x.CreateBlobs(It.IsAny<List<IFormFile>>(), containerName)).Returns(Task.CompletedTask);
            _searchServiceLogic.Setup(x => x.CreateOrUpdateIndex(containerName, indexName)).ReturnsAsync(mockResponse.Object);

            // Act
            var result = await target.UploadFileToNewIndex(displayName, indexName, isAvailableForPublic, containerName, files);

            // Clear DbContext.
            this.CleanDbContext();

            // Assert
            Assert.IsType<NoContentResult>(result);
            _blobServiceLogic.Verify(x => x.CreateBlobs(It.IsAny<List<IFormFile>>(), containerName), Times.Once);
            _searchServiceLogic.Verify(x => x.CreateOrUpdateIndex(containerName, indexName), Times.Once);
        }

        [Fact]
        public async Task UploadFileToNewIndex_IndexCreated_ReturnsOk()
        {
            // Arrange
            var files = new List<IFormFile> { new FormFile(Stream.Null, 0, 0, "file", "file.txt") };
            string userId = "testUserId";
            var indexName = "Index1";
            var containerName = "Container1";
            var displayName = "Index 1";
            var isAvailableForPublic = false;

            var mockResponse = new Mock<Response<SearchIndexer>>();
            mockResponse.SetupGet(r => r.GetRawResponse().IsError).Returns(false);
            Response<SearchIndexer> indexCreationResponse = mockResponse.Object;

            _blobServiceLogic.Setup(x => x.CreateBlobs(It.IsAny<List<IFormFile>>(), containerName + userId)).Returns(Task.CompletedTask);
            _searchServiceLogic.Setup(x => x.CreateOrUpdateIndex(containerName + userId, indexName + userId)).ReturnsAsync(mockResponse.Object);
            _accountLogic.Setup(x => x.GetUserId(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(userId);

            _dbContext.UserIndexes = (new Mock<DbSet<UserIndex>>()).Object;

            // Act
            var result = await target.UploadFileToNewIndex(displayName, indexName, isAvailableForPublic, containerName, files);

            // Clear DbContext.
            this.CleanDbContext();

            // Assert
            Assert.IsType<OkObjectResult>(result);
            _blobServiceLogic.Verify(x => x.CreateBlobs(It.IsAny<List<IFormFile>>(), containerName + userId), Times.Once);
            _searchServiceLogic.Verify(x => x.CreateOrUpdateIndex(containerName + userId, indexName + userId), Times.Once);
            _accountLogic.Verify(x => x.GetUserId(It.IsAny<ClaimsPrincipal>()), Times.Once);
        }

        [Fact]
        public async Task UploadFileToExistingIndex_IndexCreationFailed_ReturnsNoContent()
        {
            // Arrange
            var files = new List<IFormFile> { new FormFile(Stream.Null, 0, 0, "file", "file.txt") };
            var userId = "testUserId";
            int indexId = 1000;
            var indexName = "Index1";
            var containerName = "Container1";

            var mockResponse = new Mock<Response<SearchIndexer>>();
            mockResponse.SetupGet(r => r.GetRawResponse().IsError).Returns(true);
            Response<SearchIndexer> indexCreationResponse = mockResponse.Object;

            var userIndexes = new List<UserIndex>
            {
                new UserIndex { Id = indexId, UserId = userId, IndexName = indexName, ContainerName = containerName, DisplayName = "Index 1", IsAvailableForPublic = false },
            };

            _accountLogic.Setup(x => x.GetUserId(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(userId);

            _dbContext.UserIndexes.AddRange(userIndexes);
            _dbContext.SaveChanges();

            _blobServiceLogic.Setup(x => x.AddBlobs(It.IsAny<List<IFormFile>>(), containerName)).Returns(Task.CompletedTask);
            _searchServiceLogic.Setup(x => x.CreateOrUpdateIndex(containerName, indexName)).ReturnsAsync(mockResponse.Object);

            // Act
            var result = await target.UploadFileToExistingIndex(indexId, files);

            // Clear DbContext.
            this.CleanDbContext();

            // Assert
            Assert.IsType<NoContentResult>(result);
            _blobServiceLogic.Verify(x => x.AddBlobs(It.IsAny<List<IFormFile>>(), containerName), Times.Once);
            _searchServiceLogic.Verify(x => x.CreateOrUpdateIndex(containerName, indexName), Times.Once);
        }

        [Fact]
        public async Task UploadFileToExistingIndex_IndexCreated_ReturnsOk()
        {
            // Arrange
            var files = new List<IFormFile> { new FormFile(Stream.Null, 0, 0, "file", "file.txt") };
            string userId = "testUserId";
            int indexId = 1000;
            var indexName = "Index1";
            var containerName = "Container1";

            var mockResponse = new Mock<Response<SearchIndexer>>();
            mockResponse.SetupGet(r => r.GetRawResponse().IsError).Returns(false);
            Response<SearchIndexer> indexCreationResponse = mockResponse.Object;

            var userIndexes = new List<UserIndex>
            {
                new UserIndex { Id = indexId, UserId = userId, IndexName = indexName, ContainerName = containerName, DisplayName = "Index 1", IsAvailableForPublic = false },
            };

            _accountLogic.Setup(x => x.GetUserId(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(userId);

            _dbContext.UserIndexes.AddRange(userIndexes);
            _dbContext.SaveChanges();

            _blobServiceLogic.Setup(x => x.AddBlobs(It.IsAny<List<IFormFile>>(), containerName)).Returns(Task.CompletedTask);
            _searchServiceLogic.Setup(x => x.CreateOrUpdateIndex(containerName, indexName)).ReturnsAsync(mockResponse.Object);

            // Act
            var result = await target.UploadFileToExistingIndex(indexId, files);

            // Clear DbContext.
            this.CleanDbContext();

            // Assert
            Assert.IsType<OkResult>(result);
            _blobServiceLogic.Verify(x => x.AddBlobs(It.IsAny<List<IFormFile>>(), containerName), Times.Once);
            _searchServiceLogic.Verify(x => x.CreateOrUpdateIndex(containerName, indexName), Times.Once);
        }

        [Fact]
        public async Task DeleteIndex_UserCannotDeleteIndex_ReturnsBadRequestResult()
        {
            // Arrange
            string userId = "wrongUserId";
            var userIndex = new UserIndex 
            {
                UserId = "userId",
                IndexName = "Index1",
                ContainerName = "Container1",
                DisplayName = "Index 1",
                IsAvailableForPublic = false
            };

            _accountLogic.Setup(x => x.GetUserId(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(userId);

            _dbContext.UserIndexes.Add(userIndex);
            _dbContext.SaveChanges();

            // Act
            var result = await target.DeleteIndex(userIndex.Id);

            // Clear DbContext.
            this.CleanDbContext();

            // Assert
            Assert.IsType<BadRequestResult>(result);
            _blobServiceLogic.Verify(x => x.DeleteBlobContainer(It.IsAny<string>()), Times.Never);
            _searchServiceLogic.Verify(x => x.DeleteIndex(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task DeleteIndex_UserCanDeleteIndex_ReturnsOk()
        {
            // Arrange
            string userId = "userId";
            var userIndex = new UserIndex
            {
                UserId = userId,
                IndexName = "Index1",
                ContainerName = "Container1",
                DisplayName = "Index 1",
                IsAvailableForPublic = false
            };

            _accountLogic.Setup(x => x.GetUserId(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(userId);

            _dbContext.UserIndexes.Add(userIndex);
            _dbContext.SaveChanges();

            // Act
            var result = await target.DeleteIndex(userIndex.Id);

            // Clear DbContext.
            this.CleanDbContext();

            // Assert
            Assert.IsType<OkResult>(result);
            _blobServiceLogic.Verify(x => x.DeleteBlobContainer(userIndex.ContainerName), Times.Once);
            _searchServiceLogic.Verify(x => x.DeleteIndex(userIndex.IndexName, userIndex.ContainerName), Times.Once);
        }

        private void CleanDbContext()
        {
            _dbContext.Database.EnsureDeleted();
            _dbContext.Database.EnsureCreated();
        }
    }
}

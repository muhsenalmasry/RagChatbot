namespace RagChatLogicTests
{
    using Azure.Search.Documents.Indexes.Models;
    using Moq;
    using RagChatLogic.SearchService;
    using RagChatLogic.ServiceWrappers;
    using System;
    using System.Threading.Tasks;
    using Xunit;

    public class SearchServiceLogicTests
    {
        private readonly Mock<IConfigurationWrapper> _configuration;
        private readonly Mock<ISearchClientsWrapper> _searchClientsWrapper;

        private readonly SearchServiceLogic target;

        public SearchServiceLogicTests()
        {
            _configuration = new Mock<IConfigurationWrapper>();
            _searchClientsWrapper = new Mock<ISearchClientsWrapper>();

            _configuration.Setup(config => config.GetConnectionString("BlobStorage")).Returns("BlobStorageConnectionString");

            target = new SearchServiceLogic(_configuration.Object, _searchClientsWrapper.Object);
        }

        [Fact]
        public async Task CreateOrUpdateIndex_ContainerNameIsNull_ThrowsAnException()
        {
            // Arrange
            string containerName = null;
            string indexName = "TestIndex";

            // Act && Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => target.CreateOrUpdateIndex(containerName, indexName));
        }

        [Fact]
        public async Task CreateOrUpdateIndex_CreatesOrUpdateIndex()
        {
            // Arrange
            string containerName = "TestContainer";
            string indexName = "TestIndex";

            // Act
            await target.CreateOrUpdateIndex(containerName, indexName);

            // Assert
            _searchClientsWrapper.Verify(x => x.CreateOrUpdateIndex(containerName, indexName), Times.Once);
            _searchClientsWrapper.Verify(client => client.CreateOrUpdateIndexer(containerName, indexName, It.IsAny<SearchIndexerDataSourceConnection>()), Times.Once);
        }

        [Fact]
        public async Task DeleteIndex_LogicCalled()
        {
            // Arrange
            string indexName = "TestIndex";
            string datasourceName = "TestDatasource";

            // Act
            await target.DeleteIndex(indexName, datasourceName);

            // Assert
            _searchClientsWrapper.Verify(x => x.DeleteIndex(indexName, datasourceName), Times.Once);
        }
    }
}

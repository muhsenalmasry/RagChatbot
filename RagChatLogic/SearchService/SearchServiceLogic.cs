namespace RagChatLogic.SearchService
{
    using Azure.Search.Documents.Indexes.Models;
    using RagChatLogic.ServiceWrappers;
    using System;
    using System.Threading.Tasks;

    /// <summary>
    /// Azure cognitive search service related logic.
    /// </summary>
    public class SearchServiceLogic : ISearchServiceLogic
    {
        private readonly IConfigurationWrapper _configuration;
        private readonly ISearchClientsWrapper _searchClientsWrapper;

        /// <summary>
        /// Azure cognitive search service related logic.
        /// </summary>
        /// <param name="configuration">Configurations.</param>
        /// <param name="searchClientsWrapper">Search clients wrapper.</param>
        public SearchServiceLogic(IConfigurationWrapper configuration, ISearchClientsWrapper searchClientsWrapper)
        {
            _configuration = configuration;
            _searchClientsWrapper = searchClientsWrapper;
        }

        /// <summary>
        /// Create a new index or updates an exisitng one.
        /// </summary>
        /// <param name="containerName">Name of the blob container.</param>
        /// <param name="indexName">Index name if exisintg.</param>
        public async Task<Azure.Response<SearchIndexer>> CreateOrUpdateIndex(string containerName, string indexName)
        {
            if (containerName == null)
            {
                throw new ArgumentNullException(nameof(containerName));
            }

            try
            {
                await _searchClientsWrapper.CreateOrUpdateIndex(containerName, indexName);
                var dataSource = new SearchIndexerDataSourceConnection(
                    name: containerName,
                    type: SearchIndexerDataSourceType.AzureBlob,
                    connectionString: _configuration.GetConnectionString("BlobStorage"),
                    container: new SearchIndexerDataContainer(containerName));

                return await _searchClientsWrapper.CreateOrUpdateIndexer(containerName, indexName, dataSource);
            }
            catch (Exception ex)
            {
                throw new Exception("ex", ex);
            }
        }

        /// <summary>
        /// Deletes index, datasource, and indexer.
        /// </summary>
        /// <param name="indexName">Index name.</param>
        /// <param name="datasourceName">Datasource connection name.</param>
        public async Task DeleteIndex(string indexName, string datasourceName)
        {
            await _searchClientsWrapper.DeleteIndex(indexName, datasourceName);
        }
    }
}

namespace RagChatLogic.ServiceWrappers
{
    using Azure;
    using Azure.Search.Documents.Indexes;
    using Azure.Search.Documents.Indexes.Models;
    using System;
    using System.Threading.Tasks;

    /// <summary>
    /// Search clients wrapper.
    /// </summary>
    public class SearchClientsWrapper : ISearchClientsWrapper
    {
        private readonly SearchIndexClient searchIndexClient;
        private readonly SearchIndexerClient searchIndexerClient;

        /// <summary>
        /// Search clients wrapper.
        /// </summary>
        /// <param name="searchApiUri">Search service API url.</param>
        /// <param name="searchApiKey">Search service API key.</param>
        public SearchClientsWrapper(string searchApiUri, string searchApiKey)
        {
            searchIndexClient = new SearchIndexClient(new Uri(searchApiUri), new Azure.AzureKeyCredential(searchApiKey));
            searchIndexerClient = new SearchIndexerClient(new Uri(searchApiUri), new Azure.AzureKeyCredential(searchApiKey));
        }

        /// <summary>
        /// Create a new index or updates an exisitng one.
        /// </summary>
        /// <param name="containerName">Name of the blob container.</param>
        /// <param name="indexName">Index name if exisintg.</param>
        public async Task CreateOrUpdateIndex(string containerName, string indexName)
        {
            var definition = new SearchIndex(indexName)
            {
                Fields =
                {
                    new SimpleField("Id", SearchFieldDataType.String) { IsKey = true, IsFilterable = true, IsSortable = true, IsFacetable = false },
                    new SearchableField("Content") { IsFilterable = false, IsSortable = false, IsFacetable = false }
                }
            };

            await searchIndexClient.CreateOrUpdateIndexAsync(definition);
        }

        /// <summary>
        /// Create a new indexer or updates an exisitng one.
        /// </summary>
        /// <param name="containerName">Name of the blob container.</param>
        /// <param name="indexName">Index name if exisintg.</param>
        /// <param name="dataSource">Data source connection.</param>
        public async Task<Azure.Response<SearchIndexer>> CreateOrUpdateIndexer(string containerName, string indexName, SearchIndexerDataSourceConnection dataSource)
        {
            await searchIndexerClient.CreateOrUpdateDataSourceConnectionAsync(dataSource);
            var indexer = new SearchIndexer(indexName, containerName, indexName);
            try
            {
                await searchIndexerClient.GetIndexerAsync(indexName);
                await searchIndexerClient.ResetIndexerAsync(indexName);
            }
            catch (RequestFailedException ex) when (ex.Status == 404) { }

            var response = await searchIndexerClient.CreateOrUpdateIndexerAsync(indexer);
            try
            {
                await searchIndexerClient.RunIndexerAsync(indexName);
            }
            catch (RequestFailedException ex) when (ex.Status == 429) { }

            return response;
        }

        /// <summary>
        /// Deletes index, datasource, and indexer.
        /// </summary>
        /// <param name="indexName">Index name.</param>
        /// <param name="datasourceName">Datasource connection name.</param>
        public async Task DeleteIndex(string indexName, string datasourceName)
        {
            try
            {
                await searchIndexClient.DeleteIndexAsync(indexName);
                await searchIndexerClient.DeleteDataSourceConnectionAsync(indexName);
                await searchIndexerClient.DeleteIndexerAsync(indexName);
            }
            catch (RequestFailedException ex) { }
        }
    }
}

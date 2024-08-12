namespace RagChatLogic.ServiceWrappers
{
    using Azure.Search.Documents.Indexes.Models;
    using System.Threading.Tasks;

    /// <summary>
    /// Search clients wrapper.
    /// </summary>
    public interface ISearchClientsWrapper
    {
        /// <summary>
        /// Create a new index or updates an exisitng one.
        /// </summary>
        /// <param name="containerName">Name of the blob container.</param>
        /// <param name="indexName">Index name if exisintg.</param>
        Task CreateOrUpdateIndex(string containerName, string indexName);

        /// <summary>
        /// Create a new indexer or updates an exisitng one.
        /// </summary>
        /// <param name="containerName">Name of the blob container.</param>
        /// <param name="indexName">Index name if exisintg.</param>
        /// <param name="dataSource">Data source connection.</param>
        Task<Azure.Response<SearchIndexer>> CreateOrUpdateIndexer(string containerName, string indexName, SearchIndexerDataSourceConnection dataSource);

        /// <summary>
        /// Deletes index, datasource, and indexer.
        /// </summary>
        /// <param name="indexName">Index name.</param>
        /// <param name="datasourceName">Datasource connection name.</param>
        Task DeleteIndex(string indexName, string datasourceName);
    }
}

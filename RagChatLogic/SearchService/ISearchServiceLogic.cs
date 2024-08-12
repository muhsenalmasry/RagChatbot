namespace RagChatLogic.SearchService
{
    using Azure.Search.Documents.Indexes.Models;
    using System.Threading.Tasks;

    /// <summary>
    /// Azure cognitive search service related logic.
    /// </summary>
    public interface ISearchServiceLogic
    {
        /// <summary>
        /// Create a new index or updates an exisitng one.
        /// </summary>
        /// <param name="containerName">Name of the blob container.</param>
        /// <param name="indexName">Index name if exisintg.</param>
        Task<Azure.Response<SearchIndexer>> CreateOrUpdateIndex(string containerName, string? indexName = null);

        /// <summary>
        /// Deletes index, datasource, and indexer.
        /// </summary>
        /// <param name="indexName">Index name.</param>
        /// <param name="datasourceName">Datasource connection name.</param>
        Task DeleteIndex(string indexName, string datasourceName);
    }
}

namespace RagChatLogic.ServiceWrappers
{
    using Azure.AI.OpenAI;

    /// <summary>
    /// The open AI client wrapper.
    /// </summary>
    public interface IOpenAIClientWrapper
    {
        /// <summary>
        /// Gets the search chat extension configuration client.
        /// </summary>
        /// <param name="indexName">The index name.</param>
        AzureSearchChatExtensionConfiguration GetSearchClient(string indexName);

        /// <summary>
        /// Gets the open AI client.
        /// </summary>
        OpenAIClient GetOpenAIClient();
    }
}

namespace RagChatLogic.ServiceWrappers
{
    using Azure;
    using Azure.AI.OpenAI;
    using Microsoft.Extensions.Configuration;
    using System;

    /// <summary>
    /// Open AI client wrapper.
    /// </summary>
    public class OpenAIClientWrapper : IOpenAIClientWrapper
    {
        private readonly IConfiguration _configuration;

        /// <summary>
        /// Open AI client wrapper.
        /// </summary>
        /// <param name="configuration">The configuration wrapper.</param>
        public OpenAIClientWrapper(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        /// <summary>
        /// Gets the search chat extension configuration client.
        /// </summary>
        /// <param name="indexName">The index name.</param>
        public AzureSearchChatExtensionConfiguration GetSearchClient(string indexName)
        {
            return new AzureSearchChatExtensionConfiguration()
            {
                SearchEndpoint = new Uri(_configuration["Secrets-SearchApiUri"]),
                Authentication = new OnYourDataApiKeyAuthenticationOptions(_configuration["Secrets-SearchApiKey"]),
                IndexName = indexName,
            };
        }

        /// <summary>
        /// Gets the open AI client.
        /// </summary>
        public OpenAIClient GetOpenAIClient()
        {
            return new OpenAIClient(
                new Uri(_configuration["Secrets-OpenAIApiUri"]),
                new AzureKeyCredential(_configuration["Secrets-OpenAIApiKey"]));
        }
    }
}

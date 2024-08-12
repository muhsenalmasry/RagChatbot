namespace RagChatLogic.OpenAIService
{
    using Azure;
    using Azure.AI.OpenAI;
    using RagChatLogic.DTOs;
    using RagChatLogic.Enums;
    using RagChatLogic.ServiceWrappers;
    using System;
    using System.Threading.Tasks;

    /// <summary>
    /// Open AI service related logic.
    /// </summary>
    public class OpenAIServiceLogic : IOpenAIService
    {
        private readonly IOpenAIClientWrapper _openAIClientWrapper;

        /// <summary>
        /// Open AI service related logic.
        /// </summary>
        /// <param name="openAIClientWrapper">The open AI client wrapper.</param>
        public OpenAIServiceLogic(IOpenAIClientWrapper openAIClientWrapper)
        {
            _openAIClientWrapper = openAIClientWrapper;
        }

        /// <summary>
        /// Makes chat completion based on user input.
        /// </summary>
        /// <param name="userInput">User input.</param>
        /// <param name="indexName">Index name.</param>
        /// <param name="messages">List of historical messages between the user and the system.</param>
        public async Task<ChatResponseMessage> CompleteChat(string userInput, string indexName, List<Message> messages)
        {
            if(string.IsNullOrEmpty(userInput))
            {
                throw new ArgumentNullException(nameof(userInput));
            }
            else if(string.IsNullOrEmpty(indexName))
            {
                throw new ArgumentNullException(nameof(indexName));
            }

            try
            {
                var _searchClient = _openAIClientWrapper.GetSearchClient(indexName);
                var _openAIClient = _openAIClientWrapper.GetOpenAIClient();

                var chatCompletionsOptions = new ChatCompletionsOptions()
                {
                    DeploymentName = "gpt-24-turbo", // Model deployment name.
                    Messages =
                    {
                        // System message.
                        new ChatRequestUserMessage("You are a helpful assistant. You will talk like a professional in a casual tone. " +
                        " You will provide answers based on the fact that there can be a relationship between the data in the documents in the index."),
                    },
                    AzureExtensionsOptions = new AzureChatExtensionsOptions()
                    {
                        Extensions = { _searchClient },
                    },
                };

                foreach (var msg in messages)
                {
                    if (msg.Sender == nameof(MessageSender.User))
                    {
                        chatCompletionsOptions.Messages.Add(new ChatRequestUserMessage(msg.Text));
                    }
                    else if (msg.Sender == nameof(MessageSender.System))
                    {
                        chatCompletionsOptions.Messages.Add(new ChatRequestSystemMessage(msg.Text));
                    }
                }

                //chatCompletionsOptions.Messages.Add(new ChatRequestUserMessage(userInput));

                Response<ChatCompletions> response = await _openAIClient.GetChatCompletionsAsync(chatCompletionsOptions);
                ChatResponseMessage message = response.Value.Choices[0].Message;

                return message;
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to complete chat.", ex);
            }
        }
    }
}

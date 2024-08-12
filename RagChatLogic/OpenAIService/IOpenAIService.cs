namespace RagChatLogic.OpenAIService
{
    using Azure.AI.OpenAI;
    using RagChatLogic.DTOs;
    using System.Threading.Tasks;

    /// <summary>
    /// Open AI service related logic.
    /// </summary>
    public interface IOpenAIService
    {
        /// <summary>
        /// Makes chat completion based on user input.
        /// </summary>
        /// <param name="userInput">User input.</param>
        /// <param name="indexName">Index name.</param>
        /// <param name="messages">List of historical messages between the user and the system.</param>
        Task<ChatResponseMessage> CompleteChat(string userInput, string indexName, List<Message> messages);
    }
}

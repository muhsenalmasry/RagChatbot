namespace RagChatLogic.DTOs
{
    /// <summary>
    /// Chat completion request model.
    /// </summary>
    public class ChatCompletionRequestModel
    {
        /// <summary>
        /// User input.
        /// </summary>
        public string UserInput { get; set; }

        /// <summary>
        /// Index identifier.
        /// </summary>
        public int IndexId { get; set; }

        /// <summary>
        /// Historical messages between the user and the system.
        /// </summary>
        public List<Message> Messages { get; set; }
    }
}

namespace RagChatLogic.DTOs
{
    /// <summary>
    /// Registeration response.
    /// </summary>
    public class RegisterResponse
    {
        /// <summary>
        /// Whether registeration is successful.
        /// </summary>
        public bool IsSuccessful { get; set; }

        /// <summary>
        /// Response message.
        /// </summary>
        public string? Message { get; set; }
    }
}

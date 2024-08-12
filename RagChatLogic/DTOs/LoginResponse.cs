namespace RagChatLogic.DTOs
{
    /// <summary>
    /// Login reponse.
    /// </summary>
    public class LoginResponse
    {
        /// <summary>
        /// Access token.
        /// </summary>
        public string? AccessToken { get; set; }

        /// <summary>
        /// Refresh token.
        /// </summary>
        public string? RefreshToken { get; set; }

        /// <summary>
        /// Email address.
        /// </summary>
        public string? Email { get; set; }
    }
}

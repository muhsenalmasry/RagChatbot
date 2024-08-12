namespace RagChatFrontend.Authentication
{
    /// <summary>
    /// Authentication model.
    /// </summary>
    public class AuthenticationModel
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
        /// User's email address.
        /// </summary>
        public string? Email { get; set; }
    }
}

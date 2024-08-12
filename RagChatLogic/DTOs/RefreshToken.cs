namespace RagChatLogic.DTOs
{
    /// <summary>
    /// Refresh token.
    /// </summary>
    public class RefreshToken
    {
        /// <summary>
        /// Token identifier.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// User identifier.
        /// </summary>
        public string UserId { get; set; }

        /// <summary>
        /// Toke.
        /// </summary>
        public string Token { get; set; }

        /// <summary>
        /// Token's expiry date.
        /// </summary>
        public DateTime ExpiryDate { get; set; }

        /// <summary>
        /// Time the token created at.
        /// </summary>
        public DateTime CreatedAt { get; set; }
    }
}

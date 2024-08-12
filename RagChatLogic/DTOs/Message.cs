namespace RagChatLogic.DTOs
{
    /// <summary>
    /// Message model.
    /// </summary>
    public class Message
    {
        /// <summary>
        /// Message identifier.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Message's sender.
        /// </summary>
        public string Sender { get; set; }

        /// <summary>
        /// Index identifier.
        /// </summary>
        public int IndexId { get; set; }

        /// <summary>
        /// Message's text.
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// User identifier.
        /// </summary>
        public string UserId { get; set; }
    }
}

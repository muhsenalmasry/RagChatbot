namespace RagChatLogic.DTOs
{
    using RagChatLogic.Enums;

    /// <summary>
    /// User index.
    /// </summary>
    public class UserIndex
    {
        /// <summary>
        /// User index identifier.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// User identifier.
        /// </summary>
        public string UserId { get; set; }

        /// <summary>
        /// Index name.
        /// </summary>
        public string IndexName { get; set; }

        /// <summary>
        /// Container name.
        /// </summary>
        public string ContainerName { get; set; }

        /// <summary>
        /// Display name.
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// Whether index is available for public.
        /// </summary>
        public bool IsAvailableForPublic { get; set; }

        /// <summary>
        /// Datasource type.
        /// </summary>
        public DatasourceType DatasourceType { get; set; }
    }
}

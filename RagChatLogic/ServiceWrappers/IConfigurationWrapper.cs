namespace RagChatLogic.ServiceWrappers
{
    /// <summary>
    /// Configuration wrapper.
    /// </summary>
    public interface IConfigurationWrapper
    {
        /// <summary>
        /// Get connection string from configuration.
        /// </summary>
        /// <param name="name">Name of the connection string.</param>
        string GetConnectionString(string name);

        /// <summary>
        /// Get value from configuration.
        /// </summary>
        /// <param name="key">Key for the value to be retrieved.</param>
        string GetValue(string key);
    }
}

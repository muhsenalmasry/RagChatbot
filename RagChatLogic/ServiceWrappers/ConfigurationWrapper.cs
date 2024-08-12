namespace RagChatLogic.ServiceWrappers
{
    using Microsoft.Extensions.Configuration;

    /// <summary>
    /// Configuration wrapper.
    /// </summary>
    public class ConfigurationWrapper : IConfigurationWrapper
    {
        private readonly IConfiguration _configuration;

        public ConfigurationWrapper(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        /// <summary>
        /// Get connection string from configuration.
        /// </summary>
        /// <param name="name">Name of the connection string.</param>
        public string GetConnectionString(string name)
        {
            return _configuration[$"ConnectionStrings-{name}"];
        }

        /// <summary>
        /// Get value from configuration.
        /// </summary>
        /// <param name="key">Key for the value to be retrieved.</param>
        public string GetValue(string key)
        {
            return _configuration[key];
        }
    }
}

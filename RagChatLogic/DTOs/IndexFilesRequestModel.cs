namespace RagChatLogic.DTOs
{
    using Microsoft.AspNetCore.Http;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    /// <summary>
    /// Indexing file request model.
    /// </summary>
    public class IndexFilesRequestModel
    {
        /// <summary>
        /// New index display name.
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// Exisiting index name.
        /// </summary>
        public string IndexName { get; set; }

        /// <summary>
        /// Exisiting index container name.
        /// </summary>
        public string ContainerName { get; set; }

        /// <summary>
        /// Files to be uploaded.
        /// </summary>
        //public List<IFormFile> Files { get; set; }
    }
}

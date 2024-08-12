namespace RagChat.Controllers
{
    using Azure.Core;
    using Azure.Search.Documents.Indexes.Models;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
    using RagChat.Logic;
    using RagChat.Models;
    using RagChatLogic.BlobService;
    using RagChatLogic.DTOs;
    using RagChatLogic.Enums;
    using RagChatLogic.SearchService;

    /// <summary>
    /// Controller for indexing files.
    /// </summary>
    public class IndexingController : ControllerBase
    {
        private readonly IAccountLogic _accountLogic;
        private readonly IConfiguration _configuration;
        private readonly IBlobServiceLogic _blobServiceLogic;
        private readonly ISearchServiceLogic _searchServiceLogic;
        private readonly RagChatbotDbContext _dbContext;

        /// <summary>
        /// Controller for indexing files.
        /// </summary>
        /// <param name="accountLogic">Account logic.</param>
        /// <param name="configuration">Configurations.</param>
        /// <param name="blobServiceLogic">Blob service logic.</param>
        /// <param name="searchServiceLogic">Search service logic.</param>
        public IndexingController(
            IAccountLogic accountLogic,
            IConfiguration configuration,
            IBlobServiceLogic blobServiceLogic, 
            ISearchServiceLogic searchServiceLogic,
            RagChatbotDbContext dbContext)
        {
            _accountLogic = accountLogic;
            _configuration = configuration;
            _blobServiceLogic = blobServiceLogic;
            _searchServiceLogic = searchServiceLogic;
            _dbContext = dbContext;
        }

        /// <summary>
        /// Returns user indexes.
        /// </summary>
        [HttpGet("userindexes")]
        [Authorize]
        public async Task<IActionResult> GetUserIndexes()
        {
            string? userId = await _accountLogic.GetUserId(HttpContext.User);
            var indexes = await _dbContext.UserIndexes.Where(ui => ui.UserId == userId || ui.IsAvailableForPublic).ToListAsync();
            
            return Ok(indexes);
        }

        /// <summary>
        /// Uploads a file to blob storage and creates index on it.
        /// </summary>
        /// <param name="displayName">Display name of the index.</param>
        /// <param name="indexName">Index name.</param>
        /// <param name="isAvailableForPublic">Is index available for public.</param>
        /// <param name="containerName">Container name, of where files will be located.</param>
        /// <param name="datasourceType">Datasource type.</param>
        /// <param name="files">Files to be uploaded.</param>
        [HttpPost("indexfiles")]
        [Authorize]
        public async Task<IActionResult> UploadFileToNewIndex(
            [FromForm] string displayName,
            [FromForm] string indexName, 
            [FromForm] bool isAvailableForPublic, 
            [FromForm] string containerName,
            [FromForm] DatasourceType datasourceType,
            List<IFormFile> files)
        {
            string? userId = await _accountLogic.GetUserId(HttpContext.User);
            //containerName += userId;
            //indexName += userId;
            //await _blobServiceLogic.CreateBlobs(files, containerName);
            //Azure.Response<SearchIndexer> indexCreationResponse = await _searchServiceLogic.CreateOrUpdateIndex(containerName, indexName);
            //if (indexCreationResponse.GetRawResponse().IsError)
            //{
            //    return new NoContentResult();
            //}

            var userIndex = new UserIndex
            {
                DisplayName = displayName,
                IndexName = indexName,
                ContainerName = containerName,
                IsAvailableForPublic = isAvailableForPublic,
                UserId = userId,
                DatasourceType = datasourceType
            };

            await _dbContext.UserIndexes.AddAsync(userIndex);
            await _dbContext.SaveChangesAsync();

            return Ok(userIndex.Id);
        }

        /// <summary>
        /// Uploads a file to blob storage and creates index on it.
        /// </summary>
        /// <param name="indexName">Index identifier.</param>
        /// <param name="files">Files to be uploaded.</param>
        [HttpPost("indexfiles-existing")]
        [Authorize]
        public async Task<IActionResult> UploadFileToExistingIndex([FromForm] int indexId, List<IFormFile> files)
        {
            string? userId = await _accountLogic.GetUserId(HttpContext.User);
            var index = await _dbContext.UserIndexes.FirstOrDefaultAsync(ui => ui.Id == indexId && ui.UserId == userId);
            if (index == null)
            {
                throw new ArgumentNullException($"No index found with provided {indexId}");
            }

            string containerName = index.ContainerName;
            string indexName = index.IndexName;

            await _blobServiceLogic.AddBlobs(files, containerName);
            var indexCreationResponse = await _searchServiceLogic.CreateOrUpdateIndex(containerName, indexName);
            if (indexCreationResponse.GetRawResponse().IsError)
            {
                return new NoContentResult();
            }

            return Ok();
        }

        /// <summary>
        /// Deletes index.
        /// </summary>
        /// <param name="id">Index identifier.</param>
        [HttpDelete("deletechatbot")]
        [Authorize]
        public async Task<IActionResult> DeleteIndex(int id)
        {
            string? userId = await _accountLogic.GetUserId(HttpContext.User);
            var index = await _dbContext.UserIndexes.FirstOrDefaultAsync(ui => ui.Id == id && ui.UserId == userId);
            if (index == null)
            {
                return new BadRequestResult();
            }

            try
            {
                await _blobServiceLogic.DeleteBlobContainer(index.ContainerName);
                await _searchServiceLogic.DeleteIndex(index.IndexName, index.ContainerName);

                _dbContext.Messages.RemoveRange(_dbContext.Messages.Where(m => m.IndexId == index.Id));
                _dbContext.UserIndexes.Remove(index);
                _dbContext.SaveChanges();

                return Ok();
            }
            catch (Exception e)
            {
                return new BadRequestResult();
            }
        }
    }
}

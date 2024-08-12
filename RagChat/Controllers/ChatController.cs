namespace RagChat.Controllers
{
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
    using RagChat.Logic;
    using RagChat.Models;
    using RagChatLogic.DTOs;
    using RagChatLogic.Enums;
    using RagChatLogic.OpenAIService;

    /// <summary>
    /// Controller for chat completion.
    /// </summary>
    public class ChatController : ControllerBase
    {
        private readonly IAccountLogic _accountLogic;
        private readonly IOpenAIService _openAIService;
        private readonly RagChatbotDbContext _dbContext;

        /// <summary>
        /// Controller for chat completion.
        /// </summary>
        /// <param name="accountLogic">Account related logic.</param>
        /// <param name="openAIService">Azure open AI service.</param>
        /// <param name="dbContext">Chatbot database context.</param>
        public ChatController(
            IAccountLogic accountLogic,
            IOpenAIService openAIService,
            RagChatbotDbContext dbContext)
        {
            _accountLogic = accountLogic;
            _openAIService = openAIService;
            _dbContext = dbContext;
        }

        /// <summary>
        /// Makes a chat completion based on user input.
        /// </summary>
        /// <param name="request">Chat completion request.</param>
        [HttpPost("chatcompletion")]
        [Authorize]
        public async Task<IActionResult> ChatCompletion([FromBody] ChatCompletionRequestModel request)
        {
            var index = await _dbContext.UserIndexes.Where(ui => ui.Id == request.IndexId).FirstOrDefaultAsync();
            var responseMessage = await _openAIService.CompleteChat(request.UserInput, index.IndexName, request.Messages);
            string? userId = await _accountLogic.GetUserId(HttpContext.User);

            var result = new Message
                {
                    IndexId = request.IndexId,
                Text = responseMessage.Content,
                Sender = nameof(MessageSender.System),
                    UserId = userId,
            };

            await _dbContext.Messages.AddAsync(
                new Message
                {
                    IndexId = request.IndexId,
                    Text = request.UserInput,
                    Sender = nameof(MessageSender.User),
                    UserId = userId,
                });

            await _dbContext.Messages.AddAsync(result);
            _dbContext.SaveChanges();

            return Ok(result);
        }

        /// <summary>
        /// Gets messages from database by provided index identifier.
        /// </summary>
        /// <param name="selectedIndexId">Selected index identifier.</param>
        [HttpGet("chat-messages")]
        [Authorize]
        public async Task<IActionResult> GetChatMessages([FromQuery] int selectedIndexId)
        {
            string? userId = await _accountLogic.GetUserId(HttpContext.User);
            var messages = await _dbContext.Messages.Where(m => m.IndexId == selectedIndexId && m.UserId == userId).OrderBy(x => x.Id).ToListAsync();

            return Ok(messages);
        }
    }
}

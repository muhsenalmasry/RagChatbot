namespace RagChatTests.Controllers
{
    using Azure.AI.OpenAI;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
    using Moq;
    using RagChat.Controllers;
    using RagChat.Logic;
    using RagChat.Models;
    using RagChatLogic.DTOs;
    using RagChatLogic.Enums;
    using RagChatLogic.OpenAIService;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Claims;
    using System.Threading.Tasks;
    using Xunit;

    public class ChatControllerTests
    {
        private readonly Mock<IAccountLogic> _accountLogic;
        private readonly Mock<IOpenAIService> _openAIService;
        private readonly RagChatbotDbContext _dbContext;

        private ChatController target;

        public ChatControllerTests()
        {
            _accountLogic = new Mock<IAccountLogic>();
            _openAIService = new Mock<IOpenAIService>();

            var dbContextOptions = new DbContextOptionsBuilder<RagChatbotDbContext>()
                .UseInMemoryDatabase(databaseName: "ChatControllerTestDatabase")
                .Options;

            _dbContext = new RagChatbotDbContext(dbContextOptions);

            target = new ChatController(_accountLogic.Object, _openAIService.Object, _dbContext)
            {
                ControllerContext = new ControllerContext()
                {
                    HttpContext = new DefaultHttpContext
                    {
                        User = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
                        {
                            new Claim(ClaimTypes.Name, "testUser"),
                            new Claim(ClaimTypes.NameIdentifier, "userId")
                        }, "TestAuthentication"))
                    }
                }
            };
        }

        [Fact]
        public async Task GetChatMessages_UserNotFound_ReturnsEmptyContent()
        {
            // Act
            var result = await target.GetChatMessages(It.IsAny<int>());

            // Clear DbContext.
            this.CleanDbContext();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedIndexes = Assert.IsAssignableFrom<IEnumerable<Message>>(okResult.Value);
            Assert.Empty(returnedIndexes);
            _accountLogic.Verify(x => x.GetUserId(It.IsAny<ClaimsPrincipal>()), Times.Once);
        }

        [Fact]
        public async Task GetChatMessages_UserFound_ReturnsMessages()
        {
            // Arrange
            var userId = "testUserId";
            int indexId = 1;
            var messages = new List<Message>
            {
                new Message { UserId = userId, IndexId = indexId, Text = "Message text 1", Sender = nameof(MessageSender.User) },
                new Message { UserId = userId, IndexId = indexId, Text = "Message text 2", Sender = nameof(MessageSender.System) }
            };

            _accountLogic.Setup(x => x.GetUserId(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(userId);

            _dbContext.Messages.AddRange(messages);
            _dbContext.SaveChanges();

            // Act
            var result = await target.GetChatMessages(indexId);

            // Clear DbContext.
            this.CleanDbContext();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedMessages = Assert.IsAssignableFrom<IEnumerable<Message>>(okResult.Value);
            Assert.Equal(messages.Count, returnedMessages.Count());
            _accountLogic.Verify(x => x.GetUserId(It.IsAny<ClaimsPrincipal>()), Times.Once);
        }

        [Fact]
        public async Task ChatCompletion_UserInputSent_ReturnsResponseMessage()
        {
            // Arrange
            var userId = "testUserId";
            int indexId = 1;
            string indexName = "IndexName";
            var userIndexes = new List<UserIndex>
            {
                new UserIndex { Id = indexId, IndexName = indexName, UserId = userId, ContainerName = "ContainerName", DisplayName = "Index name" }
            };
            var openAiResponse = AzureOpenAIModelFactory.ChatResponseMessage(ChatRole.Assistant, "System response");
            _openAIService.Setup(x => x.CompleteChat(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<Message>>())).ReturnsAsync(openAiResponse);
            _accountLogic.Setup(x => x.GetUserId(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(userId);

            _dbContext.UserIndexes.AddRange(userIndexes);
            _dbContext.SaveChanges();

            ChatCompletionRequestModel request = new ChatCompletionRequestModel
            {
                UserInput = "User input",
                IndexId = indexId
            };

            // Act
            var result = await target.ChatCompletion(request);

            // Clear DbContext.
            this.CleanDbContext();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var okResultContent = Assert.IsAssignableFrom<Message>(okResult.Value);
            Assert.Equal(openAiResponse.Content, okResultContent.Text);
            _openAIService.Verify(x => x.CompleteChat(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<Message>>()), Times.Once);
        }

        private void CleanDbContext()
        {
            _dbContext.Database.EnsureDeleted();
            _dbContext.Database.EnsureCreated();
        }
    }
}

namespace RagChatLogicTests
{
    using Azure;
    using Azure.AI.OpenAI;
    using Moq;
    using RagChatLogic.DTOs;
    using RagChatLogic.OpenAIService;
    using RagChatLogic.ServiceWrappers;
    using Xunit;

    public class OpenAIServicesLogicTests
    {
        private readonly Mock<IOpenAIClientWrapper> _openAIClientWrapper;

        private readonly OpenAIServiceLogic target;

        public OpenAIServicesLogicTests()
        {
            _openAIClientWrapper = new Mock<IOpenAIClientWrapper>();

            target = new OpenAIServiceLogic(_openAIClientWrapper.Object);
        }

        [Fact]
        public async Task CompleteChat_UserInputIsNull_ThrowsArgumentNullException()
        {
            // Arrange
            string userInput = null;
            string indexName = "indexName";

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => target.CompleteChat(userInput, indexName, new List<Message>()));
        }

        [Fact]
        public async Task CompleteChat_IndexNameIsNull_ThrowsArgumentNullException()
        {
            // Arrange
            string userInput = "userInput";
            string indexName = null;

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => target.CompleteChat(userInput, indexName, new List<Message>()));
        }

        [Fact]
        public async Task CompleteChat_ValidInput_ReturnsChatResponseMessage()
        {
            // Arrange
            string userInput = "userInput";
            string indexName = "indexName";

            Mock<AzureSearchChatExtensionConfiguration> searchClient = new Mock<AzureSearchChatExtensionConfiguration>();
            Mock<OpenAIClient> openAIClient = new Mock<OpenAIClient>();
            var mockResponse = new Mock<Response<ChatCompletions>>();
            var choices = new List<ChatChoice>
            {
                AzureOpenAIModelFactory.ChatChoice(AzureOpenAIModelFactory.ChatResponseMessage(content: "Content")),
            };

            var chatCompletions = AzureOpenAIModelFactory.ChatCompletions(choices: choices);
            mockResponse.SetupGet(r => r.Value).Returns(chatCompletions);

            _openAIClientWrapper.Setup(x => x.GetSearchClient(indexName)).Returns(searchClient.Object);
            _openAIClientWrapper.Setup(x => x.GetOpenAIClient()).Returns(openAIClient.Object);
            openAIClient.Setup(x => x.GetChatCompletionsAsync(It.IsAny<ChatCompletionsOptions>(), default)).ReturnsAsync(mockResponse.Object);

            // Act
            var result = await target.CompleteChat(userInput, indexName, new List<Message>());

            // Assert
            Assert.NotNull(result);
            _openAIClientWrapper.Verify(x => x.GetSearchClient(indexName), Times.Once);
            _openAIClientWrapper.Verify(x => x.GetOpenAIClient(), Times.Once);
            openAIClient.Verify(x => x.GetChatCompletionsAsync(It.IsAny<ChatCompletionsOptions>(), default), Times.Once);
        }
    }
}

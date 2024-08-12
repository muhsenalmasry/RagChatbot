namespace RagChatTests.Controllers
{
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.DependencyInjection;
    using Moq;
    using RagChat.Controllers;
    using RagChat.Logic;
    using RagChatLogic.DTOs;
    using System;
    using System.Threading.Tasks;
    using Xunit;

    public class AccountControllerTests
    {
        private readonly Mock<IAccountLogic> _accountLogicMock;
        private readonly Mock<HttpContext> _httpContext;

        private readonly AccountController target;

        public AccountControllerTests()
        {
            _accountLogicMock = new Mock<IAccountLogic>();
            _httpContext = GetMockHttpContext();

            var mockAuthenticationService = new Mock<IAuthenticationService>();
            var services = new ServiceCollection();
            services.AddSingleton(mockAuthenticationService.Object);
            var serviceProvider = services.BuildServiceProvider();

            var httpContext = new DefaultHttpContext();
            httpContext.RequestServices = serviceProvider;

            target = new AccountController(_accountLogicMock.Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = _httpContext.Object
                }
            };
        }

        [Fact]
        public async Task Register_UserCreated_ReturnsOk()
        {
            // Arrange
            var user = new User();
            _accountLogicMock.Setup(x => x.CreateAccount(user, null)).ReturnsAsync(new RegisterResponse());

            // Act
            var result = await target.Register(user);

            // Assert
            Assert.IsType<OkObjectResult>(result);
            _accountLogicMock.Verify(x => x.CreateAccount(user, null), Times.Once);
        }

        [Fact]
        public async Task Login_UserLoggedIn_ReturnsOk()
        {
            // Arrange
            var loginInfo = new Login();
            _accountLogicMock.Setup(x => x.LoginAccount(loginInfo)).ReturnsAsync(new LoginResponse { AccessToken = "AccessToken", RefreshToken = "RefreshToken" });

            // Act
            var result = await target.Login(loginInfo);

            // Assert
            Assert.IsType<OkObjectResult>(result);
            _accountLogicMock.Verify(x => x.LoginAccount(loginInfo), Times.Once);
        }

        [Fact]
        public async Task Login_UserNotLoggedIn_ReturnsUnauthorized()
        {
            // Arrange
            var loginInfo = new Login();
            _accountLogicMock.Setup(x => x.LoginAccount(loginInfo)).ReturnsAsync(new LoginResponse { AccessToken = null, RefreshToken = null });

            // Act
            var result = await target.Login(loginInfo);

            // Assert
            Assert.IsType<UnauthorizedResult>(result);
            _accountLogicMock.Verify(x => x.LoginAccount(loginInfo), Times.Once);
        }

        [Fact]
        public async Task Logout_UserLoggedOut_ReturnsOk()
        {
            // Act
            var result = await target.Logout(new Login());

            // Assert
            Assert.IsType<OkResult>(result);
        }

        [Fact]
        public async Task Refresh_TokenNotRefreshed_ReturnsUnAuthorized()
        {
            // Arrange
            string accessToken = null;
            string refreshToken = "RefreshToken";
            _accountLogicMock.Setup(x => x.Refresh(It.IsAny<string>())).ReturnsAsync(accessToken);

            // Act
            var result = await target.Refresh(refreshToken);

            // Assert
            Assert.IsType<UnauthorizedResult>(result);
            _accountLogicMock.Verify(x => x.Refresh(refreshToken), Times.Once);
        }

        [Fact]
        public async Task Refresh_TokenRefreshed_ReturnsUnAuthorized()
        {
            // Arrange
            string accessToken = "AccessToken";
            string refreshToken = "RefreshToken";
            _accountLogicMock.Setup(x => x.Refresh(It.IsAny<string>())).ReturnsAsync(accessToken);

            // Act
            var result = await target.Refresh(refreshToken);

            // Assert
            Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<string>(((OkObjectResult)result).Value);
            Assert.Equal(accessToken, response);
            _accountLogicMock.Verify(x => x.Refresh(refreshToken), Times.Once);
        }

        /// <summary>
        /// Gets mocked HttpContext.
        /// </summary>
        private static Mock<HttpContext> GetMockHttpContext()
        {
            var httpContext = new Mock<HttpContext>();
            var authenticationService = new Mock<IAuthenticationService>();
            authenticationService.Setup(x => x.SignOutAsync(It.IsAny<HttpContext>(), It.IsAny<string>(), It.IsAny<AuthenticationProperties>()))
                .Returns(Task.FromResult((object)null));
            var serviceProvider = new Mock<IServiceProvider>();
            serviceProvider.Setup(x => x.GetService(typeof(IAuthenticationService))).Returns(authenticationService.Object);
            httpContext.Setup(x => x.RequestServices).Returns(serviceProvider.Object);

            return httpContext;
        }
    }
}

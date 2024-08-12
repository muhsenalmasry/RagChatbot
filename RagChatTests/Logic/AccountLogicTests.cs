namespace RagChatTests.Logic
{
    using Microsoft.AspNetCore.Identity;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using Moq;
    using RagChat.Logic;
    using RagChat.Models;
    using System;
    using System.Security.Claims;
    using System.Threading.Tasks;
    using Xunit;

    public class AccountLogicTests
    {
        private readonly Mock<UserManager<ApplicationUser>> _userManager;
        private readonly Mock<RoleManager<IdentityRole>> _roleManager;
        private readonly Mock<ITokenLogic> _tokenLogic;
        private readonly RagChatbotDbContext _dbContext;

        private readonly AccountLogic target;

        public AccountLogicTests()
        {
            _userManager = GetUserManager();
            _roleManager = GetRoleManager();
            _tokenLogic = new Mock<ITokenLogic>();
            var dbContextOptions = new DbContextOptionsBuilder<RagChatbotDbContext>()
                .UseInMemoryDatabase(databaseName: "ChatControllerTestDatabase")
                .Options;

            _dbContext = new RagChatbotDbContext(dbContextOptions);

            target = new AccountLogic(_userManager.Object, _roleManager.Object, _tokenLogic.Object, _dbContext);
        }

        [Fact]
        public async Task GetUserId_ReturnsEmptyString_WhenUserDoesNotExist()
        {
            // Arrange
            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new Claim[] { new Claim(ClaimTypes.NameIdentifier, "TestId") }));

            // Act
            var userId = await target.GetUserId(claimsPrincipal);

            // Assert
            Assert.Equal(string.Empty, userId);
        }

        [Fact]
        public async Task GetUserId_UserFound_ReturnsUserId()
        {
            // Arrange
            var user = new ApplicationUser { Id = "TestId" };
            _userManager.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(user);
            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new Claim[] { new Claim(ClaimTypes.NameIdentifier, "TestId") }));

            // Act
            var userId = await target.GetUserId(claimsPrincipal);

            // Assert
            Assert.Equal(user.Id, userId);
        }

        /// <summary>
        /// Gets mocked UserManager.
        /// </summary>
        private static Mock<UserManager<ApplicationUser>> GetUserManager()
        {
            var userManager = new Mock<UserManager<ApplicationUser>>(
                new Mock<IUserStore<ApplicationUser>>().Object,
                new Mock<IOptions<IdentityOptions>>().Object,
                new Mock<IPasswordHasher<ApplicationUser>>().Object,
                new IUserValidator<ApplicationUser>[0],
                new IPasswordValidator<ApplicationUser>[0],
                new Mock<ILookupNormalizer>().Object,
                new Mock<IdentityErrorDescriber>().Object,
                new Mock<IServiceProvider>().Object,
                new Mock<ILogger<UserManager<ApplicationUser>>>().Object);

            return userManager;
        }

        /// <summary>
        /// Gets mocked RoleManager.
        /// </summary>
        private static Mock<RoleManager<IdentityRole>> GetRoleManager()
        {
            var roleManager = new Mock<RoleManager<IdentityRole>>(
                new Mock<IRoleStore<IdentityRole>>().Object,
                new IRoleValidator<IdentityRole>[0],
                new Mock<ILookupNormalizer>().Object,
                new Mock<IdentityErrorDescriber>().Object,
                new Mock<ILogger<RoleManager<IdentityRole>>>().Object);

            return roleManager;
        }
    }
}

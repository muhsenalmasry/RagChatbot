namespace RagChatTests.Logic
{
    using Microsoft.Extensions.Configuration;
    using Microsoft.IdentityModel.Tokens;
    using Moq;
    using RagChat.Logic;
    using System;
    using System.Collections.Generic;
    using System.IdentityModel.Tokens.Jwt;
    using System.Linq;
    using System.Security.Claims;
    using System.Text;
    using Xunit;

    public class TokenLogicTests
    {
        private const string jwtKey = "MIHcAgEBBEIALQeBGjBLQ338qgdDHr8Bl0UDARrOzJb0Y9louhYRnJtCvhPfFUrf";
        private const string jwtIssuer = "QD6XL26z0qBPglKN5Ss52iJW4hFnUxzGRDYLft3GFQ==";
        private readonly Mock<IConfiguration> _configuration;

        private readonly TokenLogic target;

        public TokenLogicTests()
        {
            _configuration = new Mock<IConfiguration>();
            _configuration.Setup(config => config["Jwt-Key"]).Returns(jwtKey);
            _configuration.Setup(config => config["Jwt-Issuer"]).Returns("issuer");
            _configuration.Setup(config => config["Jwt-Audience"]).Returns("audience");
            _configuration.Setup(config => config["Jwt-IssuerSigningKey"]).Returns(jwtIssuer);

            target = new TokenLogic(_configuration.Object);
        }

        [Fact]
        public void GenerateAccessToken_WhenCalled_ReturnsAccessToken()
        {
            // Arrange
            var userClaims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, "Test User"),
                new Claim(ClaimTypes.Role, "Administrator")
            };

            // Act
            var token = target.GenerateAccessToken(userClaims);

            // Assert
            Assert.NotNull(token);

            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.ReadToken(token) as JwtSecurityToken;

            Assert.NotNull(jwtToken);
            Assert.Equal("issuer", jwtToken.Issuer);
            Assert.Equal("audience", jwtToken.Audiences.First());
            Assert.Contains(jwtToken.Claims, claim => claim.Type == ClaimTypes.Name && claim.Value == "Test User");
            Assert.Contains(jwtToken.Claims, claim => claim.Type == ClaimTypes.Role && claim.Value == "Administrator");
        }

        [Fact]
        public void GenerateRefreshToken_ShouldReturnValidTokenAndExpiryDate()
        {
            // Act
            var (refreshToken, expiryDate) = target.GenerateRefreshToken();

            // Assert
            Assert.NotNull(refreshToken);
            Assert.NotEmpty(refreshToken);
            Assert.True(IsBase64String(refreshToken));

            var expectedExpiryDate = DateTime.Now.AddDays(7);
            var difference = expectedExpiryDate - expiryDate;

            Assert.True(difference < TimeSpan.FromSeconds(5));
        }

        [Fact]
        public void GetPrincipalFromExpiredToken_ValidExpiredToken_ReturnsPrincipal()
        {
            // Arrange
            var token = CreateExpiredToken(_configuration.Object["Jwt-IssuerSigningKey"]);

            // Act
            var principal = target.GetPrincipalFromExpiredToken(token);

            // Assert
            Assert.NotNull(principal);
            Assert.IsType<ClaimsPrincipal>(principal);
        }

        [Fact]
        public void GetPrincipalFromExpiredToken_InvalidTokenType_ThrowsSecurityTokenException()
        {
            // Act & Assert
            Assert.ThrowsAny<Exception>(() => target.GetPrincipalFromExpiredToken("invalidtoken"));
        }

        /// <summary>
        /// Create an expired or invalid token.
        /// </summary>
        /// <param name="secretKey">Secret key.</param>
        private string CreateExpiredToken(string secretKey)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(secretKey);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[] { new Claim("sub", "test") }),
                Expires = DateTime.UtcNow.AddMinutes(-5),
                NotBefore = DateTime.UtcNow.AddMinutes(-10),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        /// <summary>
        /// Whether the string is a valid base64 string.
        /// </summary>
        /// <param name="base64">The base64 string.<param>
        private bool IsBase64String(string base64)
        {
            Span<byte> buffer = new Span<byte>(new byte[base64.Length]);
            return Convert.TryFromBase64String(base64, buffer, out _);
        }
    }
}

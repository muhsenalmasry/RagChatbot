using System.Security.Claims;

namespace RagChat.Logic
{
    /// <summary>
    /// Application user's token related logic.
    /// </summary>
    public interface ITokenLogic
    {
        /// <summary>
        /// Generates access token.
        /// </summary>
        /// <param name="userClaims">User claims.</param>
        string GenerateAccessToken(List<Claim> userClaims);

        /// <summary>
        /// Generate refresh token.
        /// </summary>
        (string refreshToken, DateTime expiryDate) GenerateRefreshToken();

        /// <summary>
        /// Gets principal from expired token.
        /// </summary>
        /// <param name="token">Token.</param>
        ClaimsPrincipal GetPrincipalFromExpiredToken(string token);
    }
}

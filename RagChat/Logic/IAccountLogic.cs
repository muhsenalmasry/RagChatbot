namespace RagChat.Logic
{
    using RagChatLogic.DTOs;
    using System.Security.Claims;
    using System.Threading.Tasks;

    /// <summary>
    /// User account related logic.
    /// </summary>
    public interface IAccountLogic
    {
        /// <summary>
        /// Gets logged user identifier.
        /// </summary>
        /// <param name="identityUser">Claim principal of identity user.</param>
        Task<string> GetUserId(ClaimsPrincipal identityUser);

        /// <summary>
        /// Creates a new account.
        /// </summary>
        /// <param name="user">User information.</param>
        /// <param name="roleName">Role name.</param>
        Task<RegisterResponse> CreateAccount(User user, string? roleName = null);

        /// <summary>
        /// Logs user in.
        /// </summary>
        /// <param name="login">Login information.</param>
        Task<LoginResponse> LoginAccount(Login login);

        /// <summary>
        /// Refreshes user's access token.
        /// </summary>
        /// <param name="refreshToken">Refresh token.</param>
        Task<string?> Refresh(string refreshToken);
    }
}

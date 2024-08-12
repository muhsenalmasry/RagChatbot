namespace RagChat.Controllers
{
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Mvc;
    using RagChat.Logic;
    using RagChatLogic.DTOs;

    /// <summary>
    /// Application user's account controller.
    /// </summary>
    /// <param name="accountLogic">Account logic.</param>
    public class AccountController(IAccountLogic accountLogic) : ControllerBase
    {
        /// <summary>
        /// Handles user's registeration.
        /// </summary>
        /// <param name="user">User.</param>
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] User user)
        {
            return Ok(await accountLogic.CreateAccount(user));
        }

        /// <summary>
        /// Handles user logging in.
        /// </summary>
        /// <param name="loginInfo">Login information.</param>
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] Login loginInfo)
        {
            var response = await accountLogic.LoginAccount(loginInfo);

            return string.IsNullOrEmpty(response.AccessToken) && string.IsNullOrEmpty(response.RefreshToken)
                ? Unauthorized()
                : Ok(response);
        }

        /// <summary>
        /// Handles user logging in.
        /// </summary>
        /// <param name="loginInfo">Login information.</param>
        [HttpPost("logout")]
        public async Task<IActionResult> Logout([FromBody] Login loginInfo)
        {
            await HttpContext.SignOutAsync();

            return Ok();
        }

        /// <summary>
        /// Handles expired token refreshing.
        /// </summary>
        /// <param name="refreshToken">Refresh token.</param>
        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh([FromBody] string refreshToken)
        {
            var response = await accountLogic.Refresh(refreshToken);

            return string.IsNullOrEmpty(response)
                ? Unauthorized()
                : Ok(response);
        }
    }
}

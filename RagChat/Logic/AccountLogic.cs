namespace RagChat.Logic
{
    using Microsoft.AspNetCore.Identity;
    using RagChat.Models;
    using RagChatLogic.DTOs;
    using System;
    using System.Security.Claims;
    using System.Threading.Tasks;

    /// <summary>
    /// User account related logic.
    /// </summary>
    public class AccountLogic : IAccountLogic
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ITokenLogic _tokenLogic;
        private readonly RagChatbotDbContext _dbContext;

        /// <summary>
        /// User account related logic.
        /// </summary>
        /// <param name="userManager"></param>
        /// <param name="roleManager"></param>
        /// <param name="tokenLogic"></param>
        /// <param name="dbContext"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public AccountLogic(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            ITokenLogic tokenLogic,
            RagChatbotDbContext dbContext)
        {
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            _roleManager = roleManager ?? throw new ArgumentNullException(nameof(roleManager));
            _tokenLogic = tokenLogic ?? throw new ArgumentNullException(nameof(tokenLogic));
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        /// <summary>
        /// Gets logged user identifier.
        /// </summary>
        /// <param name="identityUser">Claim principal of identity user.</param>
        public async Task<string> GetUserId(ClaimsPrincipal identityUser)
        {
            ApplicationUser user = await _userManager.GetUserAsync(identityUser);
            
            return user?.Id ?? string.Empty;
        }

        public async Task<RegisterResponse> CreateAccount(User user, string? roleName = null)
        {
            try
            {
                if (user is null)
                {
                    return new RegisterResponse { IsSuccessful = false, Message = "User data is empty" };
                }

                var newUser = new ApplicationUser()
                {
                    FullName = $"{user.FirstName} {user.LastName}",
                    Email = user.Email,
                    PasswordHash = user.Password,
                    UserName = user.Email,
                };

                var existingUser = await _userManager.FindByEmailAsync(newUser.Email);
                if (existingUser is not null)
                {
                    return new RegisterResponse { IsSuccessful = false, Message = "User with provided email already exist!" };
                }

                IdentityResult? createdUser = await _userManager.CreateAsync(newUser, user.Password);
                if (!createdUser.Succeeded)
                {
                    return new RegisterResponse { IsSuccessful = false, Message = "Error occured. Please try again!" };
                }

                if (!string.IsNullOrEmpty(roleName))
                {
                    IdentityRole? checkRole = await _roleManager.FindByNameAsync(roleName);
                    if (checkRole is null)
                    {
                        await _roleManager.CreateAsync(new IdentityRole { Name = roleName });
                    }

                    await _userManager.AddToRoleAsync(newUser, roleName);
                }
                else
                {
                    IdentityRole? checkUserRole = await _roleManager.FindByNameAsync("User");
                    if (checkUserRole is null)
                    {
                        await _roleManager.CreateAsync(new IdentityRole { Name = "User" });
                    }

                    await _userManager.AddToRoleAsync(newUser, "User");
                }

                return new RegisterResponse { IsSuccessful = true, Message = "User data is empty" };
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<LoginResponse> LoginAccount(Login login)
        {
            if (login is null)
            {
                return new LoginResponse();
            }

            ApplicationUser? user = await _userManager.FindByEmailAsync(login.Email);
            if (user is null)
            {
                return new LoginResponse();
            }

            bool isPasswordValid = await _userManager.CheckPasswordAsync(user, login.Password);
            if (!isPasswordValid)
            {
                return new LoginResponse();
            }

            var userRoles = await _userManager.GetRolesAsync(user);
            string accessToken = _tokenLogic.GenerateAccessToken(this.GetUserClaims(user, userRoles.First()));
            (string refreshToken, DateTime expiryDate) = _tokenLogic.GenerateRefreshToken();

            await _dbContext.RefreshTokens.AddAsync(new RefreshToken
            {
                UserId = user.Id,
                Token = refreshToken,
                ExpiryDate = expiryDate,
            });
            _dbContext.SaveChanges();

            return new LoginResponse { AccessToken = accessToken, RefreshToken = refreshToken };
        }

        /// <summary>
        /// Refreshes expired access token.
        /// </summary>
        /// <param name="refreshToken">Refresh token.</param>
        public async Task<string?> Refresh(string refreshToken)
        {
            var storedRefreshToken = _dbContext.RefreshTokens.FirstOrDefault(t => t.Token == refreshToken && t.ExpiryDate > DateTime.Now);
            if (storedRefreshToken is null)
            {
                return null;
            }

            ApplicationUser? user = await _userManager.FindByIdAsync(storedRefreshToken.UserId);
            if (user is null)
            {
                return null;
            }

            var userRoles = await _userManager.GetRolesAsync(user);

            return _tokenLogic.GenerateAccessToken(this.GetUserClaims(user, userRoles.First()));
        }

        private List<Claim> GetUserClaims(ApplicationUser user, string userRole)
        {
            return new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.FullName),
                new Claim(ClaimTypes.Email, user.Email!),
                new Claim(ClaimTypes.Role, userRole),
            };
        }
    }
}

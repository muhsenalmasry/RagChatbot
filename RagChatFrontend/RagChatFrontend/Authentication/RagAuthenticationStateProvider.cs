namespace RagChatFrontend.Authentication
{
    using Blazored.LocalStorage;
    using Microsoft.AspNetCore.Components.Authorization;
    using RagChatLogic.DTOs;
    using System.Security.Claims;
    using System.Text.Json;
    using System.Threading.Tasks;

    /// <summary>
    /// Authentication state provider.
    /// </summary>
    public class RagAuthenticationStateProvider : AuthenticationStateProvider
    {
        private readonly ILocalStorageService _localStorageService;
        private ClaimsPrincipal anonymous = new ClaimsPrincipal(new ClaimsIdentity());

        /// <summary>
        /// Authentication state provider.
        /// </summary>
        /// <param name="localStorageService">Blazored local storage service.</param>
        public RagAuthenticationStateProvider(ILocalStorageService localStorageService)
        {
            _localStorageService = localStorageService; 
        }

        /// <summary>
        /// Gets authentication state.
        /// </summary>
        public async override Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            try
            {
                string? authenticationModel = await _localStorageService.GetItemAsStringAsync("Authentication");
                if (authenticationModel is null)
                {
                    // return await Task.FromResult(new AuthenticationState(anonymous));
                    return new AuthenticationState(anonymous);
                }

                return new AuthenticationState(SetClaims(Deserialize(authenticationModel).Email!));
            }
            catch
            {
                return new AuthenticationState(anonymous);
            }
        }

        /// <summary>
        /// Updates authentication state.
        /// </summary>
        /// <param name="authenticationModel">Authentication model.</param>
        public async Task UpdateAuthenticationStateAsync(AuthenticationModel authenticationModel)
        {
            try
            {
                var claimsPrincipal = new ClaimsPrincipal();
                if (authenticationModel is not null)
                {
                    claimsPrincipal = SetClaims(authenticationModel.Email!);
                    await _localStorageService.SetItemAsStringAsync("Authentication", Serialize(authenticationModel));
                }
                else
                {
                    await _localStorageService.RemoveItemAsync("Authentication");
                    claimsPrincipal = anonymous;
                }
                NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(claimsPrincipal)));
            }
            catch
            {
                await Task.FromResult(new AuthenticationState(anonymous));
            }
        }

        /// <summary>
        /// Notifies authentication state changed.
        /// </summary>
        /// <param name="user">User claims principal.</param>
        public void NotifyAuthenticationStateChanged(ClaimsPrincipal user)
        {
            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(user)));
        }

        /// <summary>
        /// Sets user claims.
        /// </summary>
        /// <param name="email">User's email address.</param>
        private ClaimsPrincipal SetClaims(string email)
        {
            return new ClaimsPrincipal(
                new ClaimsIdentity(
                    new List<Claim> { new Claim(ClaimTypes.Name, email) },
                    "CustomAuth"));
        }

        /// <summary>
        /// Deserializes string.
        /// </summary>
        /// <param name="serializedString">The serialized string.</param>
        private AuthenticationModel Deserialize(string serializedString)
        {
            return JsonSerializer.Deserialize<AuthenticationModel>(serializedString)!;
        }

        /// <summary>
        /// Serliazes authentication model.
        /// </summary>
        /// <param name="authenticationModel">Authentication model.</param>
        /// <returns></returns>
        private string Serialize(AuthenticationModel authenticationModel)
        {
            return JsonSerializer.Serialize(authenticationModel)!;
        }
    }
}

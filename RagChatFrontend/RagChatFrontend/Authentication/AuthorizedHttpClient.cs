namespace RagChatFrontend.Authentication
{
    using Blazored.LocalStorage;
    using Microsoft.AspNetCore.Components;
    using Microsoft.AspNetCore.Components.Authorization;
    using Newtonsoft.Json.Linq;
    using RagChatLogic.DTOs;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Net.Http.Json;
    using System.Security.Claims;
    using System.Text;
    using System.Text.Json;
    using NewtonJson = Newtonsoft.Json;

    /// <summary>
    /// Http client with authorization.
    /// </summary>
    public class AuthorizedHttpClient : IAuthorizedHttpClient
    {
        private HttpClient _httpClient;
        private readonly ILocalStorageService _localStorageService;
        private readonly NavigationManager _navigationManager;
        private readonly AuthenticationStateProvider _authenticationStateProvider;

        /// <summary>
        /// Http client with authorization.
        /// </summary>
        /// <param name="localStorageService">Blazored local storage service.</param>
        public AuthorizedHttpClient(
            HttpClient httpClient,
            ILocalStorageService localStorageService,
            NavigationManager navigationManager,
            AuthenticationStateProvider authenticationStateProvider)
        {
            _httpClient = httpClient;
            _localStorageService = localStorageService;
            _navigationManager = navigationManager;
            _authenticationStateProvider = authenticationStateProvider;
        }

        /// <summary>
        /// Makes an authorized http request to the server.
        /// </summary>
        /// <param name="senderUrl">Url of the sender.</param>
        /// <param name="url">Url of the endpoint.</param>
        /// <param name="method">Http method.</param>
        /// <param name="content">Http content.</param>
        public async Task<HttpResponseMessage> SendAsync(string senderUrl, string url, HttpMethod method, HttpContent? content = null)
        {
            var authenticationState = await _localStorageService.GetItemAsStringAsync("Authentication");
            var authenticationModel = JsonSerializer.Deserialize<AuthenticationModel>(authenticationState);

            // Set request headers.
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authenticationModel.AccessToken);

            // Send request.
            var response = content != null
                ? await _httpClient.SendAsync(new HttpRequestMessage(method, url) { Content = content })
                : await _httpClient.SendAsync(new HttpRequestMessage(method, url));

            if(response.StatusCode == HttpStatusCode.Unauthorized)
            {
                var jsonContent = new StringContent(NewtonJson.JsonConvert.SerializeObject(authenticationModel.RefreshToken), Encoding.UTF8, "application/json");
                var refreshTokenResponse = await _httpClient.PostAsync("/refresh", jsonContent);
                if (refreshTokenResponse.StatusCode == HttpStatusCode.Unauthorized)
                {
                    _navigationManager.NavigateTo($"/login?redirecturl={senderUrl}");
                }
                else
                {
                    var accessToken = await refreshTokenResponse.Content.ReadAsStringAsync();
                    authenticationModel = new AuthenticationModel
                    {
                        AccessToken = accessToken.Trim('"'),
                        RefreshToken = authenticationModel.RefreshToken,
                        Email = authenticationModel.Email,
                    };
                    var claimsPrincipal = SetClaims(authenticationModel.Email!);
                    await _localStorageService.SetItemAsStringAsync("Authentication", JsonSerializer.Serialize(authenticationModel));
                    ((RagAuthenticationStateProvider)_authenticationStateProvider).NotifyAuthenticationStateChanged(claimsPrincipal);

                    // Set request headers.
                    _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authenticationModel.AccessToken);

                    // Send request.
                    response = await _httpClient.SendAsync(new HttpRequestMessage(method, url) { Content = content });
                }
            }

            return response;
        }

        /// <summary>
        /// Sends am authroized http request with form data.
        /// </summary>
        /// <param name="senderUrl">Url of the sender.</param>
        /// <param name="url">Url of the endpoint.</param>
        /// <param name="method">Http method.</param>
        /// <param name="formData">Multipart content (form files).</param>
        public async Task<HttpResponseMessage> SendAsync(string senderUrl, string url, HttpMethod method, MultipartFormDataContent formData)
        {
            return await SendAsync(senderUrl, url, method, (HttpContent)formData);
        }

        /// <summary>
        /// Sends am authroized http request with form data.
        /// </summary>
        /// <param name="senderUrl">Url of the sender.</param>
        /// <param name="url">Url of the endpoint.</param>
        /// <param name="method">Http method.</param>
        /// <param name="formData">Multipart content (form files).</param>
        public async Task<HttpResponseMessage> SendAsync(string senderUrl, string url, HttpMethod method, Dictionary<string, byte[]> formFiles, object data)
        {
            var formData = new MultipartFormDataContent();

            if (data != null)
            {
                var type = data.GetType();
                var properties = type.GetProperties();

                string? json;
                foreach (var property in properties)
                {
                    var propertyName = property.Name;
                    var propertyValue = property.GetValue(data);

                    formData.Add(new StringContent(propertyValue.ToString()), propertyName);
                }
            }

            foreach (var formFile in formFiles)
            {
                formData.Add(new ByteArrayContent(formFile.Value), "files", formFile.Key);
            }

            return await SendAsync(senderUrl, url, method, formData);
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
    }
}

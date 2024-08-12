using RagChatLogic.DTOs;

namespace RagChatFrontend.Authentication
{
    public interface IAuthorizedHttpClient
    {
        /// <summary>
        /// Makes an authorized http request to the server.
        /// </summary>
        /// <param name="senderUrl">Url of the sender.</param>
        /// <param name="url">Url of the endpoint.</param>
        /// <param name="method">Http method.</param>
        /// <param name="content">Http content.</param>
        Task<HttpResponseMessage> SendAsync(string senderUrl, string url, HttpMethod method, HttpContent? content = null);

        /// <summary>
        /// Sends am authroized http request with form data.
        /// </summary>
        /// <param name="senderUrl">Url of the sender.</param>
        /// <param name="url">Url of the endpoint.</param>
        /// <param name="method">Http method.</param>
        /// <param name="formData">Multipart content (form files).</param>
        Task<HttpResponseMessage> SendAsync(string senderUrl, string url, HttpMethod method, MultipartFormDataContent formData);

        /// <summary>
        /// Sends am authroized http request with form data.
        /// </summary>
        /// <param name="senderUrl">Url of the sender.</param>
        /// <param name="url">Url of the endpoint.</param>
        /// <param name="method">Http method.</param>
        /// <param name="formData">Multipart content (form files).</param>
        Task<HttpResponseMessage> SendAsync(string senderUrl, string url, HttpMethod method, Dictionary<string, byte[]> formFiles, object data);
    }
}

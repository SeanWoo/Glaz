using System;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using Glaz.Server.Data.AppSettings;
using Microsoft.Extensions.Options;

namespace Glaz.Server.Services.Helpers
{
    public sealed class VuforiaRequestsManager : IVuforiaRequestsManager
    {
        private const string DefaultMd5HashOfEmptyString = "d41d8cd98f00b204e9800998ecf8427e";

        private readonly VuforiaCredentials _credentials;

        public VuforiaRequestsManager(IOptions<VuforiaCredentials> credentials)
        {
            _credentials = credentials.Value;
        }

        public HttpRequestMessage PrepareRequest(HttpMethod method, string url)
        {
            var request = CreateBaseRequest(method, url);
            SetAuthorizationHeader(request);
            return request;
        }
        public HttpRequestMessage PrepareRequest(HttpMethod method, string url, [NotNull] string json)
        {
            var request = CreateBaseRequest(method, url);
            request.Content = new StringContent(json, Encoding.UTF8, "application/json");
            SetAuthorizationHeader(request);
            return request;
        }
        private HttpRequestMessage CreateBaseRequest(HttpMethod method, string url)
        {
            return new HttpRequestMessage(method, url)
            {
                Headers =
                {
                    Date = DateTimeOffset.UtcNow
                }
            };
        }
        private void SetAuthorizationHeader(HttpRequestMessage request)
        {
            string stringToSign = GetStringToSign(request);
            request.Headers.Authorization = new AuthenticationHeaderValue("VWS", CreateAuthorizationHeader(stringToSign));
        }
        private string GetStringToSign(HttpRequestMessage request)
        {
            string httpMethod = request.Method.Method.ToUpper();
            string date = request.Headers.Date?.UtcDateTime.ToString("R");
            string requestPath = request.RequestUri.AbsolutePath;

            bool isRequestHasContentBody = request.Content != null;
            string contentType = isRequestHasContentBody ? request.Content.Headers.ContentType.MediaType : string.Empty;
            string contentMd5 = isRequestHasContentBody
                ? CreateMd5(request.Content.ReadAsStringAsync().Result)
                : DefaultMd5HashOfEmptyString;
            return $"{httpMethod}\n{contentMd5}\n{contentType}\n{date}\n{requestPath}";
        }
        private string CreateMd5(string input)
        {
            // Use input string to calculate MD5 hash
            using var md5 = MD5.Create();
            var inputBytes = Encoding.UTF8.GetBytes(input);
            var hashBytes = md5.ComputeHash(inputBytes);

            // Convert the byte array to hexadecimal string
            var sb = new StringBuilder();
            foreach (byte hashByte in hashBytes)
            {
                sb.Append(hashByte.ToString("x2"));
            }
            return sb.ToString();
        }
        private string CreateAuthorizationHeader(string stringToSign)
        {
            var hmacSha1Encoder = new HMACSHA1(Encoding.UTF8.GetBytes(_credentials.SecretKey));
            var hmacSha1Hash = hmacSha1Encoder.ComputeHash(Encoding.UTF8.GetBytes(stringToSign));
            string signature = Convert.ToBase64String(hmacSha1Hash);
            return $"{_credentials.AccessKey}:{signature}";
        }
    }
}
using System;
using System.Buffers.Text;
using System.ComponentModel;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Glaz.Server.Data;
using Glaz.Server.Data.AppSettings;
using Glaz.Server.Data.Vuforia;
using Glaz.Server.Data.Vuforia.Responses;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Glaz.Server.Services
{
    public sealed class VuforiaService : IVuforiaService
    {
        private DateTimeOffset _date;

        private const string DefaultMd5HashOfEmptyString = "d41d8cd98f00b204e9800998ecf8427e";
        private const string BaseApiUrl = "https://vws.vuforia.com";

        private readonly JsonSerializerSettings _camelCaseSerializerSettings;

        private readonly HttpClient _httpClient;
        private readonly VuforiaCredentials _credentials;
        private readonly ILogger<IVuforiaService> _logger;

        public VuforiaService(IOptions<VuforiaCredentials> credentials, ILogger<IVuforiaService> logger)
        {
            _camelCaseSerializerSettings = new JsonSerializerSettings
            {
                ContractResolver = new DefaultContractResolver
                {
                    NamingStrategy = new CamelCaseNamingStrategy()
                }
            };
            _credentials = credentials.Value;
            _httpClient = new HttpClient();
            _logger = logger;
        }

        private void SetAuthorizationHeader(HttpRequestMessage request)
        {
            request.Headers.Date = _date;
            _logger.LogWarning(_date.Ticks.ToString());
            request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
            string stringToSign = GetStringToSign(request);
            request.Headers.Authorization = new AuthenticationHeaderValue( "VWS", GetAuthorizationHeader(stringToSign));
        }
        private string GetStringToSign(HttpRequestMessage request)
        {
            string httpMethod = request.Method.Method.ToUpper();
            string date = _date.UtcDateTime.ToString("R");
            _logger.LogWarning(_date.Ticks.ToString());
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
            byte[] inputBytes = Encoding.UTF8.GetBytes(input);
            byte[] hashBytes = md5.ComputeHash(inputBytes);

            // Convert the byte array to hexadecimal string
            StringBuilder sb = new StringBuilder();
            foreach (var hashByte in hashBytes)
            {
                sb.Append(hashByte.ToString("x2"));
            }
            return sb.ToString();
        }
        private string GetAuthorizationHeader(string stringToSign)
        {
            var hmacSha1Encoder = new HMACSHA1(Encoding.UTF8.GetBytes(_credentials.SecretKey));
            var hmacSha1Hash = hmacSha1Encoder.ComputeHash(Encoding.UTF8.GetBytes(stringToSign));
            string signature = Convert.ToBase64String(hmacSha1Hash);
            return $"{_credentials.AccessKey}:{signature}";
        }

        public async Task<string> AddTarget(TargetModel target)
        {
            string json = JsonConvert.SerializeObject(target, _camelCaseSerializerSettings);
            _date = DateTimeOffset.UtcNow;
            _logger.LogWarning(_date.Ticks.ToString());
            var request = new HttpRequestMessage(HttpMethod.Post, $"{BaseApiUrl}/targets")
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json"),
            };
            SetAuthorizationHeader(request);

            _logger.LogInformation(request.Headers.ToString());
            _logger.LogInformation(request.Content.Headers.ToString());
            var response = await _httpClient.SendAsync(request);
            string responseJson = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<CreateTargetResponse>(responseJson).TargetId;
        }

        public async Task<bool> UpdateTarget(string targetId, TargetModel newTarget)
        {
            string json = JsonConvert.SerializeObject(newTarget, _camelCaseSerializerSettings);
            var request = new HttpRequestMessage(HttpMethod.Put, $"{BaseApiUrl}/targets/{targetId}")
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };
            SetAuthorizationHeader(request);

            var response = await _httpClient.SendAsync(request);
            string responseJson = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<CommonResponse>(responseJson).IsSuccess;
        }

        public async Task<bool> DeleteTarget(string targetId)
        {
            var request = new HttpRequestMessage(HttpMethod.Delete, $"{BaseApiUrl}/targets/{targetId}");
            SetAuthorizationHeader(request);

            var response = await _httpClient.SendAsync(request);
            string responseJson = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<CommonResponse>(responseJson).IsSuccess;
        }

        public async Task<TargetRecord> GetTargetRecord(string targetId)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"{BaseApiUrl}/targets/{targetId}");
            SetAuthorizationHeader(request);

            var response = await _httpClient.SendAsync(request);
            string responseJson = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<GetTargetResponse>(responseJson).TargetRecord;
        }
    }
}

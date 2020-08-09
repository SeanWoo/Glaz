using System;
using System.Buffers.Text;
using System.ComponentModel;
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
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace Glaz.Server.Services
{
    public sealed class VuforiaService : IVuforiaService
    {
        private const string DefaultMd5HashOfEmptyString = "d41d8cd98f00b204e9800998ecf8427e";

        private readonly HttpClient _httpClient;
        private readonly VuforiaCredentials _credentials;
        private readonly ApplicationDbContext _context;

        public VuforiaService(IOptions<VuforiaCredentials> credentials,
            ApplicationDbContext context)
        {
            _credentials = credentials.Value;
            _context = context;
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri("https://vws.vuforia.com/")
            };
        }

        private string GetAuthorizationHeader(string stringToSign)
        {
            var hmacSha1Encoder = new HMACSHA1(Encoding.ASCII.GetBytes(_credentials.SecretKey));
            var hmacSha1Hash = hmacSha1Encoder.ComputeHash(Encoding.ASCII.GetBytes(stringToSign));
            string signature = Convert.ToBase64String(hmacSha1Hash);
            return $"VWS {_credentials.AccessKey}:{signature}";
        }
        private string GetStringToSign(HttpRequestMessage request)
        {
            string httpMethod = request.Method.Method.ToUpper();
            string date = request.Headers.Date.ToString();
            string requestPath = request.RequestUri.AbsolutePath;

            bool isRequestHasContentBody = request.Content.Headers.ContentLength > 0;
            string contentType = isRequestHasContentBody ? "application/json" : string.Empty;
            string contentMd5 = isRequestHasContentBody ? request.Content.Headers.ContentMD5.ToString() : DefaultMd5HashOfEmptyString;

            return $"{httpMethod}\n{contentMd5}\n{contentType}\n{date}\n{requestPath}";
        }
        private void SetAuthorizationHeader(HttpRequestMessage request)
        {
            string stringToSign = GetStringToSign(request);
            request.Headers.Authorization = new AuthenticationHeaderValue(GetAuthorizationHeader(stringToSign));
        }

        public async Task<string> AddTarget(TargetModel target)
        {
            string json = JsonConvert.SerializeObject(target);
            var request = new HttpRequestMessage(HttpMethod.Post, "targets")
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };
            SetAuthorizationHeader(request);

            var response = await _httpClient.SendAsync(request);
            string responseJson = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<CreateTargetResponse>(responseJson).TargetId;
        }

        public async Task<bool> UpdateTarget(string targetId, TargetModel newTarget)
        {
            string json = JsonConvert.SerializeObject(newTarget);
            var request = new HttpRequestMessage(HttpMethod.Put, $"targets/{targetId}")
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
            var request = new HttpRequestMessage(HttpMethod.Post, $"targets/{targetId}");
            SetAuthorizationHeader(request);

            var response = await _httpClient.SendAsync(request);
            string responseJson = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<CommonResponse>(responseJson).IsSuccess;
        }
    }
}

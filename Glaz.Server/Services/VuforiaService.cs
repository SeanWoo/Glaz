using System;
using System.Net.Http;
using System.Threading.Tasks;
using Glaz.Server.Data.Vuforia;
using Glaz.Server.Data.Vuforia.Responses;
using Glaz.Server.Services.Helpers;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Glaz.Server.Services
{
    public sealed class VuforiaService : IVuforiaService
    {
        private const string BaseApiUrl = "https://vws.vuforia.com";

        private readonly JsonSerializerSettings _camelCaseSerializerSettings;
        private readonly HttpClient _httpClient;
        private readonly IVuforiaRequestsManager _manager;
        private readonly ILogger<IVuforiaService> _logger;

        public VuforiaService(IVuforiaRequestsManager manager, ILogger<IVuforiaService> logger)
        {
            InitializeJsonSerializerSettings(out _camelCaseSerializerSettings);
            _httpClient = new HttpClient();
            _manager = manager;
            _logger = logger;
        }
        private void InitializeJsonSerializerSettings(out JsonSerializerSettings settings)
        {
            settings = new JsonSerializerSettings
            {
                ContractResolver = new DefaultContractResolver
                {
                    NamingStrategy = new CamelCaseNamingStrategy()
                }
            };
        }

        public async Task<string> AddTarget(TargetModel target)
        {
            if (target is null)
            {
                throw new ArgumentNullException(nameof(target), "TargetModel should not be null");
            }
            
            string json = JsonConvert.SerializeObject(target, _camelCaseSerializerSettings);
            var request = _manager.PrepareRequest(HttpMethod.Get, $"{BaseApiUrl}/targets", json);

            var response = await _httpClient.SendAsync(request);
            string responseJson = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<CreateTargetResponse>(responseJson);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation($"Successfully created new Vuforia target with ID: {result.TargetId}");
            }
            else
            {
                _logger.LogError($"Can't create target:\nCode: {response.StatusCode}\nResponse JSON:\n{responseJson}\nRequestJson{json}");
            }

            return result.TargetId;
        }

        public async Task<bool> UpdateTarget(string targetId, TargetModel newTarget)
        {
            string json = JsonConvert.SerializeObject(newTarget, _camelCaseSerializerSettings);
            var request = _manager.PrepareRequest(HttpMethod.Put, $"{BaseApiUrl}/targets/{targetId}", json);

            var response = await _httpClient.SendAsync(request);
            string responseJson = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<CommonResponse>(responseJson).IsSuccess;
        }

        public async Task<bool> DeleteTarget(string targetId)
        {
            var request = _manager.PrepareRequest(HttpMethod.Delete, $"{BaseApiUrl}/targets/{targetId}");

            var response = await _httpClient.SendAsync(request);
            string responseJson = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<CommonResponse>(responseJson).IsSuccess;
        }

        public async Task<TargetRecord> GetTargetRecord(string targetId)
        {
            var request = _manager.PrepareRequest(HttpMethod.Get, $"{BaseApiUrl}/targets/{targetId}");

            var response = await _httpClient.SendAsync(request);
            string responseJson = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<GetTargetResponse>(responseJson).TargetRecord;
        }
    }
}

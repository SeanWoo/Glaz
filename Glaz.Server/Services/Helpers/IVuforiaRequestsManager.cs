using System.Net.Http;

namespace Glaz.Server.Services.Helpers
{
    public interface IVuforiaRequestsManager
    {
        HttpRequestMessage PrepareRequest(HttpMethod method, string url);
        HttpRequestMessage PrepareRequest(HttpMethod method, string url, string json);
    }
}
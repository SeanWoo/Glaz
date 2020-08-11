using System.Threading.Tasks;
using Glaz.Server.Data.Vuforia;
using Glaz.Server.Data.Vuforia.Responses;

namespace Glaz.Server.Services
{
    public interface IVuforiaService
    {
        Task<string> AddTarget(TargetModel target);
        Task<bool> UpdateTarget(string targetId, TargetModel newTarget);
        Task<bool> DeleteTarget(string targetId);
        Task<TargetRecord> GetTargetRecord(string targetId);
    }
}
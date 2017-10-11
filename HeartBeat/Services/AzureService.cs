using System.Threading.Tasks;
using HeartBeat.Models;
namespace HeartBeat.Services
{
    public interface IAzureService
    {
        Task<bool> SaveHeartBeat(AzureHeartBeat request);
    }
    public class AzureService : IAzureService
    {
        public AzureService()
        {
        }

        /// <summary>
        /// Saves the heart beat.
        /// </summary>
        /// <returns>true, if operation succeeded. false, if operation failed.</returns>
        /// <param name="request">Request is the heartbeat</param>
        public async Task<bool> SaveHeartBeat(AzureHeartBeat request)
        {
            return await Task.FromResult(true);
        }
    }
}

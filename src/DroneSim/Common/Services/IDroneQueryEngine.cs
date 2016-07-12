using Common.Model;
using Microsoft.ServiceFabric.Services.Remoting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Services
{
    public interface IDroneQueryEngine : IService
    {
        Task SubscribeSwarmAsync(long swarmId);
        Task UnsubscribeSwarmAsync(long swarmId);
        Task<IEnumerable<DroneStateSnapshot>> GetDronesAsync();
        Task<IEnumerable<long>> GetSwarmIdsAsync();
    }
}

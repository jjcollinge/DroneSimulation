using Drones.Shared;
using Microsoft.ServiceFabric.Services.Remoting;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Drones.Shared
{
    public interface IDroneManager : IService
    {
        Task<List<DronePayload>> GetDronesAsync();
        Task<DronePayload> GetDroneAsync(string id);
        Task AddDroneAsync(string id, DroneState drone);
        Task RemoveDroneAsync(string id);
        Task UpdateDroneAsync(string id, DroneState drone);
        Task<string> GenerateDroneIdAsync();
    }
}

using Microsoft.ServiceFabric.Services.Remoting;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Drones.Shared
{
    public interface IDroneRegistry : IService
    {
        Task<ConcurrentBag<string>> GetDronesAsync();
        Task AddDroneAsync(string droneId);
        Task RemoveDroneAsync(string droneId);
        Task<bool> ContainsDroneIdAsync(string droneId);
        Task<long> GetDroneCountAsync();
    }
}

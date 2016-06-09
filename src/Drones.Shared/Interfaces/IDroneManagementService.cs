using Drones.Shared;
using Microsoft.ServiceFabric.Services.Remoting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Drones.Shared
{
    public interface IDroneManagementService : IService
    {
        Task<IList<IDroneActor>> GetDronesAsync();
        Task<IDroneActor> GetDroneAsync(string id);
        Task AddDroneAsync(string id, DroneModel drone);
        Task RemoveDroneAsync(string id);
        Task<string> GenerateDroneIdAsync();
    }
}

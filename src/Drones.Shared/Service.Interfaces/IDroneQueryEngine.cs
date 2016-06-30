using Microsoft.ServiceFabric.Services.Remoting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Drones.Shared
{
    public interface IDroneQueryEngine : IService
    {
        Task PublishDroneState(string id, DroneState state);
        Task<Dictionary<string, DroneState>> GetDroneMap();
    }
}

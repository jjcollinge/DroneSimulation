using DroneActor.Interfaces;
using Microsoft.ServiceFabric.Services.Remoting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DroneQueryService.Interfaces
{
    public interface IDroneQueryService : IService
    {
        Task<IList<IDroneActor>> GetDrones();
        Task<IDroneActor> GetDrone(string id);
    }
}

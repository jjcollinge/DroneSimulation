using Drones.Shared.Events;
using Drones.Shared.Model;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Services.Remoting;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Drones.Shared.Services
{
    public interface IEventStore : IService
    {
        Task RegisterSwarmAsync(ActorId swarmId);
        Task UnregisterSwarmAsync(ActorId swarmId);
        Task<List<DroneTransformedEvent>> GetEventsAsync();
        Task<List<DroneTransformedEvent>> GetEventsByDroneAsync(ActorId droneId);
        Task<List<ActorId>> GetRegisteredSwarmsAsync();
    }
}

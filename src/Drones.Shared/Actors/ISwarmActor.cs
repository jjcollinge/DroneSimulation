using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Actors;
using Drones.Shared.Events;

namespace Drones.Shared.Actors
{
    public interface ISwarmActor : IActor, IActorEventPublisher<ISwarmEvents>
    {
        Task AddDroneAsync(String droneId);
        Task RemoveDroneAsync(String droneId);
        Task<List<String>> GetDronesAsync();
    }
}

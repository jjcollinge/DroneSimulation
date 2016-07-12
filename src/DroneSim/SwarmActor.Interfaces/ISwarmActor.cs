using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Actors;
using Common.Events;

namespace SwarmActor.Interfaces
{
    public interface ISwarmActor : IActor, IActorEventPublisher<ISwarmActorEvents>
    {
        Task AddDroneAsync(long droneId);
        Task RemoveDroneAsync(long droneId);
        Task<List<long>> GetDronesAsync();
    }
}

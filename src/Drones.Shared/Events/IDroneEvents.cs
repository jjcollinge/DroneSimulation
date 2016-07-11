using Drones.Shared.Model;
using Microsoft.ServiceFabric.Actors;
using System;

namespace Drones.Shared.Events
{
    public interface IDroneEvents : IActorEvents
    {
        void DroneTransformed(ActorId droneId, DroneTransformedEvent evt);
    }
}

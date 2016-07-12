using Microsoft.ServiceFabric.Actors;
using System;

namespace Drones.Shared.Events
{
    public interface ISwarmEvents : IActorEvents
    {
        void DroneAdded(String swarmId, String droneId);
        void DroneRemoved(String swarmId, String droneId);
    }
}

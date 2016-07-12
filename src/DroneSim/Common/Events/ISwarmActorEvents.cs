using Microsoft.ServiceFabric.Actors;

namespace Common.Events
{
    public interface ISwarmActorEvents : IActorEvents
    {
        void DroneAdded(long droneId);
        void DroneRemoved(long droneId);
    }
}

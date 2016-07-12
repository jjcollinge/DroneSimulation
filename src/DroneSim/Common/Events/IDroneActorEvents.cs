using Common.Model;
using Microsoft.ServiceFabric.Actors;

namespace Common.Events
{
    public interface IDroneActorEvents : IActorEvents
    {
        void DroneUpdated(long droneId, DroneState state, Vector3 transform);
    }
}

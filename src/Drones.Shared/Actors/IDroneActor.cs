using System.Threading.Tasks;
using Microsoft.ServiceFabric.Actors;
using Drones.Shared.Events;
using Drones.Shared.Model;

namespace Drones.Shared.Actors
{
    public interface IDroneActor : IActor, IActorEventPublisher<IDroneEvents>
    {
        Task MoveAsync(DroneTransform transform);
    }
}

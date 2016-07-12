using System.Threading.Tasks;
using Microsoft.ServiceFabric.Actors;
using Common.Model;
using Common.Events;

namespace DroneActor.Interfaces
{
    public interface IDroneActor : IActor, IActorEventPublisher<IDroneActorEvents>
    {
        Task MoveAsync(Vector3 transform);
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Runtime;
using Microsoft.ServiceFabric.Actors.Client;
using DroneActor.Interfaces;
using Common.Model;
using Common.Events;

namespace DroneActor
{
    [StatePersistence(StatePersistence.Persisted)]
    internal class DroneActor : Actor, IDroneActor
    {
        private readonly static string StateKey = "State";

        public async Task MoveAsync(Vector3 transform)
        {
            var state = await StateManager.GetOrAddStateAsync<DroneState>(StateKey, new DroneState());
            state.Position += transform;
            await StateManager.SetStateAsync<DroneState>(StateKey, state);

            ActorEventSource.Current.ActorMessage(this, $"State Updated: [{state.Position.X},{state.Position.Y},{state.Position.Z}]");

            var ev = GetEvent<IDroneActorEvents>();
            ev.DroneUpdated(Id.GetLongId(), state, transform);
        }
    }
}

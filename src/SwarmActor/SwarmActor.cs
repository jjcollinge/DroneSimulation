using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Actors.Runtime;
using Drones.Shared.Events;
using Drones.Shared.Actors;

namespace SwarmActor
{
    [StatePersistence(StatePersistence.Persisted)]
    internal class SwarmActor : Actor, ISwarmActor
    {
        private List<String> _droneIds;

        public SwarmActor()
        {
            _droneIds = new List<String>();
        }

        public Task AddDroneAsync(String droneId)
        {
            _droneIds.Add(droneId);

            // Publish event
            var ev = GetEvent<ISwarmEvents>();
            ev.DroneAdded(this.Id.GetStringId(), droneId);

            return Task.FromResult(true);
        }

        public Task<List<String>> GetDronesAsync()
        {
            return Task.FromResult(_droneIds);
        }

        public Task RemoveDroneAsync(String droneId)
        {
            _droneIds.Remove(droneId);

            // Publish event
            var ev = GetEvent<ISwarmEvents>();
            ev.DroneRemoved(this.Id.GetStringId(), droneId);

            return Task.FromResult(true);
        }
    }
}

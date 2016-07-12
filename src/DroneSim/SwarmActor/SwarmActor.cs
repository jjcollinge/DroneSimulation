using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Actors.Runtime;
using SwarmActor.Interfaces;
using Common.Events;

namespace SwarmActor
{
    [StatePersistence(StatePersistence.Persisted)]
    internal class SwarmActor : Actor, ISwarmActor
    {
        private readonly static string StateKey = "State";

        public async Task AddDroneAsync(long droneId)
        {
            var drones = await StateManager.GetOrAddStateAsync<List<long>>(StateKey, new List<long>());
            drones.Add(droneId);
            await StateManager.SetStateAsync<List<long>>(StateKey, drones);

            var ev = GetEvent<ISwarmActorEvents>();
            ev.DroneAdded(droneId);
        }

        public async Task<List<long>> GetDronesAsync()
        {
            return await StateManager.GetOrAddStateAsync<List<long>>(StateKey, new List<long>());
        }

        public async Task RemoveDroneAsync(long droneId)
        {
            var drones = await StateManager.GetOrAddStateAsync<List<long>>(StateKey, new List<long>());
            drones.Remove(droneId);
            await StateManager.SetStateAsync<List<long>>(StateKey, drones);

            var ev = GetEvent<ISwarmActorEvents>();
            ev.DroneRemoved(droneId);
        }
    }
}

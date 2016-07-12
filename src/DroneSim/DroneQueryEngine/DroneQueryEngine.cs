using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Data.Collections;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using Common.Services;
using Common.Model;
using Microsoft.ServiceFabric.Services.Remoting.Runtime;
using SwarmActor.Interfaces;
using Microsoft.ServiceFabric.Actors.Client;
using Microsoft.ServiceFabric.Actors;
using Common.Events;
using DroneActor.Interfaces;

namespace DroneQueryEngine
{
    internal sealed class DroneQueryEngine : StatefulService, IDroneQueryEngine, IDroneActorEvents
    {
        private Task<IReliableDictionary<string, List<long>>> _swarms => StateManager.GetOrAddAsync<IReliableDictionary<string, List<long>>>("Swarms");
        private Task<IReliableDictionary<long, DroneStateSnapshot>> _droneSnapshots => StateManager.GetOrAddAsync<IReliableDictionary<long, DroneStateSnapshot>>("Drones");

        private readonly static string SwarmListId = "SwarmIds";
        public DroneQueryEngine(StatefulServiceContext context)
            : base(context)
        { }

        public async Task<IEnumerable<DroneStateSnapshot>> GetDronesAsync()
        {
            List<DroneStateSnapshot> droneSnapshots = new List<DroneStateSnapshot>();

            var cancelRequest = new CancellationToken();

            var drones = await _droneSnapshots;
            using (var tx = StateManager.CreateTransaction())
            {
                var enumerable = await drones.CreateEnumerableAsync(tx);
                var enumerator = enumerable.GetAsyncEnumerator();
                while (await enumerator.MoveNextAsync(cancelRequest).ConfigureAwait(false))
                {
                    droneSnapshots.Add(enumerator.Current.Value);
                }
            }

            return droneSnapshots;
        }

        public async Task<IEnumerable<long>> GetSwarmIdsAsync()
        {
            List<long> swarmIds = new List<long>();

            var swarms = await _swarms;
            using (var tx = StateManager.CreateTransaction())
            {
                var cv = await swarms.TryGetValueAsync(tx, SwarmListId);
                if (cv.HasValue)
                    swarmIds.AddRange(cv.Value);
            }

            return swarmIds;
        }

        public async Task SubscribeSwarmAsync(long swarmId)
        {
            var swarm = ActorProxy.Create<ISwarmActor>(new ActorId(swarmId));
            await AddSwarm(swarmId);

            var drones = await swarm.GetDronesAsync();
            foreach (var droneId in drones)
            {
                var drone = ActorProxy.Create<IDroneActor>(new ActorId(droneId));
                await drone.SubscribeAsync<IDroneActorEvents>(this);
            }
        }

        public async Task UnsubscribeSwarmAsync(long swarmId)
        {
            var swarm = ActorProxy.Create<ISwarmActor>(new ActorId(swarmId));
            var drones = await swarm.GetDronesAsync();

            foreach (var droneId in drones)
            {
                var drone = ActorProxy.Create<IDroneActor>(new ActorId(droneId));
                await drone.UnsubscribeAsync<IDroneActorEvents>(this);
            }

            await RemoveSwarm(swarmId);
        }

        private async Task AddSwarm(long swarmId)
        {
            var swarms = await _swarms;
            using (var tx = StateManager.CreateTransaction())
            {
                var cv = await swarms.TryGetValueAsync(tx, SwarmListId);

                if (!cv.HasValue)
                {
                    await swarms.AddAsync(tx, SwarmListId, new List<long>());
                    cv = await swarms.TryGetValueAsync(tx, SwarmListId);
                }

                var oldValue = cv.Value;
                var newValue = new List<long>(cv.Value);
                newValue.Add(swarmId);

                var success = await swarms.TryUpdateAsync(tx, SwarmListId, newValue, oldValue);

                if (success)
                    ServiceEventSource.Current.ServiceMessage(this, $"Successfully subscribed to swarm {swarmId}");
                else
                    ServiceEventSource.Current.ServiceMessage(this, $"Failed to subscribe to swarm {swarmId}");

                await tx.CommitAsync();
            }
        }

        private async Task RemoveSwarm(long swarmId)
        {
            var swarms = await _swarms;
            using (var tx = StateManager.CreateTransaction())
            {
                var cv = await swarms.TryGetValueAsync(tx, SwarmListId);

                if (!cv.HasValue)
                {
                    await swarms.AddAsync(tx, SwarmListId, new List<long>());
                    cv = await swarms.TryGetValueAsync(tx, SwarmListId);
                }

                var oldValue = cv.Value;
                var newValue = new List<long>(cv.Value);
                newValue.Remove(swarmId);
                await swarms.TryUpdateAsync(tx, SwarmListId, newValue, oldValue);
                await tx.CommitAsync();
                ServiceEventSource.Current.ServiceMessage(this, $"Successfully unsubscribed from swarm {swarmId}");
            }
        }

        public async void DroneUpdated(long droneId, DroneState state, Vector3 transform)
        {
            ServiceEventSource.Current.ServiceMessage(this, "Updating Drone Snapshot...");

            var drones = await _droneSnapshots;
            using (var tx = StateManager.CreateTransaction())
            {
                var existingSnapshot = await drones.GetOrAddAsync(tx, droneId, new DroneStateSnapshot());

                var snapshot = new DroneStateSnapshot
                {
                    DroneId = droneId,
                    State = state,
                    TimeStamp = DateTime.UtcNow
                };

                var success = await drones.TryUpdateAsync(tx, droneId, snapshot, existingSnapshot);

                if (success)
                    ServiceEventSource.Current.ServiceMessage(this, "Drone Snapshot Updated Successfully");
                else
                    ServiceEventSource.Current.ServiceMessage(this, "Drone Snapshot Failed to Update");
               
                await tx.CommitAsync();
            }
        }

        protected override IEnumerable<ServiceReplicaListener> CreateServiceReplicaListeners()
        {
            return new[]
            {
                new ServiceReplicaListener( context => this.CreateServiceRemotingListener(context))
            };
        }
    }
}

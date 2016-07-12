using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Data.Collections;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using Drones.Shared.Services;
using Microsoft.ServiceFabric.Services.Remoting.Runtime;
using Drones.Shared.Events;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Client;
using Drones.Shared.Actors;
using Drones.Shared.Model;
using Drones.Shared;

namespace EventStore
{
    internal sealed class EventStore : StatefulService, IEventStore, IDroneEvents
    {
        private Task<IReliableDictionary<ActorId, ISwarmActor>> _swarms => this.StateManager.GetOrAddAsync<IReliableDictionary<ActorId, ISwarmActor>>("Swarms");
        private Task<IReliableDictionary<ActorId, List<DroneTransformedEvent>>> _events => this.StateManager.GetOrAddAsync<IReliableDictionary<ActorId, List<DroneTransformedEvent>>>("Events");

        public EventStore(StatefulServiceContext context)
            : base(context)
        { }

        protected override IEnumerable<ServiceReplicaListener> CreateServiceReplicaListeners()
        {
            return new[]
            {
                new ServiceReplicaListener(context => this.CreateServiceRemotingListener(context))
            };
        }

        public async Task RegisterSwarmAsync(ActorId swarmId)
        {
            await TrackSwarmId(swarmId);
            await SubscribeToDronesInSwarm(swarmId);
        }

        public async Task UnregisterSwarmAsync(ActorId swarmId)
        {
            await UnsubscribeFromDronesInSwarm(swarmId);
            await UntrackSwarmId(swarmId);
        }

        public async Task<List<DroneTransformedEvent>> GetEventsAsync()
        {
            List<DroneTransformedEvent> eventList = new List<DroneTransformedEvent>();

            var events = await _events;
            using (var tx = StateManager.CreateTransaction())
            {
                var enumerable = await events.CreateEnumerableAsync(tx);
                var e = enumerable.GetAsyncEnumerator();
                while (await e.MoveNextAsync(new CancellationToken()).ConfigureAwait(false))
                {
                    // Get each drones event list
                    var droneEventList = e.Current.Value;
                    foreach (var droneEvent in droneEventList)
                    {
                        // Add them to the aggregate list
                        eventList.Add(droneEvent);
                    }
                }
            }

            return eventList;
        }

        public async Task<List<DroneTransformedEvent>> GetEventsByDroneAsync(ActorId droneId)
        {
            var events = await _events;
            using (var tx = StateManager.CreateTransaction())
            {
                var enumerable = await events.CreateEnumerableAsync(tx);
                var e = enumerable.GetAsyncEnumerator();
                while (await e.MoveNextAsync(new CancellationToken()).ConfigureAwait(false))
                {
                    if (e.Current.Key == droneId)
                    {
                        return e.Current.Value;
                    }
                }
            }
            return new List<DroneTransformedEvent>();
        }

        public async Task<List<ActorId>> GetRegisteredSwarmsAsync()
        {
            List<ActorId> swarmIds = new List<ActorId>();

            var swarms = await _swarms;

            using (var tx = StateManager.CreateTransaction())
            {
                var enumberable = await swarms.CreateEnumerableAsync(tx);
                var e = enumberable.GetAsyncEnumerator();
                while (await e.MoveNextAsync(new CancellationToken()).ConfigureAwait(false))
                {
                    swarmIds.Add(e.Current.Key);
                }
            }

            return swarmIds;
        }

        private async Task UntrackSwarmId(ActorId swarmId)
        {
            var swarms = await _swarms;
            using (var tx = StateManager.CreateTransaction())
            {
                await swarms.TryRemoveAsync(tx, swarmId);
                await tx.CommitAsync();
            }
        }

        private async Task UnsubscribeFromDronesInSwarm(ActorId swarmId)
        {
            var swarm = ActorProxy.Create<ISwarmActor>(swarmId);
            var drones = await swarm.GetDronesAsync();

            // Get all drones from swarm and unsubscribe from their events
            foreach (var droneId in drones)
            {
                var drone = ActorProxy.Create<IDroneActor>(new ActorId(droneId));
                await drone.UnsubscribeAsync<IDroneEvents>(this);
            }
        }

        private async Task SubscribeToDronesInSwarm(ActorId swarmId)
        {
            var swarm = ActorProxy.Create<ISwarmActor>(swarmId);
            var drones = await swarm.GetDronesAsync();
            foreach (var droneId in drones)
            {
                var drone = ActorProxy.Create<IDroneActor>(new ActorId(droneId));
                await drone.SubscribeAsync<IDroneEvents>(this);
            }
        }

        private async Task TrackSwarmId(ActorId swarmId)
        {
            var swarms = await _swarms;
            using (var tx = StateManager.CreateTransaction())
            {
                await swarms.TryAddAsync(tx, swarmId, null);
                await tx.CommitAsync();
            }
        }

        public async void DroneTransformed(ActorId droneId, DroneTransformedEvent evt)
        {
            var events = await _events;
            using (var tx = StateManager.CreateTransaction())
            {
                var res = await events.TryGetValueAsync(tx, droneId);
                if (res.HasValue)
                {
                    var droneEvents = res.Value;
                    droneEvents.Add(evt);
                    var success = await events.TryUpdateAsync(tx, droneId, droneEvents, droneEvents);
                    if (success)
                        ServiceEventSource.Current.ServiceMessage(this, $"Succesfully updated events for drone with id: {droneId}");
                    else
                        ServiceEventSource.Current.ServiceMessage(this, $"Failed to update events for drone with id: {droneId}");
                }
                await tx.CommitAsync();
            }
        }
    }
}

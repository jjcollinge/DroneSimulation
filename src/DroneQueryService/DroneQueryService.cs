using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Data.Collections;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using Microsoft.ServiceFabric.Services.Remoting.FabricTransport.Runtime;
using DroneQueryService.Interfaces;
using DroneActor.Interfaces;
using Microsoft.ServiceFabric.Actors.Client;
using Microsoft.ServiceFabric.Actors;
using System.Collections.Concurrent;

namespace DroneQueryService
{

    internal sealed class DroneQueryService : StatefulService, IDroneQueryService
    {
        private const string DRONES_COLLECTION_IDENTIFIER = "DRONES";
        private Task<IReliableDictionary<string, string>> _drones => StateManager.GetOrAddAsync<IReliableDictionary<string, string>>(DRONES_COLLECTION_IDENTIFIER);

        public DroneQueryService(StatefulServiceContext context)
            : base(context)
        { }

        public async Task<IDroneActor> GetDrone(string id)
        {
            IDroneActor drone = null;

            using (var tx = this.StateManager.CreateTransaction())
            {
                var droneStore = await _drones;
                var droneId = await (await droneStore.CreateLinqAsyncEnumerable(tx))
                    .Where(o => o.Key == id)
                    .Select(o => o.Value)
                    .First();

                drone = ActorProxy.Create<IDroneActor>(new ActorId(droneId));
            }

            return drone;
        }

        public async Task<IList<IDroneActor>> GetDrones()
        {
            ConcurrentBag<IDroneActor> drones = new ConcurrentBag<IDroneActor>();

            using (var tx = this.StateManager.CreateTransaction())
            {
                var droneStore = await _drones;
                var droneIds = await (await droneStore.CreateLinqAsyncEnumerable(tx))
                    .Select(o => o.Value)
                    .ToList();

                Parallel.For(0, droneIds.Count, i =>
                {
                    var drone = ActorProxy.Create<IDroneActor>(new ActorId(i));
                    drones.Add(drone);
                });
            }

            return drones.ToList();
        }

        protected override IEnumerable<ServiceReplicaListener> CreateServiceReplicaListeners()
        {
            return new[]{
                new ServiceReplicaListener(
                    (context) => new FabricTransportServiceRemotingListener(context, this))};
        }

        //protected override async Task RunAsync(CancellationToken cancellationToken)
        //{

        //}
    }
}

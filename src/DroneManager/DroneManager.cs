using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using Drones.Shared;
using System.Collections.Concurrent;
using Microsoft.ServiceFabric.Services.Remoting.Runtime;
using Microsoft.ServiceFabric.Actors.Client;
using Microsoft.ServiceFabric.Actors.Query;
using Microsoft.ServiceFabric.Actors;

namespace DroneManager
{
    internal sealed class DroneManager : StatelessService, IDroneManager
    {
        public DroneManager(StatelessServiceContext context)
            : base(context)
        { }

        public async Task AddDroneAsync(string id, DroneState droneModel)
        {
            var drone = DroneServiceFactory.CreateDrone(id);
            var registerTask = RegisterDroneId(id);
            var SetStateTask = drone.SetState(droneModel);
            await Task.WhenAll(registerTask, SetStateTask);
        }

        public async Task<DronePayload> GetDroneAsync(string id)
        {
            var drone = await DroneServiceFactory.GetDroneAsync(id);
            return new DronePayload
            {
                Id = await drone.GetIdAsync(),
                State = await drone.GetState()
            };
        }

        public async Task<ConcurrentBag<DronePayload>> GetDronesAsync()
        {
            var droneServiceProxy = DroneServiceFactory.CreateDroneServiceProxy();
            ContinuationToken continuationToken = null;
            CancellationToken cancellationToken = new CancellationToken();
            PagedResult<ActorInformation> droneIds = null;
            do
            {
                droneIds = await droneServiceProxy.GetActorsAsync(continuationToken, cancellationToken);
                continuationToken = droneIds.ContinuationToken;
            }
            while (continuationToken != null);

            var droneBag = new ConcurrentBag<DronePayload>();
            Parallel.ForEach<ActorInformation>(droneIds.Items, async (droneId) =>
            {
                var drone = ActorProxy.Create<IDroneActor>(droneId.ActorId);
                droneBag.Add(new DronePayload
                {
                    Id = await drone.GetIdAsync(),
                    State = await drone.GetState()
                });
            });

            return droneBag;
        }

        public async Task RemoveDroneAsync(string id)
        {
            var droneRegistry = DroneServiceFactory.CreateDroneRegistry();
            await droneRegistry.RemoveDroneAsync(id);

            var droneServiceProxy = DroneServiceFactory.CreateDroneServiceProxy();
            CancellationToken cancellationToken = new CancellationToken();
            await droneServiceProxy.DeleteActorAsync(new ActorId(id), cancellationToken);
        }

        public async Task UpdateDroneAsync(string id, DroneState updatedState)
        {
            var drone = await DroneServiceFactory.GetDroneAsync(id);
            if (drone != null)
                await drone.SetState(updatedState);
            else
                throw new FabricServiceNotFoundException();
        }

        public Task<string> GenerateDroneIdAsync()
        {
            var id = Guid.NewGuid();
            return Task.FromResult(id.ToString());
        }

        private static async Task RegisterDroneId(string id)
        {
            var droneRegistry = DroneServiceFactory.CreateDroneRegistry();
            await droneRegistry.AddDroneAsync(id);
        }

        protected override IEnumerable<ServiceInstanceListener> CreateServiceInstanceListeners()
        {
            return new[]{
                new ServiceInstanceListener(
                    (context) => this.CreateServiceRemotingListener(context)) };
        }

        public async Task<ConcurrentBag<DronePayload>> GetCachedDronesAsync()
        {
            var queryEngine = DroneServiceFactory.CreateDroneQueryEngine();
            var droneMap = await queryEngine.GetDroneMap();

            var droneBag = new ConcurrentBag<DronePayload>();
            Parallel.ForEach<KeyValuePair<string, DroneState>>(droneMap, (droneKVP) =>
             {
                 droneBag.Add(new DronePayload
                 {
                     Id = droneKVP.Key,
                     State = droneKVP.Value
                 });
             });

            return droneBag;
        }
    }
}

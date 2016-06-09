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
using Microsoft.ServiceFabric.Services.Remoting.FabricTransport.Runtime;
using Microsoft.ServiceFabric.Services.Remoting.Runtime;
using Microsoft.ServiceFabric.Actors.Client;
using Microsoft.ServiceFabric.Actors;

namespace DroneManagementService
{
    /// <summary>
    /// An instance of this class is created for each service instance by the Service Fabric runtime.
    /// </summary>
    internal sealed class DroneManagementService : StatelessService, IDroneManager
    {
        private static IDroneRegistry _droneRegistry;

        public DroneManagementService(StatelessServiceContext context)
            : base(context)
        {
            _droneRegistry = DroneServiceFactory.CreateDroneRegistry();
        }

        public async Task AddDroneAsync(string id, DroneState droneModel)
        {
            var drone = DroneServiceFactory.CreateDrone(id);
            var task0 = RegisterDroneId(id);
            var task1 = drone.SetState(droneModel);
            await Task.WhenAll(task0, task1);
        }

        public async Task<IDroneActor> GetDroneAsync(string id)
        {
            return await DroneServiceFactory.GetDroneAsync(id);
        }

        public async Task<IList<IDroneActor>> GetDroneAsync()
        {
            var droneRegistry = DroneServiceFactory.CreateDroneRegistry();
            var droneIds = (await droneRegistry.GetDronesAsync()).ToList();

            ConcurrentBag<IDroneActor> drones = null;
            if(droneIds.Count > 0)
            {
                Parallel.For(0, droneIds.Count, async i =>
                {
                    var drone = await DroneServiceFactory.GetDroneAsync(droneIds[i]);
                    drones.Add(drone);
                });
            }
            else
            {
                drones = new ConcurrentBag<IDroneActor>();
            }

            return drones.ToList() as IList<IDroneActor>;
        }

        public Task RemoveDroneAsync(string id)
        {
            throw new NotImplementedException();
        }

        public async Task<string> GenerateDroneIdAsync()
        {
            var count = await _droneRegistry.GetDroneCountAsync();
            return count.ToString();
        }

        private static async Task RegisterDroneId(string id)
        {
            await _droneRegistry.AddDroneAsync(id);
        }

        protected override IEnumerable<ServiceInstanceListener> CreateServiceInstanceListeners()
        {
            return new[]{
                new ServiceInstanceListener(
                    (context) => this.CreateServiceRemotingListener(context)) };
        }
    }
}

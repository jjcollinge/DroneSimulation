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

namespace DroneManagementService
{
    /// <summary>
    /// An instance of this class is created for each service instance by the Service Fabric runtime.
    /// </summary>
    internal sealed class DroneManagementService : StatelessService, IDroneManagementService
    {
        private static IDroneRegistry _droneRegistry;

        public DroneManagementService(StatelessServiceContext context)
            : base(context)
        {
            _droneRegistry = DroneServiceFactory.CreateDroneRegistry();
        }

        public async Task AddDroneAsync(string id, DroneModel droneModel)
        {
            var drone = DroneServiceFactory.CreateDrone(id);
            var task0 = RegisterDroneId(id);
            var task1 = drone.SetState(droneModel);
            await Task.WhenAll(task0, task1);
        }

        public async Task<IDroneActor> GetDroneAsync(string id)
        {
            return await DroneServiceFactory.GetDrone(id);
        }

        public async Task<IList<IDroneActor>> GetDronesAsync()
        {
            var droneRegistry = DroneServiceFactory.CreateDroneRegistry();
            var droneIds = droneRegistry.GetDronesAsync().Result.ToList();

            ConcurrentBag<IDroneActor> drones = null;
            Parallel.For(0, droneIds.Count, async i =>
            {
                var drone = await DroneServiceFactory.GetDrone(droneIds[i]);
                drones.Add(drone);
            });

            await Task.WhenAll();
            return drones?.ToList();
        }

        public Task RemoveDroneAsync(string id)
        {
            throw new NotImplementedException();
        }

        private static async Task RegisterDroneId(string id)
        {
            await _droneRegistry.AddDroneAsync(id);
        }

        private static async Task<string> GenerateDroneId()
        {
            var count = await _droneRegistry.GetDroneCountAsync();
            return count.ToString();
        }

        protected override IEnumerable<ServiceInstanceListener> CreateServiceInstanceListeners()
        {
            return new[]{
                new ServiceInstanceListener(
                    (context) => new FabricTransportServiceRemotingListener(context, this))};
        }
    }
}

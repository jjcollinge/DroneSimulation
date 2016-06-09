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
            var task0 = RegisterDroneId(id);
            var task1 = drone.SetState(droneModel);
            await Task.WhenAll(task0, task1);
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

        public async Task<List<DronePayload>> GetDronesAsync()
        {
            var droneRegistry = DroneServiceFactory.CreateDroneRegistry();
            var droneIds = await droneRegistry.GetDronesAsync();

            List<DronePayload> drones = new List<DronePayload>();

            for (int i = 1; i < droneIds.Count + 1; i++)
            {
                var drone = await DroneServiceFactory.GetDroneAsync(droneIds[(i - 1)].ToString());
                drones.Add(new DronePayload
                {
                    Id = await drone.GetIdAsync(),
                    State = await drone.GetState()
                });
            }

            return drones;
        }

        public async Task RemoveDroneAsync(string id)
        {
            var droneRegistry = DroneServiceFactory.CreateDroneRegistry();
            await droneRegistry.RemoveDroneAsync(id);
            await DroneServiceFactory.DeleteDroneAsync(id);
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

    }
}

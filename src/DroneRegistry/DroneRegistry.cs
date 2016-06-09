using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Data.Collections;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using System.Collections.Concurrent;
using Microsoft.ServiceFabric.Services.Remoting.FabricTransport.Runtime;
using Drones.Shared;
using Microsoft.ServiceFabric.Data;

namespace DroneRegistry
{
    /// <summary>
    /// An instance of this class is created for each service replica by the Service Fabric runtime.
    /// </summary>
    public sealed class DroneRegistry : StatefulService, IDroneRegistry
    {
        private const string DICTIONARY_NAME = "DRONES";
        private const string DICTIONARY_KEY = "DRONES_KEY";
        private Task<IReliableDictionary<string, ConcurrentBag<string>>> _drones => this.StateManager.GetOrAddAsync<IReliableDictionary<string, ConcurrentBag<string>>>(DICTIONARY_NAME);

        public DroneRegistry(StatefulServiceContext context)
            : base(context)
        {
            initDrones();
        }

        private async Task initDrones()
        {
            using(var tx = this.StateManager.CreateTransaction())
            {
                var drones = await _drones;
                await drones.AddAsync(tx, DICTIONARY_KEY, new ConcurrentBag<string>());
            }
        }

        public async Task<ConcurrentBag<string>> GetDronesAsync()
        {
            ConcurrentBag<string> droneBag = null;

            using (var tx = this.StateManager.CreateTransaction())
            {
                var drones = await _drones;
                droneBag = await (await drones.CreateLinqAsyncEnumerable(tx))
                    .Where(o => o.Key == DICTIONARY_KEY)
                    .Select(o => o.Value)
                    .First();   
            }

            return droneBag;
        }

        public async Task AddDroneAsync(string droneId)
        {
            using (var tx = this.StateManager.CreateTransaction())
            {
                var drones = await GetDroneBag(tx);
                drones.Add(droneId);
                await tx.CommitAsync();
            }
        }

        public async Task RemoveDroneAsync(string droneId)
        {
            using (var tx = this.StateManager.CreateTransaction())
            {
                var drones = await GetDroneBag(tx);

                // Potentially dangerous
                lock (drones)
                {
                    drones = new ConcurrentBag<string>(drones.Except(new[] { droneId }));
                }

                await tx.CommitAsync();
            }
        }

        public async Task<bool> ContainsDroneIdAsync(string droneId)
        {
            bool contains = false;

            using (var tx = this.StateManager.CreateTransaction())
            {
                var drones = await GetDroneBag(tx);
                contains = drones.Contains(droneId);
            }

            return contains;
        }

        private async Task<ConcurrentBag<string>> GetDroneBag(ITransaction tx)
        {
            var drones = await _drones;
            var droneBag = await (await drones.CreateLinqAsyncEnumerable(tx))
                .Where(o => o.Key == DICTIONARY_KEY)
                .Select(o => o.Value)
                .First();
            return droneBag;
        }

        public async Task<long> GetDroneCountAsync()
        {
            var count = 0L;
            var drones = await _drones;
            using (var tx = this.StateManager.CreateTransaction())
            {
                count = await drones.GetCountAsync(tx);
            }
            return count
        }

        protected override IEnumerable<ServiceReplicaListener> CreateServiceReplicaListeners()
        {
            return new[]{
                new ServiceReplicaListener(
                    (context) => new FabricTransportServiceRemotingListener(context, this))};
        }
    }
}

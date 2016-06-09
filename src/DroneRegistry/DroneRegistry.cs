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
using Microsoft.ServiceFabric.Services.Remoting.Runtime;

namespace DroneRegistry
{
    /// <summary>
    /// An instance of this class is created for each service replica by the Service Fabric runtime.
    /// </summary>
    public sealed class DroneRegistry : StatefulService, IDroneRegistry
    {
        private const string DICTIONARY_NAME = "DRONES";
        private const string DICTIONARY_KEY = "DRONES_KEY";
        private bool _initialised = false;

        private Task<IReliableDictionary<string, List<string>>> _drones => this.StateManager.GetOrAddAsync<IReliableDictionary<string, List<string>>>(DICTIONARY_NAME);

        public DroneRegistry(StatefulServiceContext context)
            : base(context)
        {}

        public async Task<List<string>> GetDronesAsync()
        {
            if (!_initialised) await Initialise();

            List<string> droneList = null;

            using (var tx = this.StateManager.CreateTransaction())
            {
                var drones = await _drones;
                var droneKVP = await (await drones.CreateLinqAsyncEnumerable(tx))
                    .First(o => o.Key == DICTIONARY_KEY);
                droneList = droneKVP.Value;
            }

            return droneList;
        }

        public async Task AddDroneAsync(string droneId)
        {
            if (!_initialised) await Initialise();

            using (var tx = this.StateManager.CreateTransaction())
            {
                var drones = await GetDroneList(tx);
                drones.Add(droneId);
                await tx.CommitAsync();
            }
        }

        public async Task RemoveDroneAsync(string droneId)
        {
            if (!_initialised) await Initialise();

            using (var tx = this.StateManager.CreateTransaction())
            {
                var drones = await GetDroneList(tx);
                drones.Remove(droneId);
                await tx.CommitAsync();
            }
        }

        public async Task<bool> ContainsDroneIdAsync(string droneId)
        {
            if (!_initialised) await Initialise();

            bool contains = false;

            using (var tx = this.StateManager.CreateTransaction())
            {
                var drones = await GetDroneList(tx);
                contains = drones.Contains(droneId);
                await tx.CommitAsync();
            }

            return contains;
        }

        public async Task<long> GetDroneCountAsync()
        {
            if (!_initialised) await Initialise();

            var count = 0L;
            using(var tx = this.StateManager.CreateTransaction())
            {
                var drones = await GetDroneList(tx);
                count = drones.Count;
            }

            return count;
        }

        private async Task<List<string>> GetDroneList(ITransaction tx)
        {
            var drones = await _drones;
            var droneList = await (await drones.CreateLinqAsyncEnumerable(tx))
                .Where(o => o.Key == DICTIONARY_KEY)
                .Select(o => o.Value)
                .First();
            return droneList;
        }

        private async Task Initialise()
        {
            using (var tx = this.StateManager.CreateTransaction())
            {
                var drones = _drones.Result;
                if (drones.GetCountAsync(tx).Result < 1)
                {
                    await drones.AddAsync(tx, DICTIONARY_KEY, new List<string>());
                    await tx.CommitAsync();
                    _initialised = true;
                    ServiceEventSource.Current.Message($"Registry initialised with {drones.GetCountAsync(tx).Result.ToString()} items");
                }
            }
        }

        protected override IEnumerable<ServiceReplicaListener> CreateServiceReplicaListeners()
        {
            return new[]{
                new ServiceReplicaListener(
                    (context) => this.CreateServiceRemotingListener(context)) };
        }
    }
}

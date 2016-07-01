using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using Drones.Shared;
using Microsoft.ServiceFabric.Services.Remoting.Runtime;

namespace DroneQueryEngine
{
    internal sealed class DroneQueryEngine : StatelessService, IDroneQueryEngine
    {
        private Dictionary<string, DroneState> _currentDroneMap;

        public DroneQueryEngine(StatelessServiceContext context)
            : base(context)
        {
            _currentDroneMap = new Dictionary<string, DroneState>();
        }

        public Task<Dictionary<string, DroneState>> GetDroneMap()
        {
            return Task.FromResult(_currentDroneMap);
        }

        public Task PublishDroneState(string id, DroneState state)
        {
            if (_currentDroneMap.ContainsKey(id))
                // Update
                _currentDroneMap[id] = state;
            else
                // Insert
                _currentDroneMap.Add(id, state);
            return Task.FromResult(true);
        }

        protected override IEnumerable<ServiceInstanceListener> CreateServiceInstanceListeners()
        {
            return new[]{
                new ServiceInstanceListener(
                    (context) => this.CreateServiceRemotingListener(context)) };
        }
    }
}

using Drones.Shared;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Client;
using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Remoting.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Drones.Shared
{
    public class DroneServiceFactory
    {
        private static string DRONE_MANAGEMENT_URI = "fabric:/Drones/DroneManager";
        private static string DRONE_REGISTRY_URI = "fabric:/Drones/DroneRegistry";
        private static string DRONE_QUERY_ENGINE_URI = "fabric:/Drones/DroneQueryEngine";

        private static IDroneManager _droneManager;
        private static IDroneRegistry _droneRegistry;
        private static IDroneQueryEngine _droneQueryEngine;

        public DroneServiceFactory()
        { }

        public static IDroneManager CreateDroneManager()
        {
            if(_droneManager == null)
                _droneManager = ServiceProxy.Create<IDroneManager>(new Uri(DRONE_MANAGEMENT_URI));
            return _droneManager;
        }

        public static IDroneRegistry CreateDroneRegistry()
        {
            if(_droneRegistry == null)
                _droneRegistry = ServiceProxy.Create<IDroneRegistry>(new Uri(DRONE_REGISTRY_URI), new ServicePartitionKey(0));
            return _droneRegistry;
        }

        public static IDroneQueryEngine CreateDroneQueryEngine()
        {
            if(_droneQueryEngine == null)
                _droneQueryEngine = ServiceProxy.Create<IDroneQueryEngine>(new Uri(DRONE_QUERY_ENGINE_URI));
            return _droneQueryEngine;
        }

        public async static Task<IDroneActor> GetDroneAsync(string id)
        {
            var droneRegistry = CreateDroneRegistry();
            var droneExists = await droneRegistry.ContainsDroneIdAsync(id);

            IDroneActor drone = null;
            if (droneExists)
                drone = ActorProxy.Create<IDroneActor>(new ActorId(id));

            return drone;
        }

        public static IDroneActor CreateDrone(string id)
        {
            var actor = ActorProxy.Create<IDroneActor>(new ActorId(id));
            return actor;
        }

        public static Task DeleteDroneAsync(string id)
        {
            //var droneToDelate = CreateDrone(id);
            return Task.FromResult(true);
        }
    }
}

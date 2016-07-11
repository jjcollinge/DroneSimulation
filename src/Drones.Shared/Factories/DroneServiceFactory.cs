using Drones.Shared.Actors;
using Drones.Shared.Services;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Client;
using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Remoting.Client;
using System;

namespace Drones.Shared
{
    public class DroneServiceFactory
    {
        private static string EVENT_STORE_URI = "fabric:/Drones/EventStore";

        public DroneServiceFactory()
        { }

        public static IEventStore CreateDroneEventStoreProxy()
        {
            return ServiceProxy.Create<IEventStore>(new Uri(EVENT_STORE_URI), new ServicePartitionKey(1));
        }
    }
}

using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Remoting.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DroneQueryService.Interfaces
{
    public class DroneQueryServiceFactory
    {
        private static string DRONEQUERYSERVICE_URI = "fabric:/drones/DroneQueryService";
        public static IDroneQueryService CreateDroneQueryService()
        {
            return ServiceProxy.Create<IDroneQueryService>(new Uri(DRONEQUERYSERVICE_URI), new ServicePartitionKey(0));
        }
    }
}

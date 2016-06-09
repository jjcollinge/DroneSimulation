using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using System.Net.Http;
using Newtonsoft.Json;
using Drones.Shared;
using System.Net.Http.Headers;

namespace DroneSimulator
{
    internal sealed class DroneSimulator : StatelessService
    {
        private readonly string DRONE_ENDPOINT = "http://localhost:8680/api/drone";
        private readonly TimeSpan SIMULATION_RATE = TimeSpan.FromSeconds(2);

        public DroneSimulator(StatelessServiceContext context)
            : base(context)
        { }

        protected override IEnumerable<ServiceInstanceListener> CreateServiceInstanceListeners()
        {
            return new ServiceInstanceListener[0];
        }

        protected override async Task RunAsync(CancellationToken cancellationToken)
        {
            // Wait for all services to come up
            await Task.Delay(TimeSpan.FromSeconds(30));

            // Generate initial simulation data
            var simulationRegistry = new List<string>();
            string[] ids = { "0" }; //, "1", "2", "3", "4" };
            simulationRegistry.AddRange(ids);

            // Load initial simulation data
            var droneRegistry = DroneServiceFactory.CreateDroneRegistry();
            await droneRegistry.LoadExistingRegistry(simulationRegistry);

            // Simulation loop
            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();

                Parallel.ForEach<string>(ids, async id =>
                {
                    await UpdateDrone(id);
                });

                // Enforce variable simulation rate
                await Task.Delay(SIMULATION_RATE);
            }
        }

        private async Task UpdateDrone(string id)
        {
            var random = new Random();

            var payload = new
            {
                Id = id,
                State = new
                {
                    Longitude = (Decimal)random.NextDouble(),
                    Latitude = (Decimal)random.NextDouble(),
                    Altitude = random.Next(),
                    Heading = random.Next(),
                    Speed = random.Next()
                }
            };

            var jsonPayload = JsonConvert.SerializeObject(payload);
            var content = new StringContent(jsonPayload);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            content.Headers.ContentLength = jsonPayload.Length;

            using (var http = new HttpClient())
            {
                var response = await http.PutAsync(DRONE_ENDPOINT, content);

                if (response.IsSuccessStatusCode)
                {
                    ServiceEventSource.Current.Message($"Successful update to drone {id}");
                }
                else
                {
                    ServiceEventSource.Current.Message($"Failed to update to drone {id}");
                }
            }
        }
    }
}

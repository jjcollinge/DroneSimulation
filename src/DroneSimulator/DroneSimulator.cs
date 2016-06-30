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
        private readonly int SIMULATION_SIZE = 40;

        static int seed = Environment.TickCount;
        static readonly ThreadLocal<Random> random =
        new ThreadLocal<Random>(() => new Random(Interlocked.Increment(ref seed)));

        private enum DIRECTION {
            FORWARD,
            BACKWARD,
            RIGHT,
            LEFT
        }

        public DroneSimulator(StatelessServiceContext context)
            : base(context)
        { }

        protected override IEnumerable<ServiceInstanceListener> CreateServiceInstanceListeners()
        {
            return new ServiceInstanceListener[0];
        }

        protected override async Task RunAsync(CancellationToken cancellationToken)
        {
            // Wait for all the services to startup
            await Task.Delay(TimeSpan.FromSeconds(15));

            // Generate initial simulation data
            var simulationRegistry = new List<string>();
            string[] ids = new string[SIMULATION_SIZE];
            for (int i = 0; i < SIMULATION_SIZE; i++) ids[i] = i.ToString();
            simulationRegistry.AddRange(ids);

            // Load initial simulation data
            var droneRegistry = DroneServiceFactory.CreateDroneRegistry();
            await droneRegistry.LoadExistingRegistry(simulationRegistry);

            ServiceEventSource.Current.Message("Simulation loaded");

            // Simulation loop
            while (!cancellationToken.IsCancellationRequested)
            {
                Parallel.ForEach<string>(ids, async id =>
                {
                    await UpdateDrone(id);
                });

                // Enforce variable simulation rate
                await Task.Delay(SIMULATION_RATE, cancellationToken);
            }
        }

        //private async Task UpdateDrone(string id)
        //{
        //    var payload = new
        //    {
        //        Id = id,
        //        State = new
        //        {
        //            Longitude = (Decimal)random.Value.NextDouble(),
        //            Latitude = (Decimal)random.Value.NextDouble(),
        //            Altitude = random.Value.Next(),
        //            Heading = random.Value.Next(),
        //            Speed = random.Value.Next()
        //        }
        //    };

        //    var jsonPayload = JsonConvert.SerializeObject(payload);
        //    var content = new StringContent(jsonPayload);
        //    content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
        //    content.Headers.ContentLength = jsonPayload.Length;

        //    using (var http = new HttpClient())
        //    {
        //        var response = await http.PutAsync(DRONE_ENDPOINT, content);

        //        if (response.IsSuccessStatusCode)
        //        {
        //            ServiceEventSource.Current.Message($"Successful update to drone {id}");
        //        }
        //        else
        //        {
        //            ServiceEventSource.Current.Message($"Failed to update to drone {id}");
        //        }
        //    }
        //}

        private async Task UpdateDrone(string id)
        {
            var directionToMove = random.Value.Next(0, 3);
            var drone = DroneServiceFactory.CreateDrone(id);

            switch ((DIRECTION)directionToMove)
            {
                case DIRECTION.FORWARD:
                    await drone.MoveForward();
                    break;
                case DIRECTION.BACKWARD:
                    await drone.MoveBackwards();
                    break;
                case DIRECTION.LEFT:
                    await drone.MoveLeft();
                    break;
                case DIRECTION.RIGHT:
                    await drone.MoveRight();
                    break;
                default:
                    await drone.MoveForward();
                    break;
            }

        }
    }
}

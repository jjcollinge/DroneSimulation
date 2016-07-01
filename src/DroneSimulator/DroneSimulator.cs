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
        private readonly int SIMULATION_SIZE = 20;

        private readonly TimeSpan SIMULATION_RATE = TimeSpan.FromSeconds(1);
        private bool THROTTLE_FLAG = false;
        private bool INITIALISED_FLAG = false;

        private string[] droneIds;

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
            // No communications required
            return new ServiceInstanceListener[0];
        }

        protected override async Task RunAsync(CancellationToken cancellationToken)
        {
            // Initialise simulation
            if (!INITIALISED_FLAG)
            {
                // Wait for all the services to startup
                await Task.Delay(TimeSpan.FromSeconds(15));

                // Generate initial simulation data
                var simulationRegistry = new List<string>();
                droneIds = new string[SIMULATION_SIZE];
                for (int i = 0; i < SIMULATION_SIZE; i++) droneIds[i] = i.ToString();
                simulationRegistry.AddRange(droneIds);

                // Load initial simulation data
                var droneRegistry = DroneServiceFactory.CreateDroneRegistry();
                await droneRegistry.LoadExistingRegistry(simulationRegistry);

                INITIALISED_FLAG = true;
                ServiceEventSource.Current.Message("Simulation loaded");
            }

            // Simulation loop
            while (!cancellationToken.IsCancellationRequested)
            {
                Parallel.ForEach<string>(droneIds, async id =>
                {
                    await UpdateDrone(id);
                });

                // Enforce variable simulation rate
                if(THROTTLE_FLAG)
                    await Task.Delay(SIMULATION_RATE, cancellationToken);
            }
            ServiceEventSource.Current.Message("Simulation loop exited");
        }

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

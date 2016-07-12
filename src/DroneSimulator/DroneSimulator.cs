using System;
using System.Collections.Generic;
using System.Fabric;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using Drones.Shared;
using Drones.Shared.Events;
using Microsoft.ServiceFabric.Actors.Client;
using Microsoft.ServiceFabric.Actors;
using Drones.Shared.Actors;
using Drones.Shared.Model;

namespace DroneSimulator
{
    internal sealed class DroneSimulator : StatelessService, ISwarmEvents
    {
        enum STATUS
        {
            UNINITIALISED,
            INITIALISED
        }

        private STATUS _status;
        private List<String> _cachedDroneIds;
        private ISwarmActor _swarm;

        static int seed = Environment.TickCount;
        static readonly ThreadLocal<Random> random =
        new ThreadLocal<Random>(() => new Random(Interlocked.Increment(ref seed)));

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
            if(_status == STATUS.UNINITIALISED)
            {
                Thread.Sleep(TimeSpan.FromSeconds(30));

                // Create a new swarm & subscribe to events
                var swarmId = ActorId.CreateRandom();
                _swarm = ActorProxy.Create<ISwarmActor>(swarmId);
                await _swarm.SubscribeAsync<ISwarmEvents>(this);

                // Create event store and register swarm with it
                var eventStore = DroneServiceFactory.CreateDroneEventStoreProxy();
                await eventStore.RegisterSwarmAsync(swarmId);

                for (int i = 0; i < 100; i++)
                {
                    // Add 'n' random guids to swarm
                    var droneId = ActorId.CreateRandom().ToString();
                    await _swarm.AddDroneAsync(droneId);
                }

                _status = STATUS.INITIALISED;
            }

            long iteration = 0;

            // Simulation loop
            while (!cancellationToken.IsCancellationRequested)
            {
                Parallel.ForEach<String>(_cachedDroneIds, id =>
                {
                    var drone = ActorProxy.Create<IDroneActor>(new ActorId(id));
                    var force = random.Value.Next(-10, 10);
                    var yaw = random.Value.NextDouble() * (((Math.PI / 180) * 359) - 0) + 0;
                    var pitch = random.Value.NextDouble() * (((Math.PI / 180) * 359) - 0) + 0;
                    var roll = random.Value.NextDouble() * (((Math.PI / 180) * 359) - 0) + 0;

                    drone.MoveAsync(new DroneTransform
                    {
                        Force = force,
                        Orientation = new Orientation
                        {
                            Yaw = yaw,
                            Pitch = pitch,
                            Roll = roll
                        },
                    });
                });

                ServiceEventSource.Current.ServiceMessage(this, $"Simulation loop iteration: {iteration++}");

                // Delay simulation loop
                await Task.Delay(TimeSpan.FromSeconds(1));
            }
            ServiceEventSource.Current.ServiceMessage(this, $"Simulation loop ended at: {iteration}");
        }

        public void DroneAdded(String swarmId, String droneId)
        {
            _cachedDroneIds.Add(droneId);
        }

        public void DroneRemoved(String swarmId, String droneId)
        {
            _cachedDroneIds.Remove(droneId);
        }
    }
}

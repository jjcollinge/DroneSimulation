using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using SwarmActor.Interfaces;
using DroneActor.Interfaces;
using Microsoft.ServiceFabric.Actors.Client;
using Microsoft.ServiceFabric.Actors;
using Common.Model;
using Common.Events;
using Microsoft.ServiceFabric.Services.Remoting.Client;
using Common.Services;
using Microsoft.ServiceFabric.Services.Client;

namespace DroneSimulation
{
    internal sealed class DroneSimulation : StatelessService, ISwarmActorEvents
    {
        private long _swarmId;
        private ISwarmActor _swarm;
        private List<IDroneActor> _drones;

        private readonly static Double MinimumTransform = -10;
        private readonly static Double MaximumTransform = 10;
        private readonly static int NumDronesInSimulation = 20;

        static int seed = Environment.TickCount;
        static readonly ThreadLocal<Random> random =
        new ThreadLocal<Random>(() => new Random(Interlocked.Increment(ref seed)));

        public DroneSimulation(StatelessServiceContext context)
            : base(context)
        {}

        protected override IEnumerable<ServiceInstanceListener> CreateServiceInstanceListeners()
        {
            return new ServiceInstanceListener[0];
        }

        protected override async Task RunAsync(CancellationToken cancellationToken)
        {
            await Task.Delay(TimeSpan.FromSeconds(45), cancellationToken);

            if (_drones == null)
                _drones = new List<IDroneActor>();

            await SetupSimulationAsync(cancellationToken);

            long iterations = 0;

            ServiceEventSource.Current.ServiceMessage(this, "Starting Simulation");

            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();

                await RunSimulationAsync(cancellationToken);

                ServiceEventSource.Current.ServiceMessage(this, "Iteration loop: {0}", ++iterations);

                await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
            }   
        }

        private async Task SetupSimulationAsync(CancellationToken cancellationToken)
        {
            await SetupSwarmAsync(cancellationToken);
            await SetupDronesAsync(cancellationToken);
        }

        private async Task SetupDronesAsync(CancellationToken cancellationToken)
        {
            for (int i = 0; i < NumDronesInSimulation; i++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                // Create each drone and add it to the swarm
                var drone = ActorProxy.Create<IDroneActor>(ActorId.CreateRandom());
                await _swarm.AddDroneAsync(drone.GetActorId().GetLongId());
                _drones.Add(drone);
            }

            // Create query engine and pass it the Swarm Id to subscribe to
            var queryEngine = ServiceProxy.Create<IDroneQueryEngine>(new Uri("fabric:/DroneSim/DroneQueryEngine"), new ServicePartitionKey(1));
            await queryEngine.SubscribeSwarmAsync(_swarmId);

            ServiceEventSource.Current.ServiceMessage(this, "Drones Setup Complete");
        }

        private async Task SetupSwarmAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var swarmId = ActorId.CreateRandom();
            _swarmId = swarmId.GetLongId();

            // Create and subscribe to new Swarm
            _swarm = ActorProxy.Create<ISwarmActor>(swarmId);
            await _swarm.SubscribeAsync<ISwarmActorEvents>(this);

            ServiceEventSource.Current.ServiceMessage(this, "Swarm Setup Complete");
        }

        private async Task RunSimulationAsync(CancellationToken cancellationToken)
        {
            
            foreach (var drone in _drones)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var x = random.Value.NextDouble() * (MaximumTransform - MinimumTransform) + MinimumTransform;
                var y = random.Value.NextDouble() * (MaximumTransform - MinimumTransform) + MinimumTransform;
                var z = random.Value.NextDouble() * (MaximumTransform - MinimumTransform) + MinimumTransform;
                var transform = new Vector3(x, y, z);
                await drone.MoveAsync(transform);
            }

        }

        public void DroneAdded(long droneId)
        {
            var drone = ActorProxy.Create<IDroneActor>(new ActorId(droneId));
            _drones.Add(drone);
        }

        public void DroneRemoved(long droneId)
        {
            var drone = ActorProxy.Create<IDroneActor>(new ActorId(droneId));
            _drones.Remove(drone);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Runtime;
using Microsoft.ServiceFabric.Actors.Client;
using Drones.Shared;

namespace DroneActor
{
    [StatePersistence(StatePersistence.Persisted)]
    internal sealed class DroneActor : Actor, IDroneActor
    {
        #region constants
        private const string STATE_IDENTIFIER = "DRONESTATE";
        private const int MAX_HEADING = 359;
        private const int MIN_HEADING = 0;
        #endregion

        #region private data members
        private delegate DroneModel Update(DroneModel state);
        #endregion

        #region methods
        public Task<string> GetIdAsync()
        {
            return Task.FromResult(this.GetActorId().ToString());
        }
        
        public async Task SetState(DroneModel model)
        {
            await this.StateManager.SetStateAsync(STATE_IDENTIFIER, model);
        }

        public async Task SetAltitude(int alt)
        {
            await UpdateState(state => 
            {
                state.Altitude = alt;
                return state;
            });
        }

        public async Task SetCoordinates(double lon, double lat)
        {
            await UpdateState(state =>
            {
                state.Longitude = lon;
                state.Latitude = lat;
                return state;
            });
        }

        public async Task SetHeading(int heading)
        {
            var clampedHeading = DroneCalculations.Clamp(heading, MIN_HEADING, MAX_HEADING);
            await UpdateState(state =>
            {
                state.Heading = clampedHeading;
                return state;
            });
        }

        public async Task SetSpeed(int speed)
        {
            await UpdateState(state =>
            {
                state.Speed = speed;
                return state;
            });
        }

        public async Task<Tuple<double, double>> GetCoordinates()
        {
            var state = await GetState();
            return new Tuple<double, double>(state.Longitude, state.Latitude);
        }

        public async Task<int> GetAltitude()
        {
            var state = await GetState();
            return state.Altitude;
        }

        public async Task<int> GetHeading()
        {
            var state = await GetState();
            return state.Heading;
        }

        public async Task<int> GetSpeed()
        {
            var state = await GetState();
            return state.Speed;
        }

        public async Task<DroneModel> GetState()
        {
            return await this.StateManager.GetStateAsync<DroneModel>(STATE_IDENTIFIER);
        }
        #endregion

        #region activation methods
        protected async override Task OnActivateAsync()
        {
            if (!(await this.StateManager.ContainsStateAsync(STATE_IDENTIFIER)))
            {
                // Initialise state
                var state = new DroneModel();
                await this.StateManager.TryAddStateAsync<DroneModel>(STATE_IDENTIFIER, state);
                ActorEventSource.Current.ActorMessage(this, "new actor state initialised.");
            }

            ActorEventSource.Current.ActorMessage(this, "Actor activated.");

            // The StateManager is this actor's private state store.
            // Data stored in the StateManager will be replicated for high-availability for actors that use volatile or persisted state storage.
            // Any serializable object can be saved in the StateManager.
            // For more information, see http://aka.ms/servicefabricactorsstateserialization

            return;
        }
        #endregion

        #region private methods
        private async Task UpdateState(Update update)
        {
            DroneModel state;
            var condition = await this.StateManager.TryGetStateAsync<DroneModel>(STATE_IDENTIFIER);
            if (condition.HasValue)
            {
                state = condition.Value;
                state = update(state);
                await this.StateManager.SetStateAsync<DroneModel>(STATE_IDENTIFIER, state);
            }
        }
        #endregion

    }
}
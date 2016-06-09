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
        private delegate DroneState Update(DroneState state);
        #endregion

        #region control methods
        public async Task MoveUp()
        {
            await UpdateState(state =>
            {
                state.Altitude = DroneCalculations.Clamp(state.Altitude + state.Speed,
                                                         state.Altitude + DroneCalculations.MIN_VERTICAL_SPEED,
                                                         state.Altitude + DroneCalculations.MAX_VERTICAL_SPEED);
                return state;
            });
        }

        public async Task MoveDown()
        {
            //TODO: Keep above 0
            await UpdateState(state =>
            {
                state.Altitude = DroneCalculations.Clamp(state.Altitude - state.Speed,
                                                         state.Altitude - DroneCalculations.MIN_VERTICAL_SPEED,
                                                         state.Altitude - DroneCalculations.MAX_VERTICAL_SPEED);
                return state;
            });
        }

        public async Task MoveRight()
        {
            await UpdateState(state =>
            {
                state.Longitude = DroneCalculations.Clamp(state.Longitude + state.Speed,
                                                          state.Longitude + DroneCalculations.MIN_HORIZONTAL_SPEED,
                                                          state.Longitude + DroneCalculations.MAX_HORIZONTAL_SPEED);
                return state;
            });
        }

        public async Task MoveLeft()
        {
            await UpdateState(state =>
            {
                state.Longitude = DroneCalculations.Clamp(state.Longitude - state.Speed,
                                                          state.Longitude - DroneCalculations.MIN_HORIZONTAL_SPEED,
                                                          state.Longitude - DroneCalculations.MAX_HORIZONTAL_SPEED);
                return state;
            });
        }

        public async Task MoveForward()
        {
            await UpdateState(state =>
            {
                state.Latitude = DroneCalculations.Clamp(state.Latitude + state.Speed,
                                                         state.Latitude + DroneCalculations.MIN_HORIZONTAL_SPEED,
                                                         state.Latitude + DroneCalculations.MAX_HORIZONTAL_SPEED);
                return state;
            });
        }

        public async Task MoveBackwards()
        {
            await UpdateState(state =>
            {
                state.Latitude = DroneCalculations.Clamp(state.Latitude - state.Speed,
                                                         state.Latitude - DroneCalculations.MIN_HORIZONTAL_SPEED,
                                                         state.Latitude - DroneCalculations.MAX_HORIZONTAL_SPEED);
                return state;
            });
        }

        public async Task RotateClockwise(int degrees)
        {
            //TODO: Keep within 360*
            await UpdateState(state =>
            {
                state.Heading = DroneCalculations.Clamp(state.Heading + state.Speed,
                                                        state.Heading + DroneCalculations.MAX_ROTATION_SPEED,
                                                        state.Heading + DroneCalculations.MIN_ROTATION_SPEED);
                return state;
            });
        }

        public async Task RotateAntiClockwise(int degrees)
        {
            //TODO: Keep within 360*
            await UpdateState(state =>
            {
                state.Heading = DroneCalculations.Clamp(state.Heading - state.Speed,
                                                        state.Heading - DroneCalculations.MAX_ROTATION_SPEED,
                                                        state.Heading - DroneCalculations.MIN_ROTATION_SPEED);
                return state;
            });
        }
        #endregion

        #region getters and setters
        public Task<string> GetIdAsync()
        {
            return Task.FromResult(this.GetActorId().ToString());
        }

        public async Task SetState(DroneState model)
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

        public async Task<DroneState> GetState()
        {
            return await this.StateManager.GetStateAsync<DroneState>(STATE_IDENTIFIER);
        }
        #endregion

        #region activation methods
        protected async override Task OnActivateAsync()
        {
            if (!(await this.StateManager.ContainsStateAsync(STATE_IDENTIFIER)))
            {
                // Initialise state
                var state = new DroneState();
                await this.StateManager.TryAddStateAsync<DroneState>(STATE_IDENTIFIER, state);
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
            DroneState state;
            var condition = await this.StateManager.TryGetStateAsync<DroneState>(STATE_IDENTIFIER);
            if (condition.HasValue)
            {
                state = condition.Value;
                state = update(state);
                await this.StateManager.SetStateAsync<DroneState>(STATE_IDENTIFIER, state);
            }
        }
        #endregion

    }
}
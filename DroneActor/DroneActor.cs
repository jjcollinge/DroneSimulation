﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Runtime;
using Microsoft.ServiceFabric.Actors.Client;
using DroneActor.Interfaces;
using DroneQueryService.Interfaces;

namespace DroneActor
{
    [StatePersistence(StatePersistence.Persisted)]
    internal class DroneActor : Actor, IDroneActor
    {
        #region constants
        private const string STATE_IDENTIFIER = "DRONESTATE";
        #endregion

        private IDroneQueryService _query;
        private delegate DroneActorState Update(DroneActorState state);

        public DroneActor(IDroneQueryService query)
        {
            _query = query;
        }

        #region methods
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

        public Task SetHeading(int heading)
        {
            await UpdateState(state =>
            {
                state.Altitude = alt;
                return state;
            });
        }
        public Task<Tuple<double, double>> GetCoordinates()
        {
            throw new NotImplementedException();
        }

        public Task<int> GetAltitude()
        {
            throw new NotImplementedException();
        }

        public Task<int> GetHeading()
        {
            throw new NotImplementedException();
        }

        public Task<DroneActorState> GetState()
        {
            throw new NotImplementedException();
        }
        #endregion

        #region activation methods
        protected async override Task OnActivateAsync()
        {
            if (!(await this.StateManager.ContainsStateAsync(STATE_IDENTIFIER)))
            {
                // Initialise state
                var state = new DroneActorState();
                await this.StateManager.TryAddStateAsync<DroneActorState>(STATE_IDENTIFIER, state);
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

        #region public methods
        #endregion

        #region private methods
        private async Task UpdateState(Update update)
        {
            DroneActorState state;
            var condition = await this.StateManager.TryGetStateAsync<DroneActorState>(STATE_IDENTIFIER);
            if (condition.HasValue)
            {
                state = condition.Value;
                state = update(state);
                await this.StateManager.SetStateAsync<DroneActorState>(STATE_IDENTIFIER, state);
            }
        }
        #endregion

    }
}
using Drones.Shared;
using Microsoft.ServiceFabric.Services.Remoting.Client;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Http;
using System;
using System.Threading;
using Drones.Shared.Events;
using Drones.Shared.Model;
using Microsoft.ServiceFabric.Actors;

namespace DroneWebApi.Controllers
{
    public class DroneEventsController : ApiController
    {
        public DroneEventsController()
        {}

        // GET api/drones
        public async Task<List<DroneTransformedEvent>> GetAsync()
        {
            var eventStore = DroneServiceFactory.CreateDroneEventStoreProxy();
            var events = await eventStore.GetEventsAsync();
            return events;
        }

        // GET api/drone/5 
        public async Task<List<DroneTransformedEvent>> GetAsync(string id)
        {
            var eventStore = DroneServiceFactory.CreateDroneEventStoreProxy();
            var events = await eventStore.GetEventsByDroneAsync(new ActorId(id));
            return events;
        } 
    }
}

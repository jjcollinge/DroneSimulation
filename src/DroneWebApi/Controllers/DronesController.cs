using Drones.Shared;
using Microsoft.ServiceFabric.Services.Remoting.Client;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Http;
using System;

namespace DroneWebApi.Controllers
{
    public class DroneController : ApiController
    {
        private IDroneManager _droneManager;

        public DroneController()
        {
            _droneManager = DroneServiceFactory.CreateDroneManager();
        }

        // GET api/drone 
        public async Task<IEnumerable<DronePayload>> GetAsync()
        {
            var drones = await _droneManager.GetDronesAsync();
            return drones;
        }

        // GET api/drone/5 
        public async Task<DronePayload> GetAsync(string id)
        {
            var drone = await _droneManager.GetDroneAsync(id.ToString());
            return drone;
        }

        // POST api/drone 
        public async Task PostAsync([FromBody]DroneState droneState)
        {
            var newDroneId = await _droneManager.GenerateDroneIdAsync();
            await _droneManager.AddDroneAsync(newDroneId, droneState);
        }

        // PUT api/drone/5 
        public async Task Put(DronePayload updateDrone)
        {
            await _droneManager.UpdateDroneAsync(updateDrone.Id, updateDrone.State);
        }

        // DELETE api/drone/5 
        public async Task Delete(string id)
        {
            await _droneManager.RemoveDroneAsync(id.ToString());
        }
    }
}

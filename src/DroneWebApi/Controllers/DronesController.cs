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
        private readonly string SERVICE_PREFIX = "DroneController";
        private IDroneManager _droneManager;

        public DroneController()
        {
            _droneManager = DroneServiceFactory.CreateDroneManager();
        }

        // GET api/drone 
        public async Task<Dictionary<string, DroneState>> GetAsync()
        {
            ServiceEventSource.Current.Message($"{SERVICE_PREFIX}: GetAsync called with no parameters");
            var drones = await _droneManager.GetDroneMap();
            return drones;
        }

        // GET api/drone/5 
        public async Task<DronePayload> GetAsync(string id)
        {
            ServiceEventSource.Current.Message($"{SERVICE_PREFIX}: GetAsync called with parameter {id}");
            var drone = await _droneManager.GetDroneAsync(id.ToString());
            return drone;
        }

        // POST api/drone 
        public async Task PostAsync([FromBody]DroneState droneState)
        {
            ServiceEventSource.Current.Message($"{SERVICE_PREFIX}: PostAsync called with parameter {droneState}");
            var newDroneId = await _droneManager.GenerateDroneIdAsync();
            await _droneManager.AddDroneAsync(newDroneId, droneState);
        }

        // PUT api/drone/5 
        public async Task PutAsync(DronePayload updateDrone)
        {
            ServiceEventSource.Current.Message($"{SERVICE_PREFIX}: PutAsync called with parameter {updateDrone}");
            await _droneManager.UpdateDroneAsync(updateDrone.Id, updateDrone.State);
        }

        // DELETE api/drone/5 
        public async Task DeleteAsync(string id)
        {
            ServiceEventSource.Current.Message($"{SERVICE_PREFIX}: DeleteAsync called with parameter {id}");
            await _droneManager.RemoveDroneAsync(id.ToString());
        }
    }
}

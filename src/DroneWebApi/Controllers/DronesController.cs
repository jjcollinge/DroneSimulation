using Drones.Shared;
using Microsoft.ServiceFabric.Services.Remoting.Client;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Http;

namespace DroneWebApi.Controllers
{
    public class DroneController : ApiController
    {
        private IDroneManagementService _droneService;

        public DroneController()
        {
            _droneService = DroneServiceFactory.CreateDroneManagementService();
        }

        // GET api/drone 
        public async Task<IList<DroneModel>> GetAsync()
        {
            var drones = await _droneService.GetDronesAsync();

            ConcurrentBag<DroneModel> droneStates = new ConcurrentBag<DroneModel>();
            Parallel.For(0, drones.Count, async i =>
            {
                droneStates.Add(await drones[i].GetState());
            });

            var droneList = new List<DroneModel>(droneStates);
            return droneList;
        }

        // GET api/drone/5 
        public async Task<DroneModel> GetAsync(int id)
        {
            var drone = await _droneService.GetDroneAsync(id.ToString());
            return await drone.GetState();
        }

        // POST api/drone 
        public async Task PostAsync([FromBody]DroneModel model)
        {
            var newDroneId = await _droneService.GenerateDroneIdAsync();
            await _droneService.AddDroneAsync(newDroneId, model);
        }

        // PUT api/drone/5 
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/drone/5 
        public async Task Delete(int id)
        {
            await _droneService.RemoveDroneAsync(id.ToString());
        }
    }
}

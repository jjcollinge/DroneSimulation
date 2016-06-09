using DroneActor.Interfaces;
using DroneQueryService.Interfaces;
using Microsoft.ServiceFabric.Services.Remoting.Client;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Http;

namespace DroneWebApi.Controllers
{
    public class DroneController : ApiController
    {
        // GET api/drone 
        public async Task<IList<DroneActorState>> GetAsync()
        {
            var droneService = DroneQueryServiceFactory.CreateDroneQueryService();
            var drones = await droneService.GetDrones();

            ConcurrentBag<DroneActorState> droneStates = new ConcurrentBag<DroneActorState>();
            Parallel.For(0, drones.Count, async i =>
            {
                droneStates.Add(await drones[i].GetState());
            });

            var droneList = new List<DroneActorState>(droneStates);
            return droneList;
        }

        // GET api/drone/5 
        public async Task<DroneActorState> GetAsync(int id)
        {
            var droneService = DroneQueryServiceFactory.CreateDroneQueryService();
            var drone = await droneService.GetDrone(id.ToString());
            return await drone.GetState();
        }

        // POST api/values 
        public void Post([FromBody]string value)
        {
        }

        // PUT api/values/5 
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/values/5 
        public void Delete(int id)
        {
        }
    }
}

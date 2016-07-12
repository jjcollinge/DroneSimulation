using Common.Model;
using Common.Services;
using Microsoft.ServiceFabric.Services.Remoting.Client;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Http;

namespace WebApi.Controllers
{
    public class DronesController : ApiController
    {
        // GET api/drones 
        public async Task<IEnumerable<DroneStateSnapshot>> GetAsync()
        {
            var droneQueryEngine = ServiceProxy.Create<IDroneQueryEngine>(new Uri("fabric:/DroneSim/DroneQueryEngine"), new Microsoft.ServiceFabric.Services.Client.ServicePartitionKey(1));
            var drones = await droneQueryEngine.GetDronesAsync();
            return drones;
        }

        // GET api/drones/5 
        public string Get(int id)
        {
            return "value";
        }

        // POST api/drones 
        public void Post([FromBody]string value)
        {
        }

        // PUT api/drones/5 
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/drones/5 
        public void Delete(int id)
        {
        }
    }
}

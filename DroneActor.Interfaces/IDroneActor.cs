using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Actors;

namespace DroneActor.Interfaces
{
    public interface IDroneActor : IActor
    {
        #region setters
        Task SetCoordinates(double lon, double lat);
        Task SetAltitude(int alt);
        Task SetHeading(int heading);
        #endregion

        #region getters
        Task<Tuple<double, double>> GetCoordinates();
        Task<int> GetAltitude();
        Task<int> GetHeading();
        Task<DroneActorState> GetState();
        #endregion
    }
}

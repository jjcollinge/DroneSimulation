using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Actors;

namespace Drones.Shared
{
    public interface IDroneActor : IActor
    {
        #region setters
        Task SetState(DroneModel model);
        Task SetCoordinates(double lon, double lat);
        Task SetAltitude(int alt);
        Task SetHeading(int heading);
        Task SetSpeed(int speed);
        #endregion

        #region getters
        Task<string> GetIdAsync();
        Task<Tuple<double, double>> GetCoordinates();
        Task<int> GetAltitude();
        Task<int> GetHeading();
        Task<int> GetSpeed();
        Task<DroneModel> GetState();
        #endregion
    }
}

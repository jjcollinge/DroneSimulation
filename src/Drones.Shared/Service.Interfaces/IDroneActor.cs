using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Actors;
using System.Runtime.Serialization;

namespace Drones.Shared
{
    public interface IDroneActor : IActor
    {
        #region control methods
        Task MoveUp();
        Task MoveDown();
        Task MoveRight();
        Task MoveLeft();
        Task MoveForward();
        Task MoveBackwards();
        Task RotateClockwise(int degrees);
        Task RotateAntiClockwise(int degrees);
        #endregion

        #region setters
        Task SetState(DroneState model);
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
        Task<DroneState> GetState();
        #endregion
    }
}

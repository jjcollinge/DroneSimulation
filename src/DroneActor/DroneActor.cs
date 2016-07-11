using System;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Actors.Runtime;
using System.Runtime.Serialization;
using Drones.Shared.Model;
using Drones.Shared.Actors;

namespace DroneActor
{
    [StatePersistence(StatePersistence.Persisted), DataContract]
    internal sealed class DroneActor : Actor, IDroneActor
    {
        [DataMember]
        private Drone _state;

        public DroneActor()
        { }

        protected override Task OnActivateAsync()
        {
            if (_state == null)
            {
                _state = new Drone();
            }
            return base.OnActivateAsync();
        }

        public Task MoveAsync(DroneTransform transform)
        {
            UpdatePosition(transform.Force, transform.Orientation.Yaw, transform.Orientation.Pitch, transform.Orientation.Roll);
            UpdateOrientation(transform.Orientation.Yaw, transform.Orientation.Pitch, transform.Orientation.Roll);
            return Task.FromResult(true);
        }

        private void UpdateOrientation(double yaw, double pitch, double roll)
        {
            _state.Orientation = new Orientation
            {
                Yaw = yaw,
                Pitch = pitch,
                Roll = roll
            };
        }

        private void UpdatePosition(double force, double yaw, double pitch, double roll)
        {
            // Convert Yaw, Pitch and Roll into Quaternion angles.
            double c1 = Math.Cos(yaw / 2.0);
            double s1 = Math.Sin(yaw / 2.0);
            double c2 = Math.Cos(pitch / 2.0);
            double s2 = Math.Sin(pitch / 2.0);
            double c3 = Math.Cos(roll / 2.0);
            double s3 = Math.Sin(roll / 2.0);
            double c1c2 = c1 * c2;
            double s1s2 = s1 * s2;
            var w = c1c2 * c3 - s1s2 * s3;
            var x = c1c2 * s3 + s1s2 * c3;
            var y = (s1 * c2 * c3) + (c1 * s2 * s3);
            var z = (c1 * s2 * c3) - (s1 * c2 * s3);
            // Build a transformation vector (w, x, y, z)
            var v1 = (2 * x * z) - (2 * y * w);
            var v2 = (2 * y * z) + (2 * x * w);
            var v3 = (1 - 2 * (x * x)) - (2 * (y * y));
            // Apply the transformation vector to the existing state and save in new state
            _state.Position.X += force * v1;
            _state.Position.Y += force * v2;
            _state.Position.Z += force * v3;
        }
    }
}
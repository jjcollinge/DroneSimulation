using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Drones.Shared.Model
{
    [DataContract]
    public class DroneTransformedEvent
    {
        public DroneTransformedEvent()
        {
            Id = Guid.NewGuid();
        }
        [DataMember]
        public Guid Id { get; private set; }
        [DataMember]
        public DateTime Timestamp { get; set; }
        [DataMember]
        public Drone CurrentDroneState { get; set; }
        [DataMember]
        public DroneTransform Transformation { get; set; }
    }
}

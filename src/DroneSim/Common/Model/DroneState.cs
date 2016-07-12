using System;
using System.Runtime.Serialization;

namespace Common.Model
{
    [DataContract]
    public class DroneState
    {
        public DroneState()
        {
            Model = "Unknown";
            Manufacturer = "Unknown";
            Position = new Point();
        }
        [DataMember]
        public String Model { get; set; }
        [DataMember]
        public String Manufacturer { get; set; }
        [DataMember]
        public Point Position { get; set; }
    }
}

using System.Runtime.Serialization;

namespace Drones.Shared.Model
{
    [DataContract]
    public class Orientation
    {
        [DataMember]
        public double Yaw { get; set; }
        [DataMember]
        public double Pitch { get; set; }
        [DataMember]
        public double Roll { get; set; }
    }
}

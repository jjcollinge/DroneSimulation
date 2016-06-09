using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Drones.Shared
{
    [DataContract]
    public class DroneModel
    {
        [DataMember]
        public int Altitude { get; set; }
        [DataMember]
        public Double Longitude { get; set; }
        [DataMember]
        public Double Latitude { get; set; }
        [DataMember]
        public int Heading { get; set; }
        [DataMember]
        public int Speed { get; set; }
    }
}

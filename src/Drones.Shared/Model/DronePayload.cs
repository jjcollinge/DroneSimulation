using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Drones.Shared
{
    [DataContract]
    public class DronePayload
    {
        [DataMember]
        public string Id { get; set; }

        [DataMember]
        public DroneState State { get; set; }
    }
}

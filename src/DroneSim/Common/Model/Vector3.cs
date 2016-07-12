using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Common.Model
{
    [DataContract]
    public class Vector3
    {
        public Vector3(Double x, Double y, Double z)
        {
            X = x;
            Y = y;
            Z = z;
        }
        [DataMember]
        public Double X { get; private set; }
        [DataMember]
        public Double Y { get; private set; }
        [DataMember]
        public Double Z { get; private set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Common.Model
{
    [DataContract]
    public class Point
    {
        public Point(Double x = 0.0, Double y = 0.0, Double z = 0.0)
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

        public static Point operator +(Point position, Vector3 transform)
        {
            return new Point(position.X + transform.X,
                             position.Y + transform.Y,
                             position.Z + transform.Z);
        }

        public static Point operator -(Point position, Vector3 transform)
        {
            return new Point(position.X - transform.X,
                             position.Y - transform.Y,
                             position.Z - transform.Z);
        }

        public static Point operator *(Point position, Vector3 transform)
        {
            return new Point(position.X * transform.X,
                             position.Y * transform.Y,
                             position.Z * transform.Z);
        }

        public static Point operator /(Point position, Vector3 transform)
        {
            return new Point(position.X / transform.X,
                             position.Y / transform.Y,
                             position.Z / transform.Z);
        }
    }
}

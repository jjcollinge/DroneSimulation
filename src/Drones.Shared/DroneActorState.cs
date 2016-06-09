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
        #region constants
        private const int MAX_HEADING = 360;
        private const int MIN_HEADING = 0;
        #endregion

        #region properties
        [DataMember]
        public int Altitude { get; set; }
        [DataMember]
        public Double Longitude { get; set; }
        [DataMember]
        public Double Latitude { get; set; }
        [DataMember]
        public int Heading
        {
            get { return _heading; }
            set { _heading = DroneCalculations.Clamp(value, MIN_HEADING, MAX_HEADING); }
        }
        #endregion

        #region data members
        private int _heading;
        #endregion
    }
}

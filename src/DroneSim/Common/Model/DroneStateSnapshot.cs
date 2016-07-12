using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Model
{
    public class DroneStateSnapshot
    {
        public DateTime TimeStamp { get; set; }
        public DroneState State { get; set; }
        public long DroneId { get; set; }
    }
}

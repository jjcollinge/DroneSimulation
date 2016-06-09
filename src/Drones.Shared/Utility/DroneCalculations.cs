using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Drones.Shared
{
    public class DroneCalculations
    {
        public const int MAX_HORIZONTAL_SPEED = 10;
        public const int MIN_HORIZONTAL_SPEED = 10;
        public const int MAX_VERTICAL_SPEED = 10;
        public const int MIN_VERTICAL_SPEED = 10;
        public const int MAX_ROTATION_SPEED = 10;
        public const int MIN_ROTATION_SPEED = 10;

        public static int Clamp(int value, int min, int max)
        {
            return (value < min) ? min : (value > max) ? max : value;
        }

        public static double Clamp(double value, double min, double max)
        {
            return (value < min) ? min : (value > max) ? max : value;
        }
    }
}

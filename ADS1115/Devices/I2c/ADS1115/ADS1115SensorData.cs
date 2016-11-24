using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ADS1115.Devices.I2c.ADS1115
{
    public struct ADS1115SensorData
    {
        //field és láthatóság osztálybeli?
        public int DecimalValue { get; set; }
        public double VoltageValue { get; set; }
    }
}

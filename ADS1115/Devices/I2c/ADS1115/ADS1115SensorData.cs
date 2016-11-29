using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ADC.Devices.I2c.ADS1115
{
    /// <summary>
    /// 
    /// </summary>
    public struct ADS1115SensorData
    {
        //field és láthatóság osztálybeli?
        public int DecimalValue { get; set; }
        public double VoltageValue { get; set; }
    }
}

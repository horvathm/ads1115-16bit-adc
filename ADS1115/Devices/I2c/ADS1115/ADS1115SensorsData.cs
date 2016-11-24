using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ADS1115.Devices.I2c.ADS1115
{
    public class ADS1115SensorsData
    {
        public ADS1115SensorData A0 { get; set; }
        public ADS1115SensorData A1 { get; set; }
        public ADS1115SensorData A2 { get; set; }
        public ADS1115SensorData A3 { get; set; }
    }
}

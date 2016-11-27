using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ADS1115.Devices.I2c.ADS1115
{
    interface IAnalogDititalConverter
    {
        void writeConfig(byte[] config);
        Task<byte[]> readConfig();

        Task writeTreshold(Int16 loTreshold, Int16 highTreshold);

        Task readContinuousInit(ADS1115SensorSetting setting);
        int readContinuous();
        Task<ADS1115SensorData> readSingleShot(ADS1115SensorSetting setting);
    }
}

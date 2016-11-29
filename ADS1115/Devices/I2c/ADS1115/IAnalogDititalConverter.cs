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
    interface IAnalogDititalConverter
    {
        void writeConfig(byte[] config);
        Task<byte[]> readConfig();

        void TurnAlertIntoConversionReady();
        Task writeTreshold(UInt16 loTreshold, UInt16 highTreshold);

        Task readContinuousInit(ADS1115SensorSetting setting);
        int readContinuous();
        Task<ADS1115SensorData> readSingleShot(ADS1115SensorSetting setting);
    }
}

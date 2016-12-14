using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ADC.Devices.I2c.ADS1115
{
    /// <summary>
    /// Interface of the ADC.
    /// </summary>
    interface IAnalogDititalConverter
    {
        // you can read and write configuration register, because it's enabled so why not. After writeConfig you have to init. readContinuous.
        void writeConfig(byte[] config);
        Task<byte[]> readConfig();

        // methods with the treshold registers
        void TurnAlertIntoConversionReady();
        Task writeTreshold(UInt16 loTreshold, UInt16 highTreshold);

        // methods with the conversion registers. After readSingleShot or before using first time read continuous you have to init. readContinuous.
        Task readContinuousInit(ADS1115SensorSetting setting);
        int readContinuous();
        Task<ADS1115SensorData> readSingleShot(ADS1115SensorSetting setting);
    }
}

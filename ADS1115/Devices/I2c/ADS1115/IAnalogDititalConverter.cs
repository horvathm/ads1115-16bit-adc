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
        byte[] readConfig();
        void writeLowTreshold(Int16 treshold);
        void writeHighTreshold(Int16 treshold);
        //void initializeContinuousConversionMode(ADS1115SensorSetting setting); 
        //Task<int> readContinuous(); //ha jó módban vagyunk akkor nem kell beírni csak olvasgatni else hiba
        Task<ADS1115SensorData> readSingleShot(ADS1115SensorSetting setting);
        Task<ADS1115SensorsData> readTwoDifferentialInSingleShot(ADS1115SensorSetting setting);
        Task<ADS1115SensorsData> readFourSingleEndedInSingleShot(ADS1115SensorSetting setting);
    }
}

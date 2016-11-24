using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ADS1115.Devices.I2c.ADS1115
{
    interface IAnalogDititalConverter
    {
        AdcMode ConverterMode { get; set; }

        void writeConfig(byte[] config); //beírunk a configba néha kellhet átállítani a ConverterMode
        byte[] readConfig();            //kiolvassuk a configot, talán szükség lehet mondjuk megnézni, hogy tart-e a konverzió
        void writeLowTreshold(byte[] treshold); //írjuk a tresholdot
        void writeHighTreshold(byte[] treshold);//írjuk a tresholdot
        void initializeContinuousConversionMode(ADS1115SensorSetting setting); // ha nem writeConfigból szeretnénk állítani hogy lehessen readConti, akkor először ezt állít
        Task<ADS1115SensorData> readContinuous(); //ha jó módban vagyunk akkor nem kell beírni csak olvasgatni else hiba
        Task<ADS1115SensorData> readSingleShot(ADS1115SensorSetting setting); //reg írás és utána egy olvasás is lesz ConverterMode-t állítgat
        Task<ADS1115SensorsData> readTwoDifferentialInSingleShot(ADS1115SensorSetting setting); //1 = 2 // 3 = 4
        Task<ADS1115SensorsData> readFourSingleEndedInSingleShot(ADS1115SensorSetting setting); //1, 2, 3, 4 külön érték
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.I2c;

namespace ADS1115.Devices.I2c.ADS1115
{
    //láthatóság amit itt public az mind publicnak kell lenni máshol :( paraméterben


    public class ADS1115 : IDisposable, IAnalogDititalConverter
    {
        public AdcMode ConverterMode { get; set; }
        public bool IsInitialized { get; private set; }

        private readonly byte ADC_I2C_ADDR;                     //address of the ads1115
        private const byte ADC_REG_POINTER_CONVERSION = 0x00;   //pointer register values
        private const byte ADC_REG_POINTER_CONFIG = 0x01;
        private const byte ADC_REG_POINTER_LOTRESHOLD = 0x02;
        private const byte ADC_REG_POINTER_HITRESHOLD = 0x03;
        public const int ADC_RES = 65536;                       //resolutions in different conversion modes
        public const int ADC_HALF_RES = 32768;
        private I2cDevice adc;                                  //the device

        public ADS1115(AdcAddress ads1115Addresses)
        {
            ADC_I2C_ADDR = (byte)ads1115Addresses;
        }

        public void Dispose()
        {
            adc.Dispose();
            adc = null;
        }

        public async Task InitializeAsync()
        {
            if (IsInitialized)
            {
                throw new InvalidOperationException("The I2C ads1115 sensor is already initialized.");
            }

            // gets the default controller for the system, can be the lightning or any provider
            I2cController controller = await I2cController.GetDefaultAsync();
            // gets the I2CDevice from the controller using the connection setting
            adc = controller.GetDevice(new I2cConnectionSettings(ADC_I2C_ADDR));

            if (adc == null)
                throw new Exception("I2C controller not available on the system");

            IsInitialized = true;
        }

        public void initializeContinuousConversionMode(ADS1115SensorSetting setting)
        {
            throw new NotImplementedException();
        }

        public byte[] readConfig()
        {
            /*
                beállít config reg és read 16 bit
            */
            throw new NotImplementedException();
        }

        public Task<ADS1115SensorData> readContinuous()
        {
            /*
                valahogy megoldani, hogy egyből tudjon olvasni ne kelljen mindig állítgatni
             */
            throw new NotImplementedException();
        }

        public async Task<ADS1115SensorData> readSingleShot(ADS1115SensorSetting setting)
        {
            var sensorData = new ADS1115SensorData();
            int temp = await ReadSensorAsync(configA(setting), configB(setting));   //read sensor with the generated configuration bytes
            sensorData.DecimalValue = temp;

            //calculate the voltage with different resolutions in single ended and in differential mode
            if ((byte)setting.Input <= 0x03)
                sensorData.VoltageValue = DecimalToVoltage(setting.Pga, temp, ADC_RES);
            else
                sensorData.VoltageValue = DecimalToVoltage(setting.Pga, temp, ADC_HALF_RES);

            return sensorData;
        }

        public void writeConfig(byte[] config)
        {
            /*
                beállít config reg és write 16 bit
             */
            throw new NotImplementedException();
        }

        public void writeHighTreshold(byte[] treshold)
        {
            /*
                beállít pointer reg a tresholdra és write 16 bit mint a config írásánál
            */
            throw new NotImplementedException();
        }

        public void writeLowTreshold(byte[] treshold)
        {
            /*
             beállít pointer reg a tresholdra és write 16 bit mint a config írásánál
             */
            throw new NotImplementedException();
        }

        public async Task<ADS1115SensorsData> readTwoDifferentialInSingleShot(ADS1115SensorSetting setting)
        {
            // in differential mode it's harder to define the other input so it works only in single shoot mode
            if ((byte)setting.Input > 0x03)
                throw new InvalidOperationException("It's not allowed to run with differential input");

            var sensorData = new ADS1115SensorData();
            var sensorsData = new ADS1115SensorsData();
            int temp;

            setting.Input = AdcInput.A01_DIFF;
            temp = await ReadSensorAsync(configA(setting), configB(setting));
            sensorData.DecimalValue = temp;
            sensorData.VoltageValue = DecimalToVoltage(setting.Pga, temp, ADC_HALF_RES);
            sensorsData.A0 = sensorData;
            sensorsData.A1 = sensorData;

            setting.Input = AdcInput.A23_DIFF;
            temp = await ReadSensorAsync(configA(setting), configB(setting));
            sensorData.DecimalValue = temp;
            sensorData.VoltageValue = DecimalToVoltage(setting.Pga, temp, ADC_HALF_RES);
            sensorsData.A2 = sensorData;
            sensorsData.A3 = sensorData;

            return sensorsData;
        }

        public async Task<ADS1115SensorsData> readFourSingleEndedInSingleShot(ADS1115SensorSetting setting)
        {
            // in differential mode it's harder to define the other input so it works only in single shoot mode
            if ((byte)setting.Input <= 0x03)
                throw new InvalidOperationException("It's not allowed to run with differential input");

            var sensorData = new ADS1115SensorData();
            var sensorsData = new ADS1115SensorsData();
            int temp;

            setting.Input = AdcInput.A0_SE;
            temp = await ReadSensorAsync(configA(setting), configB(setting));
            sensorData.DecimalValue = temp;
            sensorData.VoltageValue = DecimalToVoltage(setting.Pga, temp, ADC_HALF_RES);
            sensorsData.A0 = sensorData;

            setting.Input = AdcInput.A1_SE;
            temp = await ReadSensorAsync(configA(setting), configB(setting));
            sensorData.DecimalValue = temp;
            sensorData.VoltageValue = DecimalToVoltage(setting.Pga, temp, ADC_HALF_RES);
            sensorsData.A1 = sensorData;

            setting.Input = AdcInput.A2_SE;
            temp = await ReadSensorAsync(configA(setting), configB(setting));
            sensorData.DecimalValue = temp;
            sensorData.VoltageValue = DecimalToVoltage(setting.Pga, temp, ADC_HALF_RES);
            sensorsData.A2 = sensorData;

            setting.Input = AdcInput.A3_SE;
            temp = await ReadSensorAsync(configA(setting), configB(setting));
            sensorData.DecimalValue = temp;
            sensorData.VoltageValue = DecimalToVoltage(setting.Pga, temp, ADC_HALF_RES);
            sensorsData.A3 = sensorData;

            return sensorsData;
        }

        private async Task<int> ReadSensorAsync(byte configA, byte configB)
        {
            var command = new byte[] { ADC_REG_POINTER_CONFIG, configA, configB };
            var readBuffer = new byte[2];
            var writeBuffer = new byte[] { ADC_REG_POINTER_CONVERSION };
            adc.Write(command);
            await Task.Delay(10);       // havent found the proper value
            adc.WriteRead(writeBuffer, readBuffer);

            if ((byte)(readBuffer[0] & 0x80) != 0x00)
            {
                // two's complement conversion
                Array.Reverse(readBuffer);
                var Result = 255 & ~(BitConverter.ToUInt16(readBuffer, 0) - 1);
                return -1 * Result;
            }
            else
            {
                Array.Reverse(readBuffer);
                return BitConverter.ToUInt16(readBuffer, 0);
            }
        }

        // generate the first part of the config register
        private byte configA(ADS1115SensorSetting setting)
        {
            byte configA = 0;
            return configA = (byte)((byte)setting.Mode << 7 | (byte)setting.Input << 4 | (byte)setting.Pga << 1 | (byte)setting.Mode);
        }

        // generate the second part of the config register
        private byte configB(ADS1115SensorSetting setting)
        {
            byte configB;
            return configB = (byte)((byte)setting.DataRate << 5 | (byte)setting.ComMode << 4 | (byte)setting.ComPolarity << 3 | (byte)setting.ComLatching << 2 | (byte)setting.ComQueue);
        }

        // function that create voltage from a AdcPga enumeration in order to determine the voltage on the pin.
        // i assume it works well but tested too few inputs 
        private double DecimalToVoltage(AdcPga pga, int temp, int resolution)
        {
            double voltage;

            switch (pga)
            {
                case AdcPga.G2P3:
                    voltage = 6.144;
                    break;
                case AdcPga.G1:
                    voltage = 4.096;
                    break;
                case AdcPga.G2:
                    voltage = 2.048;
                    break;
                case AdcPga.G4:
                    voltage = 1.024;
                    break;
                case AdcPga.G8:
                    voltage = 0.512;
                    break;
                case AdcPga.G16:
                default:
                    voltage = 0.256;
                    break;
            }
            return voltage / (resolution / temp);
        }
    }

}

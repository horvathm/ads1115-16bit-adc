using System;
using System.Threading.Tasks;
using Windows.Devices.I2c;

namespace ADS1115.Devices.I2c_devices
{
    #region Enumerations
    public enum AdcAddress : byte { GND = 0x48, VCC = 0x49, SDA = 0x4A, SCL = 0x4B }       // Possible ads1115 addresses:  0x48: ADR -> GND  0x49: ADR -> VCC  0x4A: ADR -> SDA  0x4B: ADR -> SCL
    public enum AdcInput : byte { A0_SE = 0x04, A1_SE = 0x05, A2_SE = 0x06, A3_SE = 0x07, A01_DIFF = 0x00, A03_DIFF = 0x01, A13_DIFF = 0x02, A23_DIFF = 0x03 }
    public enum AdcPga : byte { G2P3 = 0x00, G1 = 0x01, G2 = 0x02, G4 = 0x03, G8 = 0x04, G16 = 0x05 }
    public enum AdcMode : byte { CONTINOUS_CONVERSION = 0x00, SINGLESHOOT_CONVERSION = 0x01 }
    public enum AdcDataRate : byte { SPS8 = 0X00, SPS16 = 0X01, SPS32 = 0X02, SPS64 = 0X03, SPS128 = 0X04, SPS250 = 0X05, SPS475 = 0X06, SPS860 = 0X07 }
    public enum AdcComparatorMode : byte { TRADITIONAL = 0x00, WINDOW = 0x01 }
    public enum AdcComparatorPolarity : byte { ACTIVE_LOW = 0x00, ACTIVE_HIGH = 0x01 }
    public enum AdcComparatorLatching : byte { LATCHING = 0x00, NONLATCHING = 0x01 }
    public enum AdcComparatorQueue : byte { ASSERT_AFTER_ONE = 0x01, ASSERT_AFTER_TWO = 0x02, ASSERT_AFTER_FOUR = 0x04, DISABLE_COMPARATOR = 0x03 }
    #endregion

    /*
     * Class to handle the ADS1115 16 bit ADC
     * It's almost fully functional with extra features. 
     * Setting the treshold registers are missing for example and havent tested properly yet.
     */
    class SensorADS1115 : IDisposable
    {
        #region Register Addresses       

        //address of the ads1115
        private readonly byte ADC_I2C_ADDR;

        //pointer register values
        private const byte ADC_REG_POINTER_CONVERSION = 0x00;
        private const byte ADC_REG_POINTER_CONFIG = 0x01;
        private const byte ADC_REG_POINTER_LOTRESHOLD = 0x02;
        private const byte ADC_REG_POINTER_HITRESHOLD = 0x03;
        #endregion

        private I2cDevice adc;

        private bool[] enabledInputs = new bool[4];

        public bool IsInitialized { get; private set; }

        public const int ADC_RES = 65536;
        public const int ADC_HALF_RES = 32768;

        public SensorADS1115(AdcAddress ads1115Addresses)
        {
            ADC_I2C_ADDR = (byte)ads1115Addresses;
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

        // if the AdcInput is one of the single ended inputs it measure all pin with the same settings
        //only works in single shoot mode, havent tested yet
        public async Task<ADS1115SensorsData> GetAllSensorDataSingleEnded(ADS1115SensorSetting setting)
        {
            // in differential mode it's harder to define the other input so it works only in single shoot mode
            if ((byte)setting.Input <= 0x03)
                throw new InvalidOperationException("It's not allowed to run with differential input");

            var sensorData = new ADS1115SensorsData();
            int temp;

            setting.Input = AdcInput.A0_SE;
            temp = await ReadSensorAsync(configA(setting), configB(setting));
            sensorData.A0.DecimalValue = temp;
            sensorData.A0.VoltageValue = DecimalToVoltage(setting.Pga, temp, ADC_HALF_RES);

            setting.Input = AdcInput.A1_SE;
            temp = await ReadSensorAsync(configA(setting), configB(setting));
            sensorData.A1.DecimalValue = temp;
            sensorData.A1.VoltageValue = DecimalToVoltage(setting.Pga, temp, ADC_HALF_RES);

            setting.Input = AdcInput.A2_SE;
            temp = await ReadSensorAsync(configA(setting), configB(setting));
            sensorData.A2.DecimalValue = temp;
            sensorData.A2.VoltageValue = DecimalToVoltage(setting.Pga, temp, ADC_HALF_RES);

            setting.Input = AdcInput.A3_SE;
            temp = await ReadSensorAsync(configA(setting), configB(setting));
            sensorData.A3.DecimalValue = temp;
            sensorData.A3.VoltageValue = DecimalToVoltage(setting.Pga, temp, ADC_HALF_RES);
            return sensorData;
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

        // get single sensor's data
        public async Task<ADS1115SensorData> GetSingleSensorData(ADS1115SensorSetting setting)
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

        // get a single measurement with the given config 
        // it's the best for single-shot mode with large sps, should be optimalized later
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

        public void Dispose()
        {
            adc.Dispose();
        }
    }

    class ADS1115SensorsData
    {
        public ADS1115SensorData A0 { get; set; }
        public ADS1115SensorData A1 { get; set; }
        public ADS1115SensorData A2 { get; set; }
        public ADS1115SensorData A3 { get; set; }
    }

    // class that contains the settings and each of them has the default values from the documentation
    class ADS1115SensorSetting
    {
        public AdcInput Input { get; set; } = AdcInput.A1_SE;
        public AdcPga Pga { get; set; } = AdcPga.G2;
        public AdcMode Mode { get; set; } = AdcMode.SINGLESHOOT_CONVERSION;
        public AdcDataRate DataRate { get; set; } = AdcDataRate.SPS128;
        public AdcComparatorMode ComMode { get; set; } = AdcComparatorMode.TRADITIONAL;
        public AdcComparatorPolarity ComPolarity { get; set; } = AdcComparatorPolarity.ACTIVE_LOW;
        public AdcComparatorLatching ComLatching { get; set; } = AdcComparatorLatching.LATCHING;
        public AdcComparatorQueue ComQueue { get; set; } = AdcComparatorQueue.DISABLE_COMPARATOR;
    }

    // class that contain a sensor read's result
    class ADS1115SensorData
    {
        public int DecimalValue { get; set; }
        public double VoltageValue { get; set; }
    }
}
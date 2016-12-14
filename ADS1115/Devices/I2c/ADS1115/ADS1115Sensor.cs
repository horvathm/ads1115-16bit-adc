using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.I2c;

namespace ADC.Devices.I2c.ADS1115
{
    /// <summary>
    /// Class that represents the ADS1115
    /// </summary>
    public sealed class ADS1115Sensor : IDisposable, IAnalogDititalConverter
    {
        #region Fields
        private readonly byte ADC_I2C_ADDR;                     // address of the ads1115
        private const byte ADC_REG_POINTER_CONVERSION = 0x00;   // pointer register values
        private const byte ADC_REG_POINTER_CONFIG = 0x01;
        private const byte ADC_REG_POINTER_LOTRESHOLD = 0x02;
        private const byte ADC_REG_POINTER_HITRESHOLD = 0x03;
        public const int ADC_RES = 65536;                       // resolutions in different conversion modes
        public const int ADC_HALF_RES = 32768;
        private I2cDevice adc;                                  // the device
        private bool fastReadAvailable = false;                 // if false you have to initialize before use read continuous
        #endregion

        public bool IsInitialized { get; private set; }

        /// <summary>
        /// Ctor. Sets the device address given in the parameter. 
        /// </summary>
        /// <param name="ads1115Addresses"></param>
        public ADS1115Sensor(AdcAddress ads1115Addresses = AdcAddress.GND)
        {
            ADC_I2C_ADDR = (byte)ads1115Addresses;
        }

        /// <summary>
        /// Free up the resources.
        /// </summary>
        public void Dispose()
        {
            adc.Dispose();
            adc = null;
        }

        /// <summary>
        /// Initialize sensor. You must initialize before use the ADS1115.
        /// </summary>
        /// <returns></returns>
        public async Task InitializeAsync()
        {
            if (IsInitialized)
            {
                throw new InvalidOperationException("The I2C ads1115 sensor is already initialized.");
            }

            // gets the default controller for the system, can be the lightning or any provider
            I2cController controller = await I2cController.GetDefaultAsync();

            var settings = new I2cConnectionSettings(ADC_I2C_ADDR);
            settings.BusSpeed = I2cBusSpeed.FastMode;
            // gets the I2CDevice from the controller using the connection setting
            adc = controller.GetDevice(settings);

            if (adc == null)
                throw new Exception("I2C controller not available on the system");

            IsInitialized = true;
        }

        /// <summary>
        /// Writes the configuration register.
        /// </summary>
        /// <param name="config"></param>
        public void writeConfig(byte[] config)
        {
            adc.Write(new byte[] { ADC_REG_POINTER_CONFIG, config[0], config[1] });

            fastReadAvailable = false;
        }

        /// <summary>
        /// Reads the configuration register. 
        /// </summary>
        /// <returns></returns>
        public async Task<byte[]> readConfig()
        {
            byte[] readRegister = new byte[2];
            adc.WriteRead(new byte[] { ADC_REG_POINTER_CONFIG }, readRegister);

            await Task.Delay(10);

            var writeBuffer = new byte[] { ADC_REG_POINTER_CONVERSION };
            adc.Write(writeBuffer);

            return readRegister;
        }

        /// <summary>
        /// Turns ALERT pin into conversion ready pin.
        /// </summary>
        public async void TurnAlertIntoConversionReady()
        {
            byte[] bytesH = BitConverter.GetBytes(0xFFFF);
            byte[] bytesL = BitConverter.GetBytes(0x0000);

            Array.Reverse(bytesH);
            Array.Reverse(bytesL);

            var writeBufferH = new byte[] { ADC_REG_POINTER_HITRESHOLD, bytesH[0], bytesH[1] };
            var writeBufferL = new byte[] { ADC_REG_POINTER_LOTRESHOLD, bytesL[0], bytesL[1] };

            adc.Write(writeBufferH); await Task.Delay(10);
            adc.Write(writeBufferL); await Task.Delay(10);

            var writeBuffer = new byte[] { ADC_REG_POINTER_CONVERSION };
            adc.Write(writeBuffer);
        }

        /// <summary>
        /// Writes the treshold registers. If the given value turns alert pin into conversion ready it throws ArgumentException.
        /// Default values are the default parameters. Lo: 0x8000 Hi: 0x7FFF
        /// </summary>
        /// <param name="loTreshold"></param>
        /// <param name="highTreshold"></param>
        /// <returns></returns>
        public async Task writeTreshold(UInt16 loTreshold = 32768, UInt16 highTreshold = 32767)
        {
            byte[] bytesH = BitConverter.GetBytes(highTreshold);
            byte[] bytesL = BitConverter.GetBytes(loTreshold);

            Array.Reverse(bytesH);
            Array.Reverse(bytesL);

            if (((bytesH[0] & 0x80) != 0) && ((bytesL[0] & 0x80) == 0))
                throw new ArgumentException("High treshold highest bit is 1 and low treshold highest bit is 0 witch disables treshold register");

            var writeBufferH = new byte[] { ADC_REG_POINTER_HITRESHOLD, bytesH[0], bytesH[1] };
            var writeBufferL = new byte[] { ADC_REG_POINTER_LOTRESHOLD, bytesL[0], bytesL[1] };

            adc.Write(writeBufferH);    await Task.Delay(10);
            adc.Write(writeBufferL);    await Task.Delay(10);

            var writeBuffer = new byte[] { ADC_REG_POINTER_CONVERSION };
            adc.Write(writeBuffer);
        }

        /// <summary>
        /// You have to initialize before use readContinuous and you have to do it every time when you write into the configuration register.
        /// </summary>
        /// <param name="setting"></param>
        /// <returns></returns>
        public async Task readContinuousInit(ADS1115SensorSetting setting)
        {
            if (setting.Mode != AdcMode.CONTINOUS_CONVERSION)
                throw new InvalidOperationException("You can only read in continuous mode");

            var command = new byte[] { ADC_REG_POINTER_CONFIG, configA(setting), configB(setting) };
            adc.Write(command);

            await Task.Delay(10);

            var writeBuffer = new byte[] { ADC_REG_POINTER_CONVERSION };
            adc.Write(writeBuffer);

            fastReadAvailable = true;
        }

        /// <summary>
        /// Read sensor in continuous mode.
        /// </summary>
        /// <returns></returns>
        public int readContinuous()
        {
            if (fastReadAvailable)
            {
                var readBuffer = new byte[2];
                adc.Read(readBuffer);

                if ((byte)(readBuffer[0] & 0x80) != 0x00)
                {
                    // two's complement conversion (two's complement byte array to int16)
                    readBuffer[0] = (byte)~readBuffer[0];
                    readBuffer[0] &= 0xEF;
                    readBuffer[1] = (byte)~readBuffer[1];
                    Array.Reverse(readBuffer);
                    return Convert.ToInt16(-1 * (BitConverter.ToInt16(readBuffer, 0) + 1));
                }
                else
                {
                    Array.Reverse(readBuffer);
                    return BitConverter.ToInt16(readBuffer, 0);
                }
            }
            else
            {
                throw new InvalidOperationException("It has to be initialized after every process that modifies configuration register");
            }
        }

        /// <summary>
        /// Read sensor in single-shot mode.
        /// </summary>
        /// <param name="setting"></param>
        /// <returns></returns>
        public async Task<ADS1115SensorData> readSingleShot(ADS1115SensorSetting setting)
        {
            if (setting.Mode != AdcMode.SINGLESHOOT_CONVERSION)
                throw new InvalidOperationException("You can only read in single shot mode");

            var sensorData = new ADS1115SensorData();
            int temp = await ReadSensorAsync(configA(setting), configB(setting));   //read sensor with the generated configuration bytes
            sensorData.DecimalValue = temp;

            //calculate the voltage with different resolutions in single ended and in differential mode
            if ((byte)setting.Input <= 0x03)
                sensorData.VoltageValue = DecimalToVoltage(setting.Pga, temp, ADC_RES);
            else
                sensorData.VoltageValue = DecimalToVoltage(setting.Pga, temp, ADC_HALF_RES);

            fastReadAvailable = false;

            return sensorData;
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
                // two's complement conversion (two's complement byte array to int16)
                readBuffer[0] = (byte)~readBuffer[0];
                readBuffer[0] &= 0xEF;
                readBuffer[1] = (byte)~readBuffer[1];
                Array.Reverse(readBuffer);
                return Convert.ToInt16(-1 * (BitConverter.ToInt16(readBuffer, 0) + 1));
            }
            else
            {
                Array.Reverse(readBuffer);
                return BitConverter.ToInt16(readBuffer, 0);
            }
        }

        // creates the byte byte of the configuration register from the ADS1115SensorSetting object
        private byte configA(ADS1115SensorSetting setting)
        {
            byte configA = 0;
            return configA = (byte)((byte)setting.Mode << 7 | (byte)setting.Input << 4 | (byte)setting.Pga << 1 | (byte)setting.Mode);
        }

        // creates the second byte of the configuration register from the ADS1115SensorSetting object
        private byte configB(ADS1115SensorSetting setting)
        {
            byte configB;
            return configB = (byte)((byte)setting.DataRate << 5 | (byte)setting.ComMode << 4 | (byte)setting.ComPolarity << 3 | (byte)setting.ComLatching << 2 | (byte)setting.ComQueue);
        }
        

        // creates the voltage value from the decimal value
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

            return (double)temp*(voltage/(double)resolution);
        }
    }

}
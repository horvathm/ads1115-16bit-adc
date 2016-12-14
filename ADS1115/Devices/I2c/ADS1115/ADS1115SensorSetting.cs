using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace ADC.Devices.I2c.ADS1115
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

    /// <summary>
    /// Class that contains the settings of the ADC. The default values are from the documentation.
    /// </summary>
    public class ADS1115SensorSetting : INotifyPropertyChanged
    {
        #region Properties
        public AdcInput Input
        { get { return _input; }
          set { Set(ref _input, value); }
        }
        private AdcInput _input = AdcInput.A1_SE;

        public AdcPga Pga
        {
            get { return _pga; }
            set { Set(ref _pga, value); }
        }
        private AdcPga _pga = AdcPga.G2;

        public AdcMode Mode
        {
            get { return _mode; }
            set { Set(ref _mode, value); }
        }
        private AdcMode _mode = AdcMode.SINGLESHOOT_CONVERSION;

        public AdcDataRate DataRate
        {
            get { return _dataRate; }
            set { Set(ref _dataRate, value); }
        }
        private AdcDataRate _dataRate = AdcDataRate.SPS128;

        public AdcComparatorMode ComMode
        {
            get { return _comMode; }
            set { Set(ref _comMode, value); }
        }
        private AdcComparatorMode _comMode = AdcComparatorMode.TRADITIONAL;

        public AdcComparatorPolarity ComPolarity
        {
            get { return _comPolarity; }
            set { Set(ref _comPolarity, value); }
        }
        private AdcComparatorPolarity _comPolarity = AdcComparatorPolarity.ACTIVE_LOW;

        public AdcComparatorLatching ComLatching
        {
            get { return _comLatching; }
            set { Set(ref _comLatching, value); }
        }
        private AdcComparatorLatching _comLatching = AdcComparatorLatching.LATCHING;

        public AdcComparatorQueue ComQueue
        {
            get { return _comQueue; }
            set { Set(ref _comQueue, value); }
        }
        private AdcComparatorQueue _comQueue = AdcComparatorQueue.DISABLE_COMPARATOR;
        #endregion

        #region INotifyPropertyChanged implementation
        public event PropertyChangedEventHandler PropertyChanged;

        public bool Set<T>(ref T storage, T value, [CallerMemberName]string propertyName = null)
        {
            // if unchanged return false
            if (Equals(storage, value))
                return false;
            storage = value;
            RaisePropertyChanged(propertyName);
            return true;
        }

        private void RaisePropertyChanged(string propertyName)
        {
            // if PropertyChanged not null call the Invoke method
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

    }
}

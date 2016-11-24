using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ADS1115.Devices.I2c.ADS1115
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

    public class ADS1115SensorSetting
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
}

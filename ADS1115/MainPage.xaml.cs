using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using ADC.Devices.I2c.ADS1115;
using Microsoft.IoT.Lightning.Providers;
using Windows.Devices;

namespace ADC
{
    public sealed partial class MainPage : Page, INotifyPropertyChanged
    {
        #region Fields
        private DispatcherTimer timer;
        private ADS1115Sensor adc;
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

        #region FieldsBindToComboBox
        public IEnumerable<AdcDataRate> DataRates => Enum.GetValues(typeof(AdcDataRate)).Cast<AdcDataRate>();
        public IEnumerable<AdcInput> Inputs => Enum.GetValues(typeof(AdcInput)).Cast<AdcInput>();
        public IEnumerable<AdcPga> Pgas => Enum.GetValues(typeof(AdcPga)).Cast<AdcPga>();
        public IEnumerable<AdcMode> Modes => Enum.GetValues(typeof(AdcMode)).Cast<AdcMode>();
        public IEnumerable<AdcComparatorMode> ComparatorModes => Enum.GetValues(typeof(AdcComparatorMode)).Cast<AdcComparatorMode>();
        public IEnumerable<AdcComparatorPolarity> ComparatorPolarities => Enum.GetValues(typeof(AdcComparatorPolarity)).Cast<AdcComparatorPolarity>();
        public IEnumerable<AdcComparatorLatching> ComparatorLatchings => Enum.GetValues(typeof(AdcComparatorLatching)).Cast<AdcComparatorLatching>();
        public IEnumerable<AdcComparatorQueue> ComparatorQueue => Enum.GetValues(typeof(AdcComparatorQueue)).Cast<AdcComparatorQueue>();
        #endregion

        #region Properties
        public double ConvertedValue
        {
            get { return _convertedValue; }
            set { Set(ref _convertedValue, value); }
        }
        private double _convertedValue = 0;

        public double ConvertedVoltage
        {
            get { return _convertedVoltage; }
            set { Set(ref _convertedVoltage, value); }
        }
        private double _convertedVoltage = 0;

        public ADS1115SensorSetting Setting
        {
            get { return _setting; }
            set { Set(ref _setting, value); }
        }
        private ADS1115SensorSetting _setting = new ADS1115SensorSetting();
        #endregion

        public MainPage()
        {
            this.InitializeComponent();

            // Setting the DataContext
            this.DataContext = this;

            // Register for the unloaded event so we can clean up upon exit
            Unloaded += MainPage_Unloaded;
            
            // Set Lightning as the default provider
            if (LightningProvider.IsLightningEnabled)
                LowLevelDevicesController.DefaultProvider = LightningProvider.GetAggregateProvider();

            // Initialize the DispatcherTimer
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(500);
            timer.Tick += timer_tick;

            // Initialize the sensors
            InitializeSensors();
        }

        private void MainPage_Unloaded(object sender, RoutedEventArgs e)
        {
            if(adc != null)
            {
                adc.Dispose();
                adc = null;
            };

            timer.Stop();
            timer = null;
        }

        private void timer_tick(object sender, object e)
        {
            if (adc != null && adc.IsInitialized)
            {
                try
                {
                    var temp = adc.readContinuous();
                    ConvertedVoltage = 0;
                    ConvertedValue = temp;
                }
                catch (Exception ex)
                {
                    throw new Exception("Continuous read has failed" + ex);
                }
            }
        }

        private async void InitializeSensors()
        {
            try
            {
                adc = new ADS1115Sensor(AdcAddress.GND);
                await adc.InitializeAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Initialization has failed: " + ex);
            }
        }

        private async void bt_convert_Click(object sender, RoutedEventArgs e)
        {
            if (Setting.Mode == AdcMode.CONTINOUS_CONVERSION)
            {
                if (adc != null && adc.IsInitialized)
                {
                    try
                    {
                        await adc.readContinuousInit(Setting);
                    }
                    catch (Exception ex)
                    {
                        throw new Exception("Initialization of continuous read has failed" + ex);
                    }

                    timer.Start();
                }      
            }
            else
            {
                timer.Stop();

                if (adc != null && adc.IsInitialized)
                {
                    try
                    {
                        var temp = await adc.readSingleShot(Setting);
                        ConvertedValue = temp.DecimalValue;
                        ConvertedVoltage = temp.VoltageValue;
                    }
                    catch (Exception ex)
                    {
                        throw new Exception("Read from ADS1115 has failed: " + ex);
                    }
                }
            }
        }

        private async void button_Click(object sender, RoutedEventArgs e)
        {
            await adc.writeTreshold(ushort.Parse(tb_tresh_a.Text), ushort.Parse(tb_tresh_b.Text));
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            adc.TurnAlertIntoConversionReady();
        }
    }
}


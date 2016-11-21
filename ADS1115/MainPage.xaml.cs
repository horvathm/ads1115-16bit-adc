using Microsoft.IoT.Lightning.Providers;
using ADS1115.Devices.I2c_devices;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Devices;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace ADS1115
{
    public sealed partial class MainPage : Page
    {
        #region Fields
        private DispatcherTimer timer;
        private SensorADS1115 adc;
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
        
        //xaml-be -> SelectedItem="{Binding Todo.Priority, Mode=TwoWay}"

        #region Properties
        public bool Mode
        {
            get {return _mode; }
            set {Set(ref _mode,value); }
        }
        private bool _mode = true;

        public double ConvertedValue
        {
            get { return _convertedValue; }
            set { Set(ref _convertedValue, value); }
        }
        private double _convertedValue = 0;
        #endregion

        public MainPage()
        {
            this.InitializeComponent();

            // Setting the DataContext
            this.DataContext = this;

            // Register for the unloaded event so we can clean up upon exit
            Unloaded += MainPage_Unloaded;
            /*
            // Set Lightning as the default provider
            if (LightningProvider.IsLightningEnabled)
                LowLevelDevicesController.DefaultProvider = LightningProvider.GetAggregateProvider();

            // Initialize the DispatcherTimer
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(1000);
            timer.Tick += timer_tick;
            timer.Start();
            
            // Initialize the sensors
            InitializeSensors();
            */
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
                var settings = new ADS1115SensorSetting()
                {
                    Mode = AdcMode.SINGLESHOOT_CONVERSION,
                    Pga = AdcPga.G1,
                    DataRate = AdcDataRate.SPS860
                };

                try
                {
                    /*
                    settings.Input = AdcInput.A1_SE;
                    HumidityC = (await adc.GetSingleSensorData(settings)).DecimalValue;
                    settings.Input = AdcInput.A2_SE;
                    HumidityD = (await adc.GetSingleSensorData(settings)).DecimalValue;
                    settings.Input = AdcInput.A0_SE;
                    settings.Pga = AdcPga.G2;
                    Luminance = (await adc.GetSingleSensorData(settings)).DecimalValue;
                    */

                }
                catch (Exception ex)
                {
                    throw new Exception("Read from ADS1115 has failed: " + ex);
                }
            }
        }

        private async void InitializeSensors()
        {
            try
            {
                adc = new SensorADS1115(AdcAddress.GND);
                await adc.InitializeAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Initialization has failed: " + ex);
            }
        }
    }
}

    /*
    ha singleshoot akkor a convert gombra mér egyet 
    ha continous c akkor letilt gonb és folyton frissítget egy számlálóval a prperty

     TODO: 
        hiányzik még a treshold reg
        doksi, méreget
        property a comboval miért szar itt? 
        lehetne mért értéket az adc-ben tárolni propertyben és ha cont mód akkor szálon adot időközönként frissítget => check más mo
     
     
     */
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using GalaSoft.MvvmLight;
using SQLite;
using GSRacing.RacingObjects;
using Windows.Media.Playback;
using Windows.Media.Core;
using Microsoft.IoT.Lightning.Providers;
using Windows.Devices;
using Windows.Devices.Pwm;
using Windows.Devices.Gpio;
using Windows.UI.Xaml;

namespace TimerMain.ViewModels
{
    public sealed class MainViewModel : ObservableObject, IDisposable
    {
        // singleton pattern from http://csharpindepth.com/Articles/General/Singleton.aspx
        private static readonly Lazy<MainViewModel> lazy =
            new Lazy<MainViewModel>(() => new MainViewModel());

        public static MainViewModel Instance { get { return lazy.Value; } }
        public MainMenuViewModel MainMenuVM = MainMenuViewModel.Instance;
        public EventSetupViewModel EventSetupVM = EventSetupViewModel.Instance;
        public HeatListViewModel HeatListVM = HeatListViewModel.Instance;
        public InHeatViewModel InHeatVM = InHeatViewModel.Instance;
        public ResultsViewModel ResultsVM = ResultsViewModel.Instance;
        MediaPlayer _mp = null;
        public bool InRace { get; set; } = false;

        public void ResetGates()
        {
            foreach (FinishGate fg in this.FinishGates)
                fg.ResetTrack();
        }


        private DateTime _LastGateChange;
        List<FinishGate> FinishGates = null;
        private MainViewModel()
        {
            _LastGateChange = DateTime.Now;
            Windows.Storage.StorageFolder storageFolder = Windows.Storage.ApplicationData.Current.LocalFolder;
            string szDataFilePath = string.Format("{0}{1}{2}", storageFolder.Path, System.IO.Path.DirectorySeparatorChar, "racedata.db3");
            this.conn = new SQLite.SQLiteConnection(szDataFilePath);
            this.conn.CreateTable<RaceEvent>();
            this.conn.CreateTable<Racer>();
            this.conn.CreateTable<RaceHeat>();
            this.conn.CreateTable<HeatTime>();
            this.conn.CreateTable<RaceResult>();

            _mp = new MediaPlayer();
            _mp.AutoPlay = false;

            if (!LightningProvider.IsLightningEnabled)
                return;

            LowLevelDevicesController.DefaultProvider = LightningProvider.GetAggregateProvider();
            OpenPwnPin(27);

            InitGPIO();
            this.SetState(ApplicationState.MainMenu);
        }

        async void OpenPwnPin(int iPin)
        {
            var pwmControllers = await PwmController.GetControllersAsync(LightningPwmProvider.GetPwmProvider());
            var pwmController = pwmControllers[1]; // use the on-device controller
            pwmController.SetDesiredFrequency(50); // try to match 50Hz, or 20ms
            _servoPin = pwmController.OpenPin(iPin);
            _servoPin.Start();
            _servoPin.SetActiveDutyCyclePercentage(0);
        }

        public SQLiteConnection conn { get; set; }

        private object _currentView = null;
        public object CurrentView
        {
            get { return _currentView; }
            set { this.Set(ref _currentView, value); }
        }

        private RaceEvent _currentEvent;
        public RaceEvent CurrentEvent
        {
            get { return _currentEvent; }
            set { this.Set(ref _currentEvent, value); }
        }

        public enum ApplicationState
        {
            MainMenu = 0,
            EventSetup = 1,
            HeatList = 2,
            InHeat = 3,
            Results = 4,
        }
        public void SetState(ApplicationState state)
        {
            switch (state)
            {
                case ApplicationState.MainMenu:
                    this.CurrentView = MainMenuVM;
                    break;
                case ApplicationState.EventSetup:
                    this.CurrentView = EventSetupVM;
                    break;
                case ApplicationState.HeatList:
                    this.CurrentView = HeatListVM;
                    break;
                case ApplicationState.InHeat:
                    InHeatVM.CurrentHeat = this.CurrentEvent.Heats[0];
                    this.CurrentView = InHeatVM;
                    this.PlaySound(SoundType.StartRace);
                    break;
                case ApplicationState.Results:
                    this.CurrentView = ResultsVM;
                    break;
                default:
                    throw new Exception("Unknown state: " + (int)state);
            }
        }

        private enum SoundType
        {
            StartRace = 0,
            StartHeat = 1,
        }
        private void PlaySound(SoundType st)
        {
            Uri pathUri = null;
            switch (st)
            {
                case SoundType.StartRace:
                    pathUri = new Uri("ms-appx:///Sounds/race1.wav");
                    break;
                case SoundType.StartHeat:
                    pathUri = new Uri("ms-appx:///Sounds/lights.wav");
                    break;
            }
            _mp.Source = MediaSource.CreateFromUri(pathUri);
            _mp.Play();
        }

        PwmPin _servoPin = null;

        private bool _fGateOpen;
        public bool IsGateOpen
        {
            get { return _fGateOpen; }
            set { this.Set(ref _fGateOpen, value); }
        }

        internal void SetServoState(bool fIsGateOpen)
        {
            _LastGateChange = DateTime.Now;
            _servoPin.SetActiveDutyCyclePercentage(fIsGateOpen ? .08 : .04);
            Task.Delay(500).ContinueWith(_ => _servoPin.SetActiveDutyCyclePercentage(0));            
            this.IsGateOpen = fIsGateOpen;
        }

        private GpioPin pushButton;

        private void InitGPIO()
        {
            LowLevelDevicesController.DefaultProvider =  LightningProvider.GetAggregateProvider();
            var gpio = GpioController.GetDefault();

            if (gpio == null)
                return;

            pushButton = gpio.OpenPin(17);
            pushButton.SetDriveMode(GpioPinDriveMode.Input); // VERY IMPORTANT. DO NOT SET OUTPUT
            pushButton.ValueChanged += PushButton_ValueChanged;

            FinishGates = new List<FinishGate>();
            FinishGate Track1FinishGate = new FinishGate(1, gpio, 4, InHeatVM.HeatStopWatch);
            Track1FinishGate.TrackCompleted += InHeatVM.TrackFinishGate_TrackCompleted;
            this.FinishGates.Add(Track1FinishGate);
            FinishGate Track2FinishGate = new FinishGate(2, gpio, 5, InHeatVM.HeatStopWatch);
            Track2FinishGate.TrackCompleted += InHeatVM.TrackFinishGate_TrackCompleted;
            this.FinishGates.Add(Track2FinishGate);
            FinishGate Track3FinishGate = new FinishGate(3, gpio, 6, InHeatVM.HeatStopWatch);
            Track3FinishGate.TrackCompleted += InHeatVM.TrackFinishGate_TrackCompleted;
            this.FinishGates.Add(Track3FinishGate);
            FinishGate Track4FinishGate = new FinishGate(4, gpio, 7, InHeatVM.HeatStopWatch);
            Track4FinishGate.TrackCompleted += InHeatVM.TrackFinishGate_TrackCompleted;
            this.FinishGates.Add(Track4FinishGate);
        }


        private void PushButton_ValueChanged(GpioPin sender, GpioPinValueChangedEventArgs args)
        {
            // debounce
            if (_LastGateChange.AddSeconds(2) > DateTime.Now)
                return;

            this.SetServoState(!this.IsGateOpen);
        }


        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                    this.conn.Close();
                    this.conn.Dispose();
                    this.conn = null;

                    pushButton.ValueChanged -= PushButton_ValueChanged;
                    pushButton.Dispose();
                    pushButton = null;

                    _servoPin.Dispose();
                    _servoPin = null;

                    foreach (FinishGate fg in FinishGates)
                        fg.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        ~MainViewModel()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(false);
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion

    }
}

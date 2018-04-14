using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GalaSoft.MvvmLight.Command;
using GSRacing.RacingObjects;
using GalaSoft.MvvmLight;
using Windows.UI.Xaml;
using Windows.Web.Http;

namespace TimerMain.ViewModels
{
    public class InHeatViewModel : ObservableObject
    {
        DispatcherTimer timer = new DispatcherTimer();
        //private const int TimerTickMilliseconds = 1000;
        public System.Diagnostics.Stopwatch HeatStopWatch { get; private set; }

        private static readonly Lazy<InHeatViewModel> lazy =
            new Lazy<InHeatViewModel>(() => new InHeatViewModel());

        public static InHeatViewModel Instance { get { return lazy.Value; } }
        private DispatcherTimer timerCountdown = new DispatcherTimer();

        private InHeatViewModel()
        {
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += Timer_Tick;
            HeatStopWatch = new System.Diagnostics.Stopwatch();
            timerCountdown.Interval = new TimeSpan(0, 0, 1);
            timerCountdown.Tick += TimerCountdown_Tick;
        }

        private void TimerCountdown_Tick(object sender, object e)
        {
            this.timerCountdown.Stop();
            CountdownAmt--;
            if (this.CountdownAmt > 0)
            {
                this.timerCountdown.Start();
            }
            else
            {
                this.CountdownVisible = false;
                this.StartRace();
            }
        }

        private bool _countdownVisible = false;
        public bool CountdownVisible
        {
            get { return _countdownVisible; }
            set { this.Set(ref _countdownVisible, value); }
        }

        private int _countdownAmt = 0;
        public int CountdownAmt
        {
            get { return _countdownAmt; }
            set { this.Set(ref _countdownAmt, value); RaisePropertyChanged("CountdownText"); }
        }

        public string CountdownText
        {
            get
            {
                if (_countdownAmt == 0)
                    return "Go!";

                return _countdownAmt.ToString();
            }
        }

        public MainViewModel MainVM
        {
            get { return MainViewModel.Instance; }
        }

        private RaceHeat _currentHeat;
        public RaceHeat CurrentHeat
        {
            get
            {
                return _currentHeat;
            }
            set
            {
                this.Set(ref _currentHeat, value);
                RaisePropertyChanged("NextHeat");
                RaisePropertyChanged("PreviousHeat");
                RaisePropertyChanged("HeatText");
                UpdateButtonStates();
                UpdateResults();
            }
        }
        private async void UpdateResults()
        {
            if (CurrentHeat == null)
                return;

            using (HttpClient hc = new HttpClient())
            {                
                string szGet =
                    "heatnum=" + CurrentHeat.HeatNumber +
                    "&totalheats=" + MainVM.CurrentEvent.Heats.Count +
                    "&racer1=" + CurrentHeat.HeatTimes[0].Racer.FullName +
                    "&racer2=" + CurrentHeat.HeatTimes[1].Racer.FullName +
                    "&racer3=" + CurrentHeat.HeatTimes[2].Racer.FullName +
                    "&racer4=" + CurrentHeat.HeatTimes[3].Racer.FullName;
                if (NextHeat != null)
                {
                    szGet +=
                    "&nextracer1=" + NextHeat.HeatTimes[0].Racer.FullName +
                    "&nextracer2=" + NextHeat.HeatTimes[1].Racer.FullName +
                    "&nextracer3=" + NextHeat.HeatTimes[2].Racer.FullName +
                    "&nextracer4=" + NextHeat.HeatTimes[3].Racer.FullName;
                }
                else
                {
                    szGet +=
                    "&nextracer1=" +
                    "&nextracer2=" +
                    "&nextracer3=" +
                    "&nextracer4=";
                }
                await hc.GetAsync(new Uri("http://localhost:2000/apiheatchanged?" + szGet));
            }

        }
        public void UpdateButtonStates()
        {
            this.GoBackToLastHeatCommand.RaiseCanExecuteChanged();
            this.GoToNextHeatCommand.RaiseCanExecuteChanged();
            this.ResetHeatCommand.RaiseCanExecuteChanged();
            this.SendStartRaceCommand.RaiseCanExecuteChanged();
        }

        public string HeatText
        {
            get
            {
                if (CurrentHeat == null)
                    return string.Empty;

                return string.Format("Heat {0} of {1}", this.CurrentHeat.HeatNumber, MainVM.CurrentEvent.Heats.Count);
            }
        }


        public RaceHeat NextHeat
        {
            get
            {
                if (MainVM.CurrentEvent == null)
                    return null;

                int currIndex = MainVM.CurrentEvent.Heats.IndexOf(this.CurrentHeat);
                if (currIndex == MainVM.CurrentEvent.Heats.Count - 1)
                    return null;

                return MainVM.CurrentEvent.Heats[currIndex + 1];
            }
        }
        public RaceHeat PreviousHeat
        {
            get
            {
                if (MainVM.CurrentEvent == null)
                    return null;

                int currIndex = MainVM.CurrentEvent.Heats.IndexOf(this.CurrentHeat);
                if (currIndex == 0)
                    return null;

                return MainVM.CurrentEvent.Heats[currIndex - 1];
            }
        }

        public async void TrackFinishGate_TrackCompleted(object sender, TrackCompletedEventArgs e)
        {
            if (!MainVM.InRace)
                return;

            System.Diagnostics.Debug.WriteLine("Car finished: " + e.TrackNo);
            HeatTime ht = this.CurrentHeat.HeatTimes.First(x => x.TrackNumber == e.TrackNo);
            if (ht != null)
            {
                ht.RaceTime = e.ElapsedTime;
            }
            using (HttpClient hc = new HttpClient())
            {
                string szGet = "http://localhost:2000/apiracercompleted?track=" + ht.TrackNumber + "&time=" + ht.RaceTime;
                await hc.GetAsync(new Uri(szGet));
            }

        }

        RelayCommand _goBackToLastHeatCommand;
        public RelayCommand GoBackToLastHeatCommand
        {
            get
            {
                if (_goBackToLastHeatCommand == null)
                {
                    _goBackToLastHeatCommand = new RelayCommand(this.GoBackToLastHeat, () => { return this.CurrentHeat != MainVM.CurrentEvent.Heats[0]; });
                }
                return _goBackToLastHeatCommand;
            }
        }

        RelayCommand _goToNextHeatCommand;
        public RelayCommand GoToNextHeatCommand
        {
            get
            {
                if (_goToNextHeatCommand == null)
                {
                    _goToNextHeatCommand = new RelayCommand(this.GoToNextHeat, () => { return this.CurrentHeat.Completed; });
                }
                return _goToNextHeatCommand;
            }
        }

        RelayCommand _resetHeatCommand;
        public RelayCommand ResetHeatCommand
        {
            get
            {
                if (_resetHeatCommand == null)
                {
                    _resetHeatCommand = new RelayCommand(this.ResetHeat, () => { return this.CurrentHeat.Completed; });
                }
                return _resetHeatCommand;
            }
        }

        RelayCommand _sendStartRaceCommand;
        public RelayCommand SendStartRaceCommand
        {
            get
            {
                if (_sendStartRaceCommand == null)
                {
                    _sendStartRaceCommand = new RelayCommand(this.SendStartRace, () => { return (!MainVM.InRace && !this.CurrentHeat.Completed); });
                }
                return _sendStartRaceCommand;
            }
        }

        private void GoToNextHeat()
        {
            if (this.NextHeat != null)
            {
                this.CurrentHeat = this.NextHeat;
            }
            else
            {
                if (MainVM.CurrentEvent.Results.Count == 0)
                    MainVM.CurrentEvent.CalculateResults(this.MainVM.conn);

                MainVM.CurrentEvent.Completed = true;
                this.MainVM.SetState(MainViewModel.ApplicationState.Results);
            }
            MainVM.CurrentEvent.Save(this.MainVM.conn);
        }

        internal void StartRace()
        {
            this.MainVM.ResetGates();
            this.TimerTicksCtr = 0;
            timer.Start();
            this.HeatStopWatch.Reset();
            this.HeatStopWatch.Start();
            MainVM.SetServoState(true);
        }

        private int TimerTicksCtr = 0;
        private void Timer_Tick(object sender, object e)
        {
            TimerTicksCtr++;
            CurrentTimerTime = this.HeatStopWatch.Elapsed.TotalSeconds;
            if (TimerTicksCtr < 8)
            {
                if (this.CurrentHeat.HeatTimes.Any(x => x.RaceTime == null))
                    return;
            }

            timer.Stop();
            this.HeatStopWatch.Stop();
            this.CurrentHeat.ForceComplete(8);
            MainVM.InRace = false;
            UpdateButtonStates();
        }

        private double _currentTimerTime = 0;
        public double CurrentTimerTime
        {
            get { return _currentTimerTime; }
            set { this.Set(ref _currentTimerTime, value); }
        }

        private void SendStartRace()
        {
            if (this.CurrentHeat.Completed)
                return;

            //System.Windows.Media.MediaPlayer mp = new System.Windows.Media.MediaPlayer();
            //Uri uri1 = new Uri(@"pack://siteoforigin:,,,/Sounds/lights.wav", UriKind.Absolute);
            //mp.Open(uri1);
            //mp.Play();
            MainVM.InRace = true;
            this.CountdownVisible = true;
            this.CountdownAmt = 4;
            this.timerCountdown.Start();
            this.SendStartRaceCommand.RaiseCanExecuteChanged();
        }


        private void GoBackToLastHeat()
        {
            if (this.PreviousHeat != null)
            {
                this.CurrentHeat = this.PreviousHeat;
            }
        }

        private void ResetHeat()
        {
            if (this.CurrentHeat == null)
                return;

            this.CurrentHeat.Completed = false;
            
            foreach (HeatTime ht in this.CurrentHeat.HeatTimes)
            {
                ht.RaceTime = null;
            }
            UpdateButtonStates();
        }
    }
}

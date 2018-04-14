using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GalaSoft.MvvmLight.Command;
using GSRacing.RacingObjects;

namespace TimerMain.ViewModels
{
    public class HeatListViewModel
    {
        private static readonly Lazy<HeatListViewModel> lazy =
            new Lazy<HeatListViewModel>(() => new HeatListViewModel());

        public static HeatListViewModel Instance { get { return lazy.Value; } }

        private HeatListViewModel()
        {
        }

        public RaceEvent CurrentEvent
        {
            get { return MainViewModel.Instance.CurrentEvent; }
        }


        RelayCommand _startRaceCommand;
        public RelayCommand StartRaceCommand
        {
            get
            {
                if (_startRaceCommand == null)
                {
                    _startRaceCommand = new RelayCommand(this.StartRace);
                }
                return _startRaceCommand;
            }
        }

        private void StartRace()
        {
            MainViewModel mainvm = MainViewModel.Instance;
            mainvm.SetState(MainViewModel.ApplicationState.InHeat);
        }

    }
}

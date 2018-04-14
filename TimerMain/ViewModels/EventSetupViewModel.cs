using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GSRacing.RacingObjects;

namespace TimerMain.ViewModels
{
    public class EventSetupViewModel : ObservableObject
    {
        private static readonly Lazy<EventSetupViewModel> lazy =
            new Lazy<EventSetupViewModel>(() => new EventSetupViewModel());

        public static EventSetupViewModel Instance { get { return lazy.Value; } }

        private EventSetupViewModel()
        {
        }

        public RaceEvent CurrentEvent
        {
            get { return MainViewModel.Instance.CurrentEvent; }
        }
        public DateTimeOffset CurrentEventDateTimeOffset
        {
            get { return MainViewModel.Instance.CurrentEvent.EventDate; }
            set
            {
                CurrentEvent.EventDate = value.DateTime;
                this.RaisePropertyChanged();
            }
        }
        public List<int> TrackCountList
        {
            get
            {
                List<int> li = new List<int>();
                for (int i = 1; i < 13; i++)
                    li.Add(i);
                return li;
            }
        }

        RelayCommand _addNewRacerCommand;
        public RelayCommand AddNewRacerCommand
        {
            get
            {
                if (_addNewRacerCommand == null)
                {
                    _addNewRacerCommand = new RelayCommand(this.AddNewRacer);
                }
                return _addNewRacerCommand;
            }
        }

        RelayCommand<Racer> _removeRacerCommand;
        public RelayCommand<Racer> RemoveRacerCommand
        {
            get
            {
                if (_removeRacerCommand == null)
                {
                    _removeRacerCommand = new RelayCommand<Racer>(r => this.RemoveRacer(r));
                }
                return _removeRacerCommand;
            }
        }

        RelayCommand _createHeatsCommand;
        public RelayCommand CreateHeatsCommand
        {
            get
            {
                if (_createHeatsCommand == null)
                {
                    _createHeatsCommand = new RelayCommand(this.CreateHeats);
                }
                return _createHeatsCommand;
            }
        }

        private void AddNewRacer()
        {
            //Racer r = new Racer();
            //RaceEvent re = ViewModels.MainViewModel.Instance.CurrentEvent;
            //r.RegNumber = re.Racers.Count + 1;
            //r.RacerID = Guid.NewGuid();
            //r.EventID = re.EventID;
            //re.Racers.Add(r);
            Racer r1 = new Racer();
            RaceEvent re = ViewModels.MainViewModel.Instance.CurrentEvent;
            r1.RegNumber = re.Racers.Count + 1;
            r1.RacerID = Guid.NewGuid();
            r1.EventID = re.EventID;
            r1.FirstName = "Lynn";
            r1.LastName = "Crumbling";
            re.Racers.Add(r1);
            Racer r2 = new Racer();
            r2.RegNumber = re.Racers.Count + 1;
            r2.RacerID = Guid.NewGuid();
            r2.EventID = re.EventID;
            r2.FirstName = "Imogen";
            r2.LastName = "Crumbling";
            re.Racers.Add(r2);
            Racer r3 = new Racer();
            r3.RegNumber = re.Racers.Count + 1;
            r3.RacerID = Guid.NewGuid();
            r3.EventID = re.EventID;
            r3.FirstName = "Griffin";
            r3.LastName = "Crumbling";
            re.Racers.Add(r3);
            Racer r4 = new Racer();
            r4.RegNumber = re.Racers.Count + 1;
            r4.RacerID = Guid.NewGuid();
            r4.EventID = re.EventID;
            r4.FirstName = "Griffin";
            r4.LastName = "Crumbling";
            re.Racers.Add(r4);
        }

        private void RemoveRacer(Racer r)
        {
            ViewModels.MainViewModel.Instance.CurrentEvent.Racers.Remove(r);
        }
        private void CreateHeats()
        {
            RaceEvent re = ViewModels.MainViewModel.Instance.CurrentEvent;
            if (re.Racers.Count == 0)
                return;

            if (re.Heats.Count == 0)
            {
                re.CreateHeats(ViewModels.MainViewModel.Instance.conn);
                re.Save(ViewModels.MainViewModel.Instance.conn);
            }

            ViewModels.MainViewModel.Instance.SetState(MainViewModel.ApplicationState.HeatList);
        }

    }
}

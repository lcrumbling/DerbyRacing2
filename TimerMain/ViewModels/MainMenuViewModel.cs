using System;
using System.Collections.ObjectModel;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GSRacing.RacingObjects;
using Windows.System;

namespace TimerMain.ViewModels
{
    public sealed class MainMenuViewModel : ObservableObject
    {
        private static readonly Lazy<MainMenuViewModel> lazy =
            new Lazy<MainMenuViewModel>(() => new MainMenuViewModel());

        public static MainMenuViewModel Instance { get { return lazy.Value; } }

        private MainMenuViewModel()
        {
        }

        RelayCommand _loadEventCommand;
        public RelayCommand LoadEventCommand
        {
            get
            {
                if (_loadEventCommand == null)
                {
                    _loadEventCommand = new RelayCommand(this.LoadEvent);
                }
                return _loadEventCommand;
            }
        }

        RelayCommand _createEventCommand;
        public RelayCommand CreateEventCommand
        {
            get
            {
                if (_createEventCommand == null)
                {
                    _createEventCommand = new RelayCommand(this.CreateEvent);
                }
                return _createEventCommand;
            }
        }

        RelayCommand _deleteEventCommand;
        public RelayCommand DeleteEventCommand
        {
            get
            {
                if (_deleteEventCommand == null)
                {
                    _deleteEventCommand = new RelayCommand(this.DeleteEvent);
                }
                return _deleteEventCommand;
            }
        }

        private RaceEvent _selectedEvent;
        public RaceEvent SelectedEvent
        {
            get { return _selectedEvent; }
            set { this.Set(ref _selectedEvent, value); }
        }

        private void LoadEvent()
        {
            if (this.SelectedEvent == null)
                return;

            ViewModels.MainViewModel.Instance.CurrentEvent = new RaceEvent(this.SelectedEvent.EventID, ViewModels.MainViewModel.Instance.conn);
            ViewModels.MainViewModel.Instance.SetState(MainViewModel.ApplicationState.EventSetup);
        }

        private void CreateEvent()
        {
            ViewModels.MainViewModel.Instance.CurrentEvent = new RaceEvent();
            ViewModels.MainViewModel.Instance.CurrentEvent.EventName = "New Event";
            ViewModels.MainViewModel.Instance.CurrentEvent.EventDate = DateTime.Now.Date;
            ViewModels.MainViewModel.Instance.CurrentEvent.EventID = Guid.NewGuid();
            ViewModels.MainViewModel.Instance.CurrentEvent.Save(ViewModels.MainViewModel.Instance.conn);
            ViewModels.MainViewModel.Instance.SetState(MainViewModel.ApplicationState.EventSetup);
        }

        private void DeleteEvent()
        {
            RaceEvent.Delete(this.SelectedEvent.EventID, ViewModels.MainViewModel.Instance.conn);
        }
        public ObservableCollection<RaceEvent> AllEvents
        {
            get
            {
                return RaceEvent.AllEvents(ViewModels.MainViewModel.Instance.conn);
            }
        }

        public object[] EventList
        {
            get { return null; }
        }

        private RelayCommand _shutdownCommand;
        public RelayCommand ShutdownCommand
        {
            get
            {
                if (_shutdownCommand == null)
                    _shutdownCommand = new RelayCommand(() =>  ShutdownManager.BeginShutdown(ShutdownKind.Shutdown, TimeSpan.FromSeconds(0)));
                return _shutdownCommand;
            }
        }

    }
}

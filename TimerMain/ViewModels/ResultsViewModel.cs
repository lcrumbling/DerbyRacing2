using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Windows.System;
namespace TimerMain.ViewModels
{
    public class ResultsViewModel : ObservableObject
    {
        private static readonly Lazy<ResultsViewModel> lazy =
            new Lazy<ResultsViewModel>(() => new ResultsViewModel());

        public static ResultsViewModel Instance { get { return lazy.Value; } }

        private ResultsViewModel()
        {
        }
        public MainViewModel MainVM
        {
            get { return MainViewModel.Instance; }
        }
        private RelayCommand _mainMenuCommand;
        public RelayCommand MainMenuCommand
        {
            get
            {
                if (_mainMenuCommand == null)
                    _mainMenuCommand = new RelayCommand(() => { MainVM.SetState(MainViewModel.ApplicationState.MainMenu); });
                return _mainMenuCommand;
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Diagnostics;
using TestStand.Controls;
using TestStand.Model;
using TestStand.Services;
using TestStand.View;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Navigation;
using Jupiter.Application;
using Jupiter.Mvvm;
using Jupiter.Services.Navigation;
using Jupiter.Utils.Helpers;

namespace TestStand.ViewModel
{
    public class UserTimeoutViewModel : ViewModelBase
    {
        private int _remainingSeconds;
        private string _leftTimeString;

        private string _badgeId = string.Empty;
        private bool _scanned;
        private EmployeeService _employeeService = Ioc.Resolve<EmployeeService>();

        private static DispatcherTimer _remainingTimer = new DispatcherTimer();

        #region Commands

        /// <summary>
        /// Переходим на главную страницу
        /// </summary>
        public DelegateCommand GoToMainViewCommand { get; private set; }

        #endregion

        public int RemainingSeconds
        {
            get { return _remainingSeconds; }
            set
            {
                if (Set(ref _remainingSeconds, value))
                    LeftTimeString = _remainingSeconds + " " + StringHelper.LocalizeNumerals(_remainingSeconds, "секунда", "секунды", "секунд");
            }
        }

        public string LeftTimeString
        {
            get { return _leftTimeString; }
            set
            {
                Set(ref _leftTimeString, value);
            }
        }

        public UserTimeoutViewModel()
        {
            RemainingSeconds = 20;

            _remainingTimer.Tick += RemainingTimerTick;
            _remainingTimer.Interval = TimeSpan.FromSeconds(1);
            _remainingTimer.Start();
        }

        public override void OnNavigatedTo(Dictionary<string, object> parameters, NavigationMode mode)
        {
            CoreWindow.GetForCurrentThread().KeyDown += UserTimeoutViewModel_KeyDown;

            Settings.IsAuthorized = false;
            base.OnNavigatedTo(parameters, mode);
        }

        public override void OnNavigatingFrom(NavigatingEventArgs e)
        {
            CoreWindow.GetForCurrentThread().KeyDown -= UserTimeoutViewModel_KeyDown;
            base.OnNavigatingFrom(e);
        }

        private void UserTimeoutViewModel_KeyDown(CoreWindow sender, KeyEventArgs args)
        {
            Debug.WriteLine(args.VirtualKey.ToString());

            if (_badgeId.Length <= AppConst.MAX_BADGEID_LENGTH)
            {
                var n = Math.Abs((int)VirtualKey.Number0 - (int)args.VirtualKey);
                if (_badgeId.Length < 3 && n != 0)
                    return;

                if (_badgeId.Length >= 3 && !_badgeId.StartsWith("000"))
                    return;

                _badgeId += n;
            }

            if (_badgeId.Length == AppConst.MAX_BADGEID_LENGTH && !_scanned)
            {
                _scanned = true;
                CoreWindow.GetForCurrentThread().KeyDown -= UserTimeoutViewModel_KeyDown;


                if (Settings.CurrentSessionBadgeId == _badgeId)
                {
                    _remainingTimer.Stop();
                    _remainingTimer.Tick -= RemainingTimerTick;

                    Popup.Current.Close();
                    LogOutService.Restart();
                }
                    
                _scanned = false;
                _badgeId = string.Empty;

                CoreWindow.GetForCurrentThread().KeyDown += UserTimeoutViewModel_KeyDown;                              
            }
        }

        protected override void InitializeCommands()
        {
            GoToMainViewCommand = new DelegateCommand(() =>
            {
                _remainingTimer.Stop();
                _remainingTimer.Tick -= RemainingTimerTick;

                Popup.Current.Close();
                JupiterApp.Current.NavigationService.Navigate(typeof(MainView), parameter: null, clearHistory: true);
            });
        }
       
        private void RemainingTimerTick(object sender, object e)
        {
            RemainingSeconds--;

            if (RemainingSeconds == 0)
            {
                _remainingTimer.Stop();
                _remainingTimer.Tick -= RemainingTimerTick;

                Popup.Current.Close();
                JupiterApp.Current.NavigationService.Navigate(typeof(MainView), parameter: null, clearHistory: true);
            }
        }
    }
}

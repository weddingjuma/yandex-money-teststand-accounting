using System.Collections.Generic;
using TestStand.View;
using Jupiter.Mvvm;
using System;
using System.Diagnostics;
using Windows.UI.Core;
using Windows.UI.Xaml.Navigation;
using Jupiter.Services.Navigation;
using TestStand.Model;
using Windows.System;
using Windows.UI.Popups;
using Jupiter.Utils.Helpers;
using TestStand.Services;

namespace TestStand.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        private string _badgeId = string.Empty;
        private Employee _employee;
        private bool _scanned;
        private EmployeeService _employeeService = Ioc.Resolve<EmployeeService>();

        #region Commands
        /// <summary>
        /// Команда перехода к списку телефонов
        /// </summary>
        public DelegateCommand GoToDevicesCommand { get; private set; }

        /// <summary>
        /// Команда перехода к истории
        /// </summary>
        public DelegateCommand GoToHistoryCommand { get; private set; }

        #endregion

        private string _appVersion;
        public string AppVersion
        {
            get { return _appVersion; }
            set
            {
                Set(ref _appVersion, value);
            }
        }

        /// <summary>
        /// Сканированией пропуска и получение данных
        /// </summary>
        public override void OnNavigatedTo(Dictionary<string, object> parameters, NavigationMode mode)
        {
            CoreWindow.GetForCurrentThread().KeyDown += ScannerViewModel_KeyDown;

            Settings.IsAuthorized = false;
            Settings.CurrentSessionBadgeId = null;
            LogOutService.Stop();

            AppVersion = AppInfoHelper.GetAppVersionString();

            base.OnNavigatedTo(parameters, mode);
        }

        public override void OnNavigatingFrom(NavigatingEventArgs e)
        {
            CoreWindow.GetForCurrentThread().KeyDown -= ScannerViewModel_KeyDown;
            base.OnNavigatingFrom(e);
        }

        private async void ScannerViewModel_KeyDown(CoreWindow sender, KeyEventArgs args)
        {
            Debug.WriteLine(args.VirtualKey.ToString());

            if (_badgeId.Length <= AppConst.MAX_BADGEID_LENGTH)
            {
                var n = Math.Abs((int)VirtualKey.Number0 - (int)args.VirtualKey);
                if (_badgeId.Length < 3 && n != 0)
                    return;

                if (_badgeId.Length >= 3 &&!_badgeId.StartsWith("000"))
                    return;

                _badgeId += n;
            }

            if (_badgeId.Length == AppConst.MAX_BADGEID_LENGTH && !_scanned)
            {
                _scanned = true;
                CoreWindow.GetForCurrentThread().KeyDown -= ScannerViewModel_KeyDown;
                try
                {
                    _employee = await _employeeService.GetEmployeeByBadgeId(_badgeId);

                    Settings.CurrentSessionBadgeId = _badgeId;

                    if (_employee == null)
                    {
                        _employee = await _employeeService.AddEmployeeInDbByBadgeId(_badgeId);
                    }

                    NavigationService.Navigate(typeof(ScannerView), new Dictionary<string, object>()
                    {
                        ["Employee"] = _employee
                    });
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex);

                    _scanned = false;
                    _badgeId = string.Empty;
                    CoreWindow.GetForCurrentThread().KeyDown += ScannerViewModel_KeyDown;
                    await new MessageDialog("Ой! Что-то пошло не так!", "Обратитесь к администратору системы! Спасибо :)").ShowAsync();
                }
            }
        }

        protected override void InitializeCommands()
        {
            GoToDevicesCommand = new DelegateCommand(() =>
            {
                NavigationService.Navigate(typeof(DevicesView));
            });

            GoToHistoryCommand = new DelegateCommand(() =>
            {
                NavigationService.Navigate(typeof(HistoryView));
            });
        }
    }
}
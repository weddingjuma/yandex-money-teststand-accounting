using System.Collections.ObjectModel;
using TestStand.Model;
using Jupiter.Mvvm;
using TestStand.Services;
using TestStand.View;
using System.Linq;
using System;
using System.Collections.Generic;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Jupiter.Application;

namespace TestStand.ViewModel
{
    public class DevicesViewModel : ViewModelBase
    {
        private ObservableCollection<DeviceEmployeeEntry> _deviceEmployeeEntries;
        private List<DeviceEmployeeEntry> _deviceEmployees;

        #region Commands
        /// <summary>
        /// Переходим на страницу истории девайса
        /// </summary>
        public DelegateCommand<Device> GoToDeviceHistoryCommand { get; private set; }

        /// <summary>
        /// Переходим на предыдущую страницу
        /// </summary>
        public DelegateCommand GoBackCommand { get; private set; }

        #endregion

        public ObservableCollection<DeviceEmployeeEntry> DeviceEmployeeEntries
        {
            get { return _deviceEmployeeEntries; }
            set
            {
                Set(ref _deviceEmployeeEntries, value);
            }
        }

        public DevicesViewModel()
        {
            if (!IsInDesignMode)
                LoadData();

        }

        public override void OnNavigatedTo(Dictionary<string, object> parameters, NavigationMode mode)
        {
            LogOutService.Restart();
        }

        private async void LoadData()
        {           
            var deviceService = Ioc.Resolve<DeviceService>();
            var historyService = Ioc.Resolve<HistoryService>();

            _deviceEmployees = await deviceService.GetDevicesWithEmployeesAsync();

            foreach (var deviceEntry in _deviceEmployees)
            {
                var deviceHistory = await historyService.GetHistoryByDevice(deviceEntry.Device.Id);
                if (deviceHistory != null && deviceHistory.Count > 0)
                {
                    var date = deviceHistory.OrderBy(d => d.Date).Last().Date;
                    if (date != DateTime.MinValue)
                        deviceEntry.Date = date;
                }
            }

            DeviceEmployeeEntries = new ObservableCollection<DeviceEmployeeEntry>(_deviceEmployees);

        }

        protected override void InitializeCommands()
        {
            GoToDeviceHistoryCommand = new DelegateCommand<Device>(device =>
            {
                NavigationService.Navigate(typeof(DeviceHistoryView), new Dictionary<string, object>()
                {
                    ["Device"] = device
                });
            });

            GoBackCommand = new DelegateCommand(() =>
            {
                NavigationService.GoBack();
            });
        }

        public void Asb_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput & !string.IsNullOrEmpty(sender.Text))
            {
                var matchingContacts = GetMatchingContacts(sender.Text);
                DeviceEmployeeEntries = new ObservableCollection<DeviceEmployeeEntry>(matchingContacts);
            }
            else
                DeviceEmployeeEntries = new ObservableCollection<DeviceEmployeeEntry>(_deviceEmployees);
        }

        public IEnumerable<DeviceEmployeeEntry> GetMatchingContacts(string query)
        {
            return DeviceEmployeeEntries
                .Where(c => c.Device.Model.IndexOf(query, StringComparison.CurrentCultureIgnoreCase) > -1 ||
                       c.Employee?.FirstName?.IndexOf(query, StringComparison.CurrentCultureIgnoreCase) > -1)
                .OrderByDescending(c => c.Device.Model.StartsWith(query, StringComparison.CurrentCultureIgnoreCase));
        }
    }
}
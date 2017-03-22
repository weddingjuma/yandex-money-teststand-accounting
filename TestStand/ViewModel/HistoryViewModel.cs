using SQLite;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TestStand.Model;
using TestStand.Services;
using Windows.Storage;
using Windows.UI.Xaml.Navigation;
using Jupiter.Application;
using Jupiter.Mvvm;
using TestStand.View;


namespace TestStand.ViewModel
{
    public class HistoryViewModel : ViewModelBase
    {

        private DeviceService _deviceService = Ioc.Resolve<DeviceService>();


        private List<HistoryDeviceEmployeeEntry> _history;

        #region Commands

        /// <summary>
        /// Переходим на предыдущую страницу
        /// </summary>
        public DelegateCommand GoBackCommand { get; private set; }

        #endregion

        public List<HistoryDeviceEmployeeEntry> History
        {
            get { return _history; }
            set { Set(ref _history, value); }
        }

        public HistoryViewModel()
        {
            if (!IsInDesignMode)
                Load();
        }

        public override void OnNavigatedTo(Dictionary<string, object> parameters, NavigationMode mode)
        {
            LogOutService.Restart();
        }


        private async void Load()
        {
            var historyService = Ioc.Resolve<HistoryService>();
            var history = await historyService.GetHistory();
            var employes = new List<Employee>();
            var devices = new List<Device>();
            var result = new List<HistoryDeviceEmployeeEntry>();

            string сonnectionString = Path.Combine(ApplicationData.Current.LocalFolder.Path, "TestStand.sqlite");
            var сonnection = new SQLiteAsyncConnection(сonnectionString);

            foreach (var historyItem in history)
            {
                var i = new HistoryDeviceEmployeeEntry();

                var device = devices.FirstOrDefault(e => e.Id == historyItem.DeviceId);
                if (device == null)
                {
                    device = await сonnection.Table<Device>().Where(r => r.Id == historyItem.DeviceId).FirstOrDefaultAsync();
                    devices.Add(device);
                }

                var employee = employes.FirstOrDefault(e => e.BadgeId == historyItem.BadgeId);
                if (employee == null)
                {
                    employee = await сonnection.Table<Employee>().Where(r => r.BadgeId == historyItem.BadgeId).FirstOrDefaultAsync();
                    if (employee != null)
                        employes.Add(employee);
                }
          
                i.Employee = employee;
                i.HistoryEntry = historyItem;
                i.Device = device;

                result.Add(i);
            }

            History = result.OrderByDescending(x => x.HistoryEntry.Date).ToList();
        }

        protected override void InitializeCommands()
        {
            GoBackCommand = new DelegateCommand(() =>
            {
                NavigationService.GoBack();
            });
        }
    }
}
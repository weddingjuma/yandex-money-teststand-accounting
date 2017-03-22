using SQLite;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TestStand.Model;
using TestStand.Services;
using TestStand.View;
using Windows.Storage;
using Windows.UI.Xaml.Navigation;
using Jupiter.Application;
using Jupiter.Mvvm;

namespace TestStand.ViewModel
{
    public class DeviceHistoryViewModel : ViewModelBase
    {
        private Device _device;
        private Employee _employee;
        private List<HistoryDeviceEmployeeEntry> _history;

        #region Commands

        /// <summary>
        /// Переходим на предыдущую страницу
        /// </summary>
        public DelegateCommand GoBackCommand { get; private set; }

        #endregion

        /// <summary>
        /// Ссылка на фото сотрудника
        /// </summary>

        public Device Device
        {
            get { return _device; }
            set
            {
                Set(ref _device, value);
            }
        }

        public Employee Employee
        {
            get { return _employee; }
            set
            {
                Set(ref _employee, value);
            }
        }

        public List<HistoryDeviceEmployeeEntry> History
        {
            get { return _history; }
            set { Set(ref _history, value); }
        }

        public override void OnNavigatedTo(Dictionary<string, object> parameters, NavigationMode mode)
        {
            Device = (Device)parameters["Device"];

            LoadData();

            LogOutService.Restart();
        }

        private async void LoadData()
        {
            var historyService = Ioc.Resolve<HistoryService>();
            var history = await historyService.GetHistoryByDevice(Device.Id);
            var employes = new List<Employee>();
            var result = new List<HistoryDeviceEmployeeEntry>();

            string сonnectionString = Path.Combine(ApplicationData.Current.LocalFolder.Path, "TestStand.sqlite");
            var сonnection = new SQLiteAsyncConnection(сonnectionString);

            foreach (var historyItem in history)
            {
                var i = new HistoryDeviceEmployeeEntry();

                var employee = employes.FirstOrDefault(e => e.BadgeId == historyItem.BadgeId);
                if (employee == null)
                {
                    employee = await сonnection.Table<Employee>().Where(r => r.BadgeId == historyItem.BadgeId).FirstOrDefaultAsync();
                    if (employee != null)
                        employes.Add(employee);
                }

                i.Employee = employee;
                i.HistoryEntry = historyItem;

                result.Add(i);
            }
            History = result.ToList();
            History.Reverse();

            if (!string.IsNullOrEmpty(Device.BadgeId))
            {
                Employee employeeWithThisDevice = await Ioc.Resolve<EmployeeService>().GetEmployeeByBadgeId(Device.BadgeId);
            }
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
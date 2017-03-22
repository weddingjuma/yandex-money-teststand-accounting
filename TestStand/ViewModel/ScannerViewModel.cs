using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.UI.Core;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using TestStand.Utils.Barcode;
using Jupiter.Mvvm;
using Jupiter.Services.Navigation;
using ZXing;
using TestStand.Model;
using TestStand.Services;
using System.Linq;
using Jupiter.Utils.Helpers;
using System.Collections.ObjectModel;
using TestStand.Controls;

namespace TestStand.ViewModel
{
    public enum ScanningState
    {
        Waiting,
        Scanning,
        ReturnDevice,
        TakeDevice,
        DeviceInBucket,
        Error
    }

    public class ScannerViewModel : ViewModelBase
    {
        private QrCodeScanner _qrScanner;
        private ScanningState _scanningState;
        private string _scanStatus = "Наведите камеру на QR-код";

        private DeviceService _deviceService = Ioc.Resolve<DeviceService>();

        private List<Device> _devicesOnEmployee;
        private string _employeePhotoSource;

        private string _actionButtonTitle = "Завершить";
        private ActionFeedbackButtonState _actionButtonState;
        private bool _isActionButtonEnabled = true;

        private Employee _employee;

        private ObservableCollection<Device> _devicesToTake = new ObservableCollection<Device>();

        private ObservableCollection<Device> _devicesToReturn = new ObservableCollection<Device>();

        private ObservableCollection<DeviceGroup> _devicesGroups = new ObservableCollection<DeviceGroup>();


        #region Commands

        /// <summary>
        /// Действие при нажатии на кнопку (добавить/вернуть/звершить)
        /// </summary>
        public DelegateCommand ActionWithDevicesCommand { get; private set; }

        /// <summary>
        /// Удалить девайс из списка отсканированных
        /// </summary>
        public DelegateCommand<Device> DeleteDeviceCommand { get; private set; }

        #endregion

        /// <summary>
        /// Сотрудник
        /// </summary>
        public Employee Employee
        {
            get { return _employee; }
            set
            {
                Set(ref _employee, value);
            }
        }

        /// <summary>
        /// Текст на кнопке
        /// </summary>
        public string ActionButtonTitle
        {
            get { return _actionButtonTitle; }
            set
            {
                Set(ref _actionButtonTitle, value);
            }
        }

        /// <summary>
        /// Состояние кнопки
        /// </summary>
        public ActionFeedbackButtonState ActionButtonState
        {
            get { return _actionButtonState; }
            set
            {
                Set(ref _actionButtonState, value);
            }
        }

        /// <summary>
        /// Активна ли кнопка
        /// </summary>
        public bool IsActionButtonEnabled
        {
            get { return _isActionButtonEnabled; }
            set
            {
                Set(ref _isActionButtonEnabled, value);
            }
        }

        /// <summary>
        /// Устройства на сотруднике
        /// </summary>
        public List<Device> DevicesOnEmployee
        {
            get { return _devicesOnEmployee; }
            set { Set(ref _devicesOnEmployee, value); }
        }

        /// <summary>
        /// Ссылка на фото сотрудника
        /// </summary>
        public string EmployeePhotoSource
        {
            get { return _employeePhotoSource; }
            set
            {
                Set(ref _employeePhotoSource, value);
            }
        }

        /// <summary>
        /// Группы отсканированных устройств (берем/отдаем)
        /// </summary>
        public ObservableCollection<DeviceGroup> DevicesGroups
        {
            get { return _devicesGroups; }
            set
            {
                _devicesGroups = value;
                RaisePropertyChanged();
            }
        }

        /// <summary>
        /// Состояние процесса сканирования
        /// </summary>
        public ScanningState ScanningState
        {
            get { return _scanningState; }
            private set
            {
                if (!Set(ref _scanningState, value))
                    return;

                switch (_scanningState)
                {
                    case ScanningState.Waiting:
                        ScanStatus = "Наведите камеру на QR-код";
                        break;

                    case ScanningState.Scanning:
                        ScanStatus = "Наведите камеру на QR-код";
                        break;

                    case ScanningState.Error:
                        _qrScanner?.StopScanning();
                        ScanStatus = "Не удалось распознать устройство";
                        break;

                    case ScanningState.DeviceInBucket:
                        _qrScanner?.StopScanning();
                        ScanStatus = "Устройство добавлено";
                        break;
                }
            }
        }

        /// <summary>
        /// Текстовый статус сканирования
        /// </summary>
        public string ScanStatus
        {
            get { return _scanStatus; }
            private set { Set(ref _scanStatus, value); }
        }

        public ScannerViewModel()
        {
            RegisterTasks("scanning");
        }

        public async void SetCaptureElement(CaptureElement captureElement)
        {
            if (_qrScanner != null)
                return;

            _qrScanner = new QrCodeScanner(captureElement);
            _qrScanner.CodeScanned += CodeScanned;
            await _qrScanner.StartAsync();
        }

        public override void OnNavigatedTo(Dictionary<string, object> parameters, NavigationMode mode)
        {
            if (_qrScanner != null && !_qrScanner.IsStarted)
                _qrScanner.StartAsync();

            Employee = (Employee)parameters["Employee"];

            if (mode == NavigationMode.Back)
            {
                if (SessionState.ContainsKey("DevicesToTake"))
                    _devicesToTake = (ObservableCollection<Device>)SessionState["DevicesToTake"];
                if (SessionState.ContainsKey("DevicesToReturn"))
                    _devicesToReturn = (ObservableCollection<Device>)SessionState["DevicesToReturn"];

                UpdateDeviceGroups();
            }

            LoadData();

            LogOutService.Restart();
        }

        public override async void OnNavigatingFrom(NavigatingEventArgs e)
        {
            await _qrScanner.StopAsync();

            if (e.NavigationMode == NavigationMode.Back)
            {
                //clear state
                SessionState.Remove("DevicesToTake");
                SessionState.Remove("DevicesToReturn");
            }
            else
            {
                SessionState["DevicesToTake"] = _devicesToTake;
                SessionState["DevicesToReturn"] = _devicesToReturn;
            }

            base.OnNavigatingFrom(e);
        }

        protected override void InitializeCommands()
        {
            DeleteDeviceCommand = new DelegateCommand<Device>(device =>
            {
                if (_devicesToTake.Contains(device))
                    _devicesToTake.Remove(device);
                else if (_devicesToReturn.Contains(device))
                    _devicesToReturn.Remove(device);

                UpdateDeviceGroups();
            });

            ActionWithDevicesCommand = new DelegateCommand(async () =>
            {
                IsActionButtonEnabled = false;


                try
                {
                    if (_devicesToTake.Any() || _devicesToReturn.Any())
                    {

                        ActionButtonState = ActionFeedbackButtonState.InProgress;

                        //пытаемся взять и вернуть девайсы

                        foreach (Device device in _devicesToTake)
                            await _deviceService.TakeDeviceAsync(device.Id, _employee.BadgeId);

                        foreach (Device device in _devicesToReturn)
                            await _deviceService.ReturnDeviceAsync(device.Id, _employee.BadgeId);

                        await BackupDbService.BackupDbAsync();

                        //если все ок, показывает анимацию юзеру
                        ActionButtonState = ActionFeedbackButtonState.Success;

                        await Task.Delay(1000);

                        //возвращаемся на главную
                        while (NavigationService.Frame.BackStack.Count > 1)
                        {
                            NavigationService.RemoveBackEntry();
                        }
                    }

                    //возвращаемся на главную
                    while (NavigationService.Frame.BackStack.Count > 1)
                    {
                        NavigationService.RemoveBackEntry();
                    }

                    NavigationService.GoBack();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex);

                    //если ошибка, показывает анимацию юзеру
                    ActionButtonState = ActionFeedbackButtonState.Error;

                    await Task.Delay(3000);

                    //возвращаемся в нормальное состояние
                    ActionButtonState = ActionFeedbackButtonState.Normal;
                }
                finally
                {
                    IsActionButtonEnabled = true;
                }
            });
        }

        private async void LoadData()
        {
            var historyService = Ioc.Resolve<HistoryService>();
            DevicesOnEmployee = await historyService.GetHistoryByEmployee(Employee.BadgeId);
        }


        private async void CodeScanned(object sender, Result result)
        {
            Debug.WriteLine("Scanned: " + result.Text);

            await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                RecognizeAsync(result.Text);
            });
        }

        private async void RecognizeAsync(string qrCodeContent)
        {
            LogOutService.Restart();

            if (ScanningState == ScanningState.Scanning)
                return;

            try
            {
                UpdateScanningState(ScanningState.Scanning);

                TaskStarted("scanning");

                int id;

                if (!int.TryParse(qrCodeContent, out id))
                {
                    UpdateScanningState(ScanningState.Error, "Не удалось распознать устройство");
                    await Task.Delay(3000);
                    UpdateScanningState(ScanningState.Waiting);
                    _qrScanner.StartScanning();
                    return;
                }

                if (!_devicesToTake.Any(x => x.Id == id) && !_devicesToReturn.Any(x => x.Id == id))
                {
                    var device = await _deviceService.GetDeviceByIdAsync(id);

                    if (string.IsNullOrEmpty(device.BadgeId))
                    {
                        _devicesToTake.Add(device);
                    }
                    else
                    {
                        _devicesToReturn.Add(device);
                    }

                    UpdateDeviceGroups();

                    UpdateScanningState(ScanningState.DeviceInBucket);

                    await Task.Delay(1000);
                }
                else
                {
                    UpdateScanningState(ScanningState.Error, "Устройство уже в списке");
                    await Task.Delay(3000);
                }

                UpdateScanningState(ScanningState.Waiting);
                _qrScanner.StartScanning();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);

                UpdateScanningState(ScanningState.Error, "Не удалось отсканировать устройство");
                await Task.Delay(3000);
                UpdateScanningState(ScanningState.Scanning);
                _qrScanner.StartScanning();
            }
            finally
            {
                TaskFinished("scanning");
            }
        }

        private void UpdateScanningState(ScanningState state, string message = null)
        {
            ScanningState = state;

            if (!string.IsNullOrEmpty(message))
                ScanStatus = message;
        }

        private void UpdateDeviceGroups()
        {
            int beforeСount = _devicesGroups.Count;

            if (_devicesToTake.Count > 0)
            {
                if (!_devicesGroups.Any(g => g.Items == _devicesToTake))
                    _devicesGroups.Insert(0, new DeviceGroup { Title = "Вы берете", Items = _devicesToTake });
            }
            else
            {
                var takeGroup = _devicesGroups.FirstOrDefault(g => g.Items == _devicesToTake);
                if (takeGroup != null)
                    _devicesGroups.Remove(takeGroup);
            }

            if (_devicesToReturn.Count > 0)
            {
                if (!_devicesGroups.Any(g => g.Items == _devicesToReturn))
                    _devicesGroups.Add(new DeviceGroup { Title = "Вы отдаете", Items = _devicesToReturn });
            }
            else
            {
                var returnGroup = _devicesGroups.FirstOrDefault(g => g.Items == _devicesToReturn);
                if (returnGroup != null)
                    _devicesGroups.Remove(returnGroup);
            }

            int afterСount = _devicesGroups.Count;

            //если до изменений не было групп, но они появились, либо наоборот, уведомляем UI, что группы изменились
            //чтобы все элементы, привязанные к этому свойству, корректно обновились
            if (beforeСount == 0 && afterСount != 0 || beforeСount != 0 && afterСount == 0)
                RaisePropertyChanged(nameof(DevicesGroups));

            //текст на кнопке
            if (_devicesToTake.Count + _devicesToReturn.Count > 0)
            {
                var actions = new List<string>();
                if (_devicesToTake.Count > 0)
                    actions.Add("ВЗЯТЬ " + _devicesToTake.Count);
                if (_devicesToReturn.Count > 0)
                    actions.Add("ОТДАТЬ " + _devicesToReturn.Count);

                var localizeCount = DevicesGroups.Last().Items.Count; //берем последнюю группу в списке и смотрим на кол-во девайсов в ней, тогда фраза типа "ВЗЯТЬ 3, ОТДАТЬ 1 УСТРОЙСТВО" будет выглядеть корректно

                ActionButtonTitle = string.Join(", ", actions) + " " + StringHelper.LocalizeNumerals(localizeCount, "УСТРОЙСТВО", "УСТРОЙСТВА", "УСТРОЙСТВ");
            }
            else
            {
                ActionButtonTitle = "ЗАВЕРШИТЬ";
            }
        }
    }
}

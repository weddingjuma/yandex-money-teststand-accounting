using System.Collections.Generic;
using System.Threading.Tasks;
using SQLite;
using TestStand.Enum;
using TestStand.Model;
using System;

namespace TestStand.Services
{
    public class DeviceService
    {
        private readonly SQLiteAsyncConnection _db;

        public DeviceService(SQLiteAsyncConnection connection)
        {
            _db = connection;
        }

        /// <summary>
        /// Получить девайс по id
        /// </summary>
        public async Task<Device> GetDeviceByIdAsync(int id)
        {
            return await _db.Table<Device>().Where(r => r.Id == id).FirstOrDefaultAsync();
        }

        /// <summary>
        /// Получить все устройства
        /// </summary>
        /// <returns></returns>
        public async Task<List<Device>> GetAllDevicesAsync()
        {
            return await _db.Table<Device>().ToListAsync();
        }

        /// <summary>
        /// Получить устройства с сотрудниками
        /// </summary>
        public async Task<List<DeviceEmployeeEntry>> GetDevicesWithEmployeesAsync()
        {            
            var devices = await GetAllDevicesAsync();
            var entries = new List<DeviceEmployeeEntry>();

            foreach (var device in devices)
            {
                var employee = await _db.Table<Employee>().Where(e => e.BadgeId == device.BadgeId).FirstOrDefaultAsync();
                entries.Add(new DeviceEmployeeEntry() { Device = device, Employee = employee });
            }

            return entries;
        }

        /// <summary>
        /// Забрать девайс
        /// </summary>
        /// <param name="deviceId">Id девайса</param>
        /// <param name="employeeBadgeId">Бейдж сотрудника</param>
        public async Task<bool> TakeDeviceAsync(int deviceId, string employeeBadgeId)
        {
            EmployeeService employeeService = Ioc.Resolve<EmployeeService>();

            var device = await GetDeviceByIdAsync(deviceId);
            var employee = await employeeService.GetEmployeeByBadgeId(employeeBadgeId);

            if (!string.IsNullOrEmpty(device.BadgeId)) //девайс уже кто-то взял
                return false;
            else
            {
                device.BadgeId = employeeBadgeId;
                device.EmployeeName = employee.FirstName;
                device.TakenDate = DateTime.Now;

                await _db.UpdateAsync(device);

                await Ioc.Resolve<HistoryService>().AddEntry(device, employeeBadgeId, DeviceAction.Take);

                return true;
            }
        }

        /// <summary>
        /// Вернуть девайс
        /// </summary>
        /// <param name="deviceId">Id устройства</param>
        /// <param name="employeeBadgeId">Бейдж сотрудника</param>
        public async Task<bool> ReturnDeviceAsync(int deviceId, string employeeBadgeId)
        {
            var device = await GetDeviceByIdAsync(deviceId);
            if (string.IsNullOrEmpty(device.BadgeId)) //девайс никто не брал
                return false;
            else
            {
                device.BadgeId = null;
                device.TakenDate = null;
                device.EmployeeName = null;
                await _db.UpdateAsync(device);

                await Ioc.Resolve<HistoryService>().AddEntry(device, employeeBadgeId, DeviceAction.Return);

                return true;
            }
        }

        /// <summary>
        /// Проверка на ошибочное действие. Если пользователь хочет сдать девайс, который уже сдан. Или взять девайс, который уже взят.
        /// </summary>
        /// <param name="deviceId">Id устройства</param>
        /// <param name="action">действие, которое хотят совершить с ус-ом</param>
        public async Task<bool> GetDeviceStatusAsync(int deviceId, string badgeId)
        {
            var device = await GetDeviceByIdAsync(deviceId);
            if (device.BadgeId == badgeId)
                return true;
            else
                return false;
        }

        /// <summary>
        /// Получить девайс по id
        /// </summary>
        public async Task<List<Device>> SearchDevicesAsync(string query)
        {
            query = query.ToLower();

            return await _db.Table<Device>().Where(d => d.Model.Contains(query)).ToListAsync();
        }

    }
}
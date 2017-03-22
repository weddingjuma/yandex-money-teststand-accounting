using Newtonsoft.Json.Linq;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestStand.Model;
using Windows.ApplicationModel.AppExtensions;
using Windows.ApplicationModel.AppService;
using Windows.Foundation.Collections;

namespace TestStand.Services
{
    public class EmployeeService
    {
        private readonly SQLiteAsyncConnection _db;

        public EmployeeService(SQLiteAsyncConnection connection)
        {
            _db = connection;
        }

        public async Task<List<Employee>> GetAllEmployes()
        {
            List<Employee> all = await _db.Table<Employee>().ToListAsync();

            return await _db.Table<Employee>().ToListAsync();
        }

        public async Task<Employee> GetEmployeeByBadgeId(string badgeId)
        {
            return await _db.Table<Employee>().Where(e => e.BadgeId == badgeId).FirstOrDefaultAsync();
        }

        public async Task<Employee> AddEmployeeInDbByBadgeId(string badgeId)
        {
            DateTime date = DateTime.Now;

            Employee employee = await GetEmployeeInfoByBadgeId(badgeId);
            employee.Date = date;

            if (!string.IsNullOrEmpty(employee.Login))
            {
                await _db.InsertAsync(employee);
                return employee;
            }
            else
                return null;
        }

        private async Task<Employee> GetEmployeeInfoByBadgeId(string badgeId)
        {
            ExtensionsService extensionsService = Ioc.Resolve<ExtensionsService>();

            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("BadgeId", badgeId);

            var extensionResponse = await extensionsService.InvokeExtension("com.extensions.yamoney.teststand", parameters);

            var response = JObject.Parse((string)extensionResponse["Response"]); // Получаем результат

            Employee employee = new Employee();

            employee.FirstName = response["guestfio"].Value<string>();
            employee.Login = response["Login"].Value<string>();
            employee.Photo = response["Photo"].Value<string>();
            employee.BadgeId = badgeId;

            return employee;
        }
    }
}

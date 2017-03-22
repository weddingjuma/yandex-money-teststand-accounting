using Newtonsoft.Json.Linq;
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
    public class ExtensionsService
    {
        public async Task<Dictionary<string, object>> InvokeExtension(string extensionId, Dictionary<string, object> parameters)
        {
            Employee employee = new Employee();
            //"com.extensions.yamoney.teststand"
            AppExtensionCatalog catalog = AppExtensionCatalog.Open(extensionId);
            var extensions = new System.Collections.Generic.List<AppExtension>(await catalog.FindAllAsync());

            var employeeService = extensions[0];
            var packageFamilyName = employeeService.Package.Id.FamilyName;

            IPropertySet properties = await employeeService.GetExtensionPropertiesAsync();
            PropertySet serviceProperty = (PropertySet)properties["Service"];
            var serviceName = serviceProperty["#text"].ToString();

            AppServiceConnection connection = new AppServiceConnection
            {
                AppServiceName = serviceName,
                PackageFamilyName = packageFamilyName
            }; // Параметры подключения


            var message = new ValueSet(); // Параметры для передачи

            foreach (var kv in parameters)
            {
                message.Add(kv);
            }

            var status = await connection.OpenAsync(); // Открываем подключение
            using (connection)
            {
                if (status != AppServiceConnectionStatus.Success) // Проверяем статус
                {
                    return null;
                }

                var response = await connection.SendMessageAsync(message); // Отправляем сообщение и ждем ответа
                if (response.Status == AppServiceResponseStatus.Success)
                {
                    return response.Message.ToDictionary(kv => kv.Key, kv => kv.Value);
                }
                else
                    return null;              
            }
        }
    }
}

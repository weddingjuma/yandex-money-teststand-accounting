using LightBuzz.SMTP;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TestStand.Model;
using Windows.ApplicationModel.Email;
using Windows.UI.Xaml;

namespace TestStand.Services
{
    public class EmailNotificationService
    {
        private static async Task SendEmail(string device, string login, string name)
        {
            using (SmtpClient client = new SmtpClient("mail.yamoney.ru", 25, false, "Stand_of_testing_devices", ""))
            {
                EmailMessage emailMessage = new EmailMessage();

                emailMessage.To.Add(new EmailRecipient(login + "@yamoney.ru"));
                emailMessage.CC.Add(new EmailRecipient("sakustov@yamoney.ru, natashkin@yamoney.ru"));
                //emailMessage.Bcc.Add(new EmailRecipient("someone3@anotherdomain.com"));
                emailMessage.Subject = "ВНИМАНИЕ! Превышен максимальный срок использования " + device;
                emailMessage.Body = name + ", необходимо вернуть " + device + " на стенд тестовых устройств в кабинет 604.1.";

                await client.SendMailAsync(emailMessage);
            }
        }

        private static async Task<List<Device>> CreateEmailList()
        {
            List<Device> emailList = new List<Device>();

            var deviceService = Ioc.Resolve<DeviceService>();
            List<Device> devices = await deviceService.GetAllDevicesAsync();


            foreach (Device device in devices)
            {
                if (device.BadgeId != null && DateTime.Now > device.TakenDate?.AddDays(3))
                {
                    emailList.Add(device);
                }
            }
            return emailList;
        }

        private static DispatcherTimer _timer = new DispatcherTimer();

        static EmailNotificationService()
        {
            _timer.Tick += TimerTick;
            _timer.Interval = TimeSpan.FromHours(1);
        }

        public static void Start()
        {
            _timer.Start();
        }

        public static void StopTimer()
        {
            _timer.Stop();
        }
        public static void RestartTimer()
        {
            StopTimer();
            Start();
        }

        private static async void TimerTick(object sender, object e)
        {
            if (DateTime.Now.Hour == 11)
            {
                _timer.Stop();

                var employeeService = Ioc.Resolve<EmployeeService>();
                List<Device> emailList = await CreateEmailList();

                foreach (Device device in emailList)
                {
                    Employee employee = await employeeService.GetEmployeeByBadgeId(device.BadgeId);
                    await SendEmail(device.Model, employee.Login, employee.FirstName);
                }

                _timer.Start();
            }
        }
    }
}

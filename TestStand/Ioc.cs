using System.IO;
using Windows.Storage;
using GalaSoft.MvvmLight.Ioc;
using SQLite;
using TestStand.Services;

namespace TestStand
{
    public class Ioc
    {
        private static readonly string DbPath = Path.Combine(ApplicationData.Current.LocalFolder.Path, "TestStand.sqlite");

        public static void Setup()
        {
            var db = new SQLiteAsyncConnection(DbPath);

            SimpleIoc.Default.Register(() => new DeviceService(db));
            SimpleIoc.Default.Register(() => new HistoryService(db));
            SimpleIoc.Default.Register(() => new EmployeeService(db));
            SimpleIoc.Default.Register(() => new ExtensionsService());

            SimpleIoc.Default.Register(() => new BingImageService());
        }

        public static T Resolve<T>()
        {
            return SimpleIoc.Default.GetInstance<T>();
        }
    }
}

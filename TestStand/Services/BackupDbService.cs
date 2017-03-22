using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Xaml;

namespace TestStand.Services
{
    public class BackupDbService
    {
        private const string BACKUP_FOLDER_NAME = "DbBackup";

        private static DispatcherTimer _timer;

        public static async Task BackupDbAsync()
        {
            string dbName = "TestStand.sqlite";
            string dateTime = DateTime.Now.ToString("yyyy-MM-dd hh.mm.ss.ff");
            StorageFolder localAppFolder = ApplicationData.Current.LocalFolder;

            var backupFolder = await KnownFolders.PicturesLibrary.TryGetItemAsync(BACKUP_FOLDER_NAME) as StorageFolder;
            if (backupFolder == null)
            {
                backupFolder = await KnownFolders.PicturesLibrary.CreateFolderAsync(BACKUP_FOLDER_NAME);
            }     

            StorageFile dbFile = await localAppFolder.GetFileAsync(dbName);
            StorageFile copiedFile = await dbFile.CopyAsync(backupFolder, dateTime + "_" + dbName, NameCollisionOption.ReplaceExisting);
        }

        public static void StartBackupTimer()
        {
            _timer = new DispatcherTimer();
            _timer.Tick += TimerTick;
            _timer.Interval = TimeSpan.FromMinutes(90);
            _timer.Start();
        }

        private static async void TimerTick(object sender, object e)
        {
            await BackupDbAsync();
        }
    }
}

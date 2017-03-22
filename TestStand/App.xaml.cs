using System;
using System.Threading.Tasks;
using TestStand.Services;
using TestStand.View;
using Windows.ApplicationModel.Activation;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media.Animation;
using Jupiter.Application;
using Jupiter.Services.Navigation;
using Windows.ApplicationModel.AppExtensions;
using Windows.Foundation.Collections;
using Windows.ApplicationModel.AppService;
using TestStand.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace TestStand
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    sealed partial class App : JupiterApp
    {
        public App()
        {
            Ioc.Setup();
        }

        public override async void OnStart(StartKind startKind, IActivatedEventArgs args)
        {
            NavigationService.Frame.ContentTransitions = new TransitionCollection()
            {
                new NavigationThemeTransition()
                {
                    DefaultNavigationTransitionInfo = new DrillInNavigationTransitionInfo()
                }
            };


            //Копирование БД
            string dbName = "TestStand.sqlite";
            StorageFolder localAppFolder = ApplicationData.Current.LocalFolder;

            if (await ApplicationData.Current.LocalFolder.TryGetItemAsync(dbName) == null)
            {
                StorageFile dbFile = await Windows.ApplicationModel.Package.Current.InstalledLocation.GetFileAsync(dbName);
                StorageFile copiedFile = await dbFile.CopyAsync(localAppFolder);
            }

            if (string.IsNullOrEmpty(Settings.Token))
            {
                await LoginAsync();
                if (string.IsNullOrEmpty(Settings.Token))
                {
                    Exit();
                    return;
                }
            }

            EmailNotificationService.Start();

            NavigationService.Navigate(typeof(MainView));
        }

        public override void OnInitialize(IActivatedEventArgs args)
        {
            // content may already be shell when resuming

            if (Window.Current.Content is Shell)
            {
                base.OnInitialize(args);
                return;
            }

            // setup shell

            var shell = new Shell();
            var navigationService = new NavigationService(shell.ContentFrame);
            navigationService.IsBackButtonEnabled = false; //отключаем навигацию

            WindowWrapper.Current().NavigationService = navigationService;

            Window.Current.Content = shell;
        }

        private async Task LoginAsync()
        {
            //var authUrl = new Uri("https://oauth.yandex-team.ru/authorize?response_type=token&client_id=4f36928e110443bc930a6f7fe1651951");
            //var result = await WebAuthenticationBroker.AuthenticateAsync(WebAuthenticationOptions.None, authUrl, new Uri("https://oauth.yandex-team.ru/verification_code"));
            //if (result.ResponseStatus == WebAuthenticationStatus.Success)
            //{
            //    var parameters = result.ResponseData.ParseQueryString();
            //    Settings.Token = parameters["access_token"];
            //}
            Settings.Token = "AQAD-qJSJrMDAAAGr8koHFdF_U8MqOs7fhk3VxA";
        }
    }
}

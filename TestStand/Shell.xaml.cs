using System;
using System.Diagnostics;
using TestStand.Services;
using Windows.ApplicationModel.AppExtensions;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace TestStand
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Shell : Page
    {
        public Shell()
        {
            this.InitializeComponent();
        }

        private void Page_Loaded(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            LoadBingImage();
        }

        private async void LoadBingImage()
        {
            var bingService = Ioc.Resolve<BingImageService>();
            try
            {
                var url = await bingService.GetPictureOfDayAsync();
                if (!string.IsNullOrEmpty(url))
                    BingImage.Source = new BitmapImage(new Uri(url));
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }
    }
}

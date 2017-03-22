using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Media.Animation;
using Jupiter.Mvvm;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace TestStand.View
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class UserTimeoutView : UserControl
    {
        public UserTimeoutView()
        {
            this.InitializeComponent();
        }

        private void rootElement_Loaded(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            var vm = DataContext as ViewModelBase;
            if (vm != null)
                vm.OnNavigatedTo(null, Windows.UI.Xaml.Navigation.NavigationMode.New);

            Controls.Popup.Current.Closing += Current_Closing;

            var loadAnim = (Storyboard)Resources["LoadAnim"];
            loadAnim.Begin();
        }

        private void Current_Closing()
        {
            var vm = DataContext as ViewModelBase;
            if (vm != null)
                vm.OnNavigatingFrom(null);

            var unloadAnim = (Storyboard)Resources["UnloadAnim"];
            unloadAnim.Begin();
        }
    }
}

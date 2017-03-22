using System;
using System.Numerics;
using TestStand.Controls;
using Windows.UI.Composition;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Hosting;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace TestStand.View
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SearchView : UserControl
    {
        private Compositor _compositor;
        private Visual _visual;

        public SearchView()
        {
            this.InitializeComponent();

            _visual = ElementCompositionPreview.GetElementVisual(this);
            _compositor = _visual.Compositor;
        }

        private void Page_Loaded(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            Popup.Current.Closing += Current_Closing;

            LoadAnim();
        }

        private void Current_Closing()
        {
            if (Popup.Current != null)
                Popup.Current.Closing -= Current_Closing;

            UnloadAnim();
        }

        #region Composition stuff

        private void LoadAnim()
        {
            var bgAnim = _compositor.CreateVector3KeyFrameAnimation();
            bgAnim.InsertKeyFrame(0, new Vector3(0, (float)this.SearchGrid.ActualHeight, 0));
            bgAnim.InsertKeyFrame(1f, new Vector3(0, 0, 0));
            bgAnim.Duration = TimeSpan.FromMilliseconds(700);

            _visual.StartAnimation(nameof(Visual.Offset), bgAnim);

            SearchBox.Focus(Windows.UI.Xaml.FocusState.Keyboard);
        }

        private void UnloadAnim()
        {
            var closeAnim = _compositor.CreateVector3KeyFrameAnimation();
            closeAnim.InsertKeyFrame(1f, new Vector3(0, (float)this.SearchGrid.ActualHeight, 0));
            closeAnim.Duration = TimeSpan.FromMilliseconds(500);

            _visual.StartAnimation(nameof(Visual.Offset), closeAnim);
        }

        #endregion
    }
}
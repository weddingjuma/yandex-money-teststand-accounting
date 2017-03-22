using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Composition;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Hosting;
using Jupiter.Utils.Extensions;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace TestStand.Controls
{
    public sealed partial class Popup : UserControl
    {
        private Compositor _compositor;
        private Visual _bgOverlayVisual;


        public static readonly DependencyProperty PopupContentProperty = DependencyProperty.Register(
                        "PopupContent", typeof(object), typeof(Popup), new PropertyMetadata(default(object)));

        public object PopupContent
        {
            get { return (object)GetValue(PopupContentProperty); }
            set { SetValue(PopupContentProperty, value); }
        }

        public delegate void ClosedEventHandler();

        public event ClosedEventHandler Closing;
        public event ClosedEventHandler Closed;

        public static Popup Current { get; private set; }

        public Popup()
        {
            this.InitializeComponent();

            _compositor = ElementCompositionPreview.GetElementVisual(this).Compositor;
        }

        public static void Show(object content)
        {
            var rootGrid = (Window.Current.Content as FrameworkElement)?.GetVisualDescendents().OfType<Grid>().FirstOrDefault();
            if (rootGrid == null)
                return;

            var popup = new Popup();
            Current = popup;
            Grid.SetRowSpan(popup, int.MaxValue);
            Grid.SetColumnSpan(popup, int.MaxValue);

            popup.PopupContent = content;
            rootGrid.Children.Add(popup);
        }

        public void CloseNow()
        {
            if (this.Parent != null)
                ((Grid)this.Parent).Children.Remove(this);

            Closed?.Invoke();
            Current = null;
        }

        public void Close()
        {
            UnloadAnim();
        }

        private void rootElement_Loaded(object sender, RoutedEventArgs e)
        {
            _bgOverlayVisual = ElementCompositionPreview.GetElementVisual(BgOverlay);

            LoadAnim();
        }

        private void BgOverlay_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            Close();
        }

        #region Composition stuff

        private void LoadAnim()
        {
            var fadeAnim = _compositor.CreateScalarKeyFrameAnimation();
            fadeAnim.InsertKeyFrame(0f, 0f);
            fadeAnim.InsertKeyFrame(1f, 1f);
            fadeAnim.Duration = TimeSpan.FromMilliseconds(900);

            _bgOverlayVisual.StartAnimation(nameof(Visual.Opacity), fadeAnim);
        }

        private void UnloadAnim()
        {
            Closing?.Invoke();

            var fadeAnim = _compositor.CreateScalarKeyFrameAnimation();
            fadeAnim.InsertKeyFrame(1f, 0);
            fadeAnim.Duration = TimeSpan.FromMilliseconds(900);

            var scopedBatch = _compositor.CreateScopedBatch(CompositionBatchTypes.Animation);
            scopedBatch.Completed += (s, e) => CloseNow();

            _bgOverlayVisual.StartAnimation(nameof(Visual.Opacity), fadeAnim);

            scopedBatch.End();
        }

        #endregion
    }
}
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Graphics.Display;
using Windows.Media;
using Windows.Media.Capture;
using Windows.Media.Devices;
using Windows.Media.MediaProperties;
using Windows.System.Display;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;
using ZXing;

namespace TestStand.Utils.Barcode
{
    /// <summary>
    /// Сканер QR кодов
    /// </summary>
    public class QrCodeScanner
    {
        private static readonly int DelayBetweenAnalyzingFrames = 150;
        private static readonly int InitialDelayBeforeAnalyzingFrames = 300;
        private static readonly int DelayBetweenContinuousScans = 1000;

        // Rotation metadata to apply to the preview stream (MF_MT_VIDEO_ROTATION)
        // Reference: http://msdn.microsoft.com/en-us/library/windows/apps/xaml/hh868174.aspx
        private static readonly Guid RotationKey = new Guid("C380465D-2271-428C-9B83-ECEA3B4A85C1");

        // Receive notifications about rotation of the UI and apply any necessary rotation to the preview stream
        private readonly DisplayInformation _displayInformation = DisplayInformation.GetForCurrentView();

        private DisplayRequest _displayRequest;

        private MediaCapture _capture;

        private readonly CaptureElement _captureElement;

        private readonly BarcodeReader _barcodeReader = new BarcodeReader
        {
            Options =
                {
                    TryHarder = true
                }
        };

        private Timer _scanningTimer;

        private readonly CoreDispatcher _dispatcher;

        private bool _isProcessingFrame;

        private WriteableBitmap _bitmap;

        private CancellationTokenSource _scanCancellationToken;

        /// <summary>
        /// Вызывается при успешном сканировании кода
        /// </summary>
        public event EventHandler<Result> CodeScanned;

        /// <summary>
        /// Инициализирована ли камера
        /// </summary>
        public bool IsInitialized { get; set; }

        /// <summary>
        /// Запущен ли сканер
        /// </summary>
        public bool IsStarted { get; set; }

        public QrCodeScanner(CaptureElement element) : this()
        {
            _captureElement = element;
        }

        public QrCodeScanner()
        {
            _dispatcher = Windows.ApplicationModel.Core.CoreApplication.GetCurrentView().CoreWindow.Dispatcher;
        }

        /// <summary>
        /// Фокусировка
        /// </summary>
        public async Task FocusAsync()
        {
            if (_captureElement == null)
                return;

            try
            {
                //Focusing
                await _capture.VideoDeviceController.FocusControl.FocusAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }

        /// <summary>
        /// Начать сканирование
        /// <param name="previewOnly">True - запустить только камеру, без сканирования</param>
        /// </summary>
        public async Task StartAsync(bool previewOnly = false, bool onlyFrontCamera = true)
        {
            _scanCancellationToken = new CancellationTokenSource();

            _displayInformation.OrientationChanged += DisplayInformationOrientationChanged;
            _displayRequest = new DisplayRequest();
            _displayRequest.RequestActive();

            _capture = new MediaCapture();

            var videodevices = await DeviceInformation.FindAllAsync(DeviceClass.VideoCapture);

            // ищем сначала заднюю камеру

            DeviceInformation camera = null;

            if (!onlyFrontCamera)
            {
                camera = videodevices.FirstOrDefault(item => item.EnclosureLocation != null && item.EnclosureLocation.Panel == Windows.Devices.Enumeration.Panel.Back);
            }

            //задней камеры может не быть (например на ПК)
            if (camera == null)
            {
                //ищем переднюю
                camera = videodevices.FirstOrDefault(item => item.EnclosureLocation != null && item.EnclosureLocation.Panel == Windows.Devices.Enumeration.Panel.Front);
            }

            if (camera == null)
            {
                //TODO камеры нет, показывать ошибку?
                return;
            }

            await _capture.InitializeAsync(new MediaCaptureInitializationSettings
            {
                StreamingCaptureMode = StreamingCaptureMode.Video,
                VideoDeviceId = camera.Id
            });

            if (_captureElement != null)
                _captureElement.Source = _capture;

            try
            {
                //проверяем, какие режимы фокусировки поддерживаются
                var focusModes = _capture.VideoDeviceController.FocusControl.SupportedFocusModes.ToList();
                if (focusModes.Any(m => m == FocusMode.Continuous))
                    _capture.VideoDeviceController.FocusControl.Configure(new FocusSettings() { Mode = FocusMode.Continuous });
                else if (focusModes.Any(m => m == FocusMode.Auto))
                    _capture.VideoDeviceController.FocusControl.Configure(new FocusSettings() { Mode = FocusMode.Auto });
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }

            await _capture.StartPreviewAsync();

            await SetPreviewRotationAsync();

            try
            {
                //Focusing
                if (_capture.VideoDeviceController.FocusControl.Supported)
                    await _capture.VideoDeviceController.FocusControl.FocusAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }

            IsInitialized = true;

            if (!previewOnly)
                StartScanning();
        }

        /// <summary>
        /// Остановить сканирование
        /// </summary>
        public async Task StopAsync()
        {
            if (!IsInitialized)
                return;

            IsInitialized = false;

            if (IsStarted)
                StopScanning();

            _displayInformation.OrientationChanged -= DisplayInformationOrientationChanged;

            _displayRequest.RequestRelease();
            _displayRequest = null;

            await _capture.StopPreviewAsync();
            _capture.Dispose();
            _capture = null;
        }

        /// <summary>
        /// Запустить сканирование
        /// </summary>
        public void StartScanning()
        {
            IsStarted = true;

            // Get our preview properties
            var previewProperties = _capture.VideoDeviceController.GetMediaStreamProperties(MediaStreamType.VideoPreview) as VideoEncodingProperties;

            _scanningTimer = new Timer(async (state) =>
            {
                if (_isProcessingFrame || _capture == null || _capture.CameraStreamState != CameraStreamState.Streaming)
                {
                    _scanningTimer?.Change(DelayBetweenAnalyzingFrames, Timeout.Infinite);
                    return;
                }

                var token = _scanCancellationToken.Token;

                var delay = DelayBetweenAnalyzingFrames;

                _isProcessingFrame = true;

                VideoFrame destFrame = null;
                VideoFrame frame = null;

                try
                {
                    // Setup a frame to use as the input settings
                    destFrame = new VideoFrame(Windows.Graphics.Imaging.BitmapPixelFormat.Bgra8, (int)previewProperties.Width, (int)previewProperties.Height);

                    // Get preview 
                    frame = await _capture.GetPreviewFrameAsync(destFrame);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("GetPreviewFrame Failed: {0}", ex);
                }

                if (token.IsCancellationRequested)
                    return;

                Result result = null;

                // Try decoding the image
                try
                {
                    if (frame != null)
                    {
                        await _dispatcher.RunAsync(CoreDispatcherPriority.Low, () =>
                        {
                            if (_bitmap == null)
                                _bitmap = new WriteableBitmap(frame.SoftwareBitmap.PixelWidth, frame.SoftwareBitmap.PixelHeight);

                            frame.SoftwareBitmap.CopyToBuffer(_bitmap.PixelBuffer);

                            result = _barcodeReader.Decode(_bitmap);

                            if (destFrame != null)
                            {
                                destFrame.Dispose();
                                destFrame = null;
                            }

                            frame.Dispose();
                            frame = null;
                        });
                    }

                }
                catch (Exception ex)
                {

                }

                if (token.IsCancellationRequested)
                    return;

                if (result != null)
                {
                    CodeScanned?.Invoke(this, result);

                    delay = DelayBetweenContinuousScans;
                }

                _isProcessingFrame = false;

                _scanningTimer?.Change(delay, Timeout.Infinite);

            }, null, InitialDelayBeforeAnalyzingFrames, Timeout.Infinite);
        }

        /// <summary>
        /// Остановить сканирование
        /// </summary>
        public void StopScanning()
        {
            IsStarted = false;

            _scanCancellationToken.Cancel();
            _scanCancellationToken = new CancellationTokenSource();

            _scanningTimer?.Dispose();
            _scanningTimer = null;

            _isProcessingFrame = false;
        }

        private async void DisplayInformationOrientationChanged(DisplayInformation sender, object args)
        {
            if (!IsStarted)
                return;

            _bitmap = null; //пересоздаем битмап при изменении ориентации экрана

            await SetPreviewRotationAsync();
        }

        private async Task SetPreviewRotationAsync()
        {
            if (_capture.CameraStreamState != CameraStreamState.Streaming)
                return;

            // Calculate which way and how far to rotate the preview
            int rotationDegrees = ConvertDisplayOrientationToDegrees(_displayInformation.CurrentOrientation);

            // Add rotation metadata to the preview stream to make sure the aspect ratio / dimensions match when rendering and getting preview frames
            var props = _capture.VideoDeviceController.GetMediaStreamProperties(MediaStreamType.VideoPreview);
            props.Properties.Add(RotationKey, rotationDegrees);
            await _capture.SetEncodingPropertiesAsync(MediaStreamType.VideoPreview, props, null);
        }

        private static int ConvertDisplayOrientationToDegrees(DisplayOrientations orientation)
        {
            switch (orientation)
            {
                case DisplayOrientations.Portrait:
                    return 90;
                case DisplayOrientations.LandscapeFlipped:
                    return 180;
                case DisplayOrientations.PortraitFlipped:
                    return 270;
                default:
                    return 0;
            }
        }
    }
}

using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

using Coding4Fun.Kinect.Wpf;

using Microsoft.Research.Kinect.Nui;

namespace Kinect.Demo.Camera
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region --------------------- Member Variables ---------------------

        // Kinect Runtime
        private Runtime kinectRuntime;

        #endregion

        #region --------------------- Constructor ---------------------

        /// <summary>
        /// Initializes a new instance of the MainWindow class.
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();

            SetupKinect();
        }

        #endregion

        #region --------------------- Private Methods ---------------------
        
        private void SetupKinect()
        {
            // Check to see if there are any Kinect devices connected.
            if (Runtime.Kinects.Count == 0)
            {
                MessageBox.Show("No Kinect connected");
            }
            else
            {
                // Use first Kinect.
                kinectRuntime = Runtime.Kinects[0];

                // Initialize to return both Color & Depth data.
                kinectRuntime.Initialize(RuntimeOptions.UseColor | RuntimeOptions.UseDepth);

                // Attach to the event to receive video frame data.
                kinectRuntime.VideoFrameReady += KinectRuntime_VideoFrameReady;

                // Attach to the event to receive the depth frame data.
                kinectRuntime.DepthFrameReady += KinectRuntime_DepthFrameReady;

                // Start capturing video by opening the stream.
                kinectRuntime.VideoStream.Open(ImageStreamType.Video, 2, ImageResolution.Resolution640x480, ImageType.Color);

                // Start capturing the depth data by opening the stream.
                kinectRuntime.DepthStream.Open(ImageStreamType.Depth, 2, ImageResolution.Resolution640x480, ImageType.Depth);

                kinectRuntime.NuiCamera.ElevationAngle = 0;
            }
        }
        
        #endregion

        #region  --------------------- Event Handlers ---------------------

        private void KinectRuntime_VideoFrameReady(object sender, ImageFrameReadyEventArgs e)
        {
             // Get the bitmap using Coding4Fun extension method. (Short Way)
            VideoImage.Source = e.ImageFrame.ToBitmapSource();
        }

        private void KinectRuntime_DepthFrameReady(object sender, ImageFrameReadyEventArgs e)
        {
            DepthImage.Source = e.ImageFrame.ToBitmapSource();
        }

        private void SetTiltButton_Click(object sender, RoutedEventArgs e)
        {
            kinectRuntime.NuiCamera.ElevationAngle = (int)Math.Round(KinectTiltSlider.Value, 0);
        }

        #endregion
    }
}

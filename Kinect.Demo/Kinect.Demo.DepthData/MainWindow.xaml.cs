using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using Coding4Fun.Kinect.Wpf;

using Microsoft.Research.Kinect.Nui;

namespace Kinect.Demo.DepthData
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
                kinectRuntime.Initialize(RuntimeOptions.UseDepthAndPlayerIndex);

                // Attach to the event to receive the depth frame data.
                kinectRuntime.DepthFrameReady += KinectRuntime_DepthFrameReady;

                // Start capturing the depth data by opening the stream.
                kinectRuntime.DepthStream.Open(ImageStreamType.Depth, 2, ImageResolution.Resolution320x240, ImageType.DepthAndPlayerIndex);
                
                kinectRuntime.NuiCamera.ElevationAngle = 0;
            }
        }

        #endregion

        #region  --------------------- Event Handlers ---------------------

        private void KinectRuntime_DepthFrameReady(object sender, ImageFrameReadyEventArgs e)
        {
            // Create our own byte array with our own colorized pixels.
            byte[] colorizedByteData = GetColorizedDepthData(e.ImageFrame);

            PlanarImage image = e.ImageFrame.Image;

            // Create an image based on our colorized array.
            DepthImage.Source = BitmapSource.Create(
                image.Width, image.Height, 96, 96, PixelFormats.Bgra32, null, colorizedByteData, image.Width * PixelFormats.Bgra32.BitsPerPixel / 8);
        }

        private byte[] GetColorizedDepthData(ImageFrame imageFrame)
        {
            int height = imageFrame.Image.Height;
            int width = imageFrame.Image.Width;

            // Get the raw depth data in a byte array.
            byte[] depthData = imageFrame.Image.Bits;


            // Create our own byte array to hold the new colorized byte array each with 4 bits.
            byte[] colorFrame = new byte[imageFrame.Image.Height * imageFrame.Image.Width * 4];

            // Constants for the bit positions for the RGBA data.
            const int BlueIndex = 0;
            const int GreenIndex = 1;
            const int RedIndex = 2;
            const int TransparencyIndex = 3;

            // Start at the first position in the raw depth data.
            var depthIndex = 0;

            // Iterate each row in the data.
            for (var y = 0; y < height; y++)
            {

                var heightOffset = y * width;

                // Iterate each column in the data.
                for (var x = 0; x < width; x++)
                {
                    // Get the index of the byte array for the colors by 4 due to the four bits for the RGBA data.
                    var index = ((width - x - 1) + heightOffset) * 4;

                    // Get the index of the player (0 - 7) for this pixel.
                    int playerIndex = GetPlayerIndex(depthData[depthIndex]);

                    // By default make all the pixel colors transparent to start with.
                    Color pixelColor = Colors.Transparent;

                    // Change the pixel color based on the player index.
                    switch (playerIndex)
                    {
                        case 1:
                            pixelColor = Colors.Red;
                            break;
                        case 2:
                            pixelColor = Colors.Green;
                            break;
                        case 3:
                            pixelColor = Colors.Blue;
                            break;
                        case 4:
                            pixelColor = Colors.White;
                            break;
                        case 5:
                            pixelColor = Colors.Gold;
                            break;
                        case 6:
                            pixelColor = Colors.Cyan;
                            break;
                        case 7:
                            pixelColor = Colors.Plum;
                            break;
                    }

                    // Set the RGBA bits for the color byte array.
                    colorFrame[index + BlueIndex] = (byte)pixelColor.B;
                    colorFrame[index + RedIndex] = (byte)pixelColor.R;
                    colorFrame[index + GreenIndex] = (byte)pixelColor.G;
                    colorFrame[index + TransparencyIndex] = (byte)pixelColor.A;

                    // Increment by 2 since there are 2 bytes of data for each pixel.
                    depthIndex += 2;
                }
            }

            return colorFrame;
        }

        private static int GetPlayerIndex(byte firstFrame)
        {
            // Use bit shifting to get the player index from the first 3 bits of the first byte.
             return (int)firstFrame & 7;
        }
       
        #endregion
    }
}

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
using Coding4Fun.Kinect.Wpf.Controls;

using Microsoft.Research.Kinect.Nui;

namespace Kinect.Demo.UserInteraction
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region --------------------- Member Variables ---------------------

        // Kinect Runtime
        private Runtime kinectRuntime;

        private Image currentlySelectedImage;

        private bool isClosing = false;

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

                // Initialize to return skeletal data.
                kinectRuntime.Initialize(RuntimeOptions.UseSkeletalTracking);

                // Attach to the event to receive skeleton frame data.
                kinectRuntime.SkeletonFrameReady += KinectRuntime_SkeletonFrameReady;

                kinectRuntime.SkeletonEngine.TransformSmooth = true;

                TransformSmoothParameters parameters = new TransformSmoothParameters();
                parameters.Smoothing = 0.5f;
                parameters.Correction = 0.3f;
                parameters.Prediction = 0.2f;
                parameters.JitterRadius = .2f;
                parameters.MaxDeviationRadius = 0.5f;

                kinectRuntime.SkeletonEngine.SmoothParameters = parameters;

                kinectRuntime.NuiCamera.ElevationAngle = 0;

                handCursor.Click += HandCursor_Click;
            }
        }

        private void TrackHandMovement(SkeletonData skeleton)
        {
            // Get the left and right hand joints.
            JointsCollection joints = skeleton.Joints;
            Joint rightHand = joints[JointID.HandRight];
            Joint leftHand = joints[JointID.HandLeft];

            // Find which hand is being used for cursor by which hand is closer.
            var joinCursorHand = (rightHand.Position.Z < leftHand.Position.Z)
                            ? rightHand
                            : leftHand;

            // Scale the joint position X and Y to the size of the screen.
            float posX = joinCursorHand.ScaleTo((int)SystemParameters.PrimaryScreenWidth, (int)SystemParameters.PrimaryScreenHeight).Position.X;
            float posY = joinCursorHand.ScaleTo((int)SystemParameters.PrimaryScreenWidth, (int)SystemParameters.PrimaryScreenHeight).Position.Y;
            
            // Raise an event the hand cursor has changed.
            OnHandCursorLocationChanged(handCursor, null, (int)posX, (int)posY);
        }


        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            isClosing = true;
        }

        #endregion

        #region  --------------------- Event Handlers ---------------------

        private void KinectRuntime_SkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {
            SkeletonFrame allSkeletons = e.SkeletonFrame;

            SkeletonData skeleton = (from s in allSkeletons.Skeletons
                                     where s.TrackingState == SkeletonTrackingState.Tracked
                                     select s).FirstOrDefault();

            if (skeleton != null)
            {
                TrackHandMovement(skeleton);
            }
        }

        private void OnHandCursorLocationChanged(HoverButton hand, List<Button> buttons, int x, int y)
        {
            if (HandCursorIsOverPicture(hand, EdwardButton) |
                HandCursorIsOverPicture(hand, JacobButton))
            {
                hand.Hovering();
            }
            else
            {
                hand.Release();
            }

            Canvas.SetLeft(hand, x - (hand.ActualWidth / 2));
            Canvas.SetTop(hand, y - (hand.ActualHeight / 2));
        }

        private bool HandCursorIsOverPicture(HoverButton hand, Image target)
        {
            if (isClosing || !Window.GetWindow(hand).IsActive)
            {
                return false;
            }

            var handTopLeft = new Point(Canvas.GetTop(hand), Canvas.GetLeft(hand));
            var handLeft = handTopLeft.X + (hand.ActualWidth / 2);
            var handTop = handTopLeft.Y + (hand.ActualHeight / 2);

            Point targetTopLeft = target.PointToScreen(new Point());

            if (handTop > targetTopLeft.X
                && handTop < targetTopLeft.X + target.ActualWidth
                && handLeft > targetTopLeft.Y
                && handLeft < targetTopLeft.Y + target.ActualHeight)
            {
                currentlySelectedImage = target;
                return true;
            }

            return false;
        }

        private void HandCursor_Click(object sender, RoutedEventArgs e)
        {
            if (currentlySelectedImage == EdwardButton)
            {
                EdwardBella.Visibility = System.Windows.Visibility.Visible;
               JacobBella.Visibility = System.Windows.Visibility.Hidden;
            }
            else if (currentlySelectedImage == JacobButton)
            {
                EdwardBella.Visibility = System.Windows.Visibility.Hidden;
                JacobBella.Visibility = System.Windows.Visibility.Visible;
            }
            else
            {
                EdwardBella.Visibility = System.Windows.Visibility.Hidden;
                JacobBella.Visibility = System.Windows.Visibility.Hidden;
            }
        }

        #endregion
    }
}

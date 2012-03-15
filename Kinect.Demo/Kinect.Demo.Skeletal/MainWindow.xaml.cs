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

namespace Kinect.Demo.Skeletal
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
            }
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
                DrawSkeleton(skeleton);
            }
        }

        private void DrawSkeleton(SkeletonData skeleton)
        {
            SetUIElementPosition(HeadImage, skeleton.Joints[JointID.Head]);
            SetUIElementPosition(LeftHandImage, skeleton.Joints[JointID.HandLeft]);
            SetUIElementPosition(RighthandImage, skeleton.Joints[JointID.HandRight]);
            SetUIElementPosition(LeftBootImage, skeleton.Joints[JointID.FootLeft], 50);
            SetUIElementPosition(RightBootImage, skeleton.Joints[JointID.FootRight], 50);
        }

        private void SetUIElementPosition(FrameworkElement element, Joint joint)
        {
            SetUIElementPosition(element, joint, 0);
        }

        private void SetUIElementPosition(FrameworkElement element, Joint joint, int yOffset)
        {
            var scaledJoint = joint.ScaleTo(1024, 768, .99f, .99f);

            Canvas.SetLeft(element, scaledJoint.Position.X);
            Canvas.SetTop(element, scaledJoint.Position.Y - yOffset);
        }

        #endregion
    }
}

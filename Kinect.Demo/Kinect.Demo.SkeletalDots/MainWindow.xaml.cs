using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml;

using Coding4Fun.Kinect.Wpf;

using Microsoft.Research.Kinect.Audio;
using Microsoft.Research.Kinect.Nui;
using Microsoft.Speech.AudioFormat;
using Microsoft.Speech.Recognition;

namespace Kinect.Demo.SkeletalDots
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region --------------------- Member Variables ---------------------

        // Kinect Runtime
        private Runtime kinectRuntime;

        // Collection to hold the circles for each joint in the skeleton.
        private Dictionary<JointID, Ellipse> markers;

        // Speech variables.
        private const string RecognizerId = "SR_MS_en-US_Kinect_10.0";
        private Stream stream;
        private KinectAudioSource source;
        private SpeechRecognitionEngine speechRecognitionEngine;

        #endregion

        #region --------------------- Constructor ---------------------

        /// <summary>
        /// Initializes a new instance of the MainWindow class.
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();

            SetupKinect();

            SetupAudio();
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

                /* Good blog post on the smoothing parameters:
                 * http://cm-bloggers.blogspot.com/2011/07/kinect-sdk-smoothing-skeleton-data.html
                 */
                kinectRuntime.SkeletonEngine.TransformSmooth = true;

                TransformSmoothParameters parameters = new TransformSmoothParameters();
                parameters.Smoothing = 0.5f;
                parameters.Correction = 0.3f;
                parameters.Prediction = 0.2f;
                parameters.JitterRadius = .2f;
                parameters.MaxDeviationRadius = 0.99f;

                kinectRuntime.SkeletonEngine.SmoothParameters = parameters;

                kinectRuntime.NuiCamera.ElevationAngle = 0;
            }
        }

        private void SetupAudio()
        {
            source = new KinectAudioSource();

            source.FeatureMode = true;
            source.AutomaticGainControl = false;
            source.SystemMode = SystemMode.OptibeamArrayOnly;

            RecognizerInfo ri = SpeechRecognitionEngine.InstalledRecognizers().Where(r => r.Id == RecognizerId).FirstOrDefault();

            if (ri != null)
            {
                speechRecognitionEngine = new SpeechRecognitionEngine(ri.Id);

                var colors = new Choices();
                colors.Add("red");
                colors.Add("green");
                colors.Add("blue");

                var gb = new GrammarBuilder();
                gb.Culture = ri.Culture;
                gb.Append(colors);

                var g = new Grammar(gb);

                speechRecognitionEngine.LoadGrammar(g);
                speechRecognitionEngine.SpeechRecognized += SpeechRecognitionEngine_SpeechRecognized;
                speechRecognitionEngine.SpeechDetected += SpeechRecognitionEngine_SpeechDetected;

                stream = source.Start();

                speechRecognitionEngine.SetInputToAudioStream(stream, new SpeechAudioFormatInfo(EncodingFormat.Pcm, 16000, 16, 1, 0x7d00, 2, null));

                speechRecognitionEngine.RecognizeAsync(RecognizeMode.Multiple);
            }
        }

        private void SetEllipseColor(Color color)
        {
            if (markers != null)
            {
                foreach (var key in markers.Keys)
                {
                    markers[key].Fill = new SolidColorBrush(color);
                }
            }
        }

        private void DrawSkeleton(SkeletonData skeleton)
        {
            if (markers == null)
            {
                markers = new Dictionary<JointID, Ellipse>();
            }

            if (markers.Count == 0)
            {
                foreach (Joint joint in skeleton.Joints)
                {
                    if (!markers.ContainsKey(joint.ID))
                    {
                        Ellipse jointEllipse = Copy(EllipseTemplate);
                        jointEllipse.Visibility = Visibility.Visible;

                        markers.Add(joint.ID, jointEllipse);

                        MainCanvas.Children.Add(markers[joint.ID]);
                    }
                }
            }

            foreach (Joint joint in skeleton.Joints)
            {
                SetUIElementPosition(markers[joint.ID], joint);
            }
        }

        private void SetUIElementPosition(FrameworkElement element, Joint joint)
        {
            var scaledJoint = joint.ScaleTo(1024, 768, 1f, 1f);

            Canvas.SetLeft(element, scaledJoint.Position.X);
            Canvas.SetTop(element, scaledJoint.Position.Y);
        }

        private Ellipse Copy(UIElement element)
        {
            string shapestring = XamlWriter.Save(element);
            StringReader stringReader = new StringReader(shapestring);
            XmlTextReader xmlTextReader = new XmlTextReader(stringReader);
            Ellipse deepCopyobject = (Ellipse)XamlReader.Load(xmlTextReader);
            return deepCopyobject;
        }
        
        private void CleanUpAudioResources()
        {
            if (source != null)
            {
                source.Stop();
                source = null;
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
        
        private void SpeechRecognitionEngine_SpeechDetected(object sender, SpeechDetectedEventArgs e)
        {
             SetEllipseColor(Colors.HotPink);
        }

        private void SpeechRecognitionEngine_SpeechRecognized(object sender, SpeechRecognizedEventArgs e)
        {
            switch (e.Result.Text)
            {
                case "red":
                    SetEllipseColor(Colors.Red);
                    break;
                case "green":
                    SetEllipseColor(Colors.Green);
                    break;
                default:
                    SetEllipseColor(Colors.Blue);
                    break;
            }
        }
        
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            CleanUpAudioResources();
        }

        #endregion
    }
}

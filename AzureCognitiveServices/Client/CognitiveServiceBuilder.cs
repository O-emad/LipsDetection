using OpenCvSharp.WpfExtensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Windows.Threading;
using static AzureCognitiveServices.Client.CognitiveService;
using FaceAPI = Microsoft.Azure.CognitiveServices.Vision.Face;
using VisionAPI = Microsoft.Azure.CognitiveServices.Vision.ComputerVision;
using OpenCvSharp;

namespace AzureCognitiveServices.Client
{
    public class CognitiveServiceBuilder : ICognitiveServiceBuilder
    {
        protected CognitiveService Service;
        private CognitiveServiceBuilder()
        {
            Service = new CognitiveService();

        }
        public static CognitiveServiceBuilder Create() => new CognitiveServiceBuilder();

        public CognitiveServiceBuilder HavingHttpConnection(Action<IHttpConnectedServiceBuilder> connectionConfigure)
        {
            var builder = new HttpConnectedServiceBuilder();
            connectionConfigure(builder);
            Service.HttpConnectionConfiguration = builder.Build();
            return this;
        }
        public CognitiveServiceBuilder EnableResultOverlay()
        {
            Service.DrawResult = true;
            return this;
        }

        public CognitiveService Build(Dispatcher dispatcher)
        {

            Service.Dispatcher = dispatcher;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            Service.Grabber = new FrameGrabber<LiveCameraResult>();
            // Set up a listener for when the client receives a new frame.
            Service.Grabber.NewFrameProvided += (s, e) =>
            {

                //controls client face detection
                //if we are fusing data, then we detect locally
                if (Service.FuseClientRemoteResults)
                {
                    // Local face detection. 
                    var intputArray = InputArray.Create(e.Frame.Image);
                    OutputArray outputArray = OutputArray.Create(new Mat());
                    Cv2.CvtColor(intputArray,outputArray,ColorConversionCodes.BGR2GRAY);
                    var image = outputArray.GetMat();
                    var rects = Service.LocalFaceDetector.DetectMultiScale(image,1.05,5, HaarDetectionTypes.DoRoughSearch | HaarDetectionTypes.FindBiggestObject, new Size(100,100));
                    if(rects.Length > 0)
                    {

                         Rect? _face = rects.OrderByDescending(f => f.Width).FirstOrDefault();
                        if (_face is not null)
                        {
                            Rect[] faces = new Rect[] { _face.Value };
                            e.Frame.UserData = faces;
                            
                        }
                            
                    }
                    else
                    {
                        e.Frame.UserData=null;
                    }
                    // Attach faces to frame. 
                    
                }
                
                // The callback may occur on a different thread, so we must use the
                // MainWindow.Dispatcher when manipulating the UI. 
                _ = dispatcher.BeginInvoke((Action)(() =>
                {
                    // Display the image in the left pane.
                    if(Service.LeftImage is not null)
                        Service.LeftImage.Source = e.Frame.Image.ToBitmapSource();

                    // If we're fusing client-side face detection with remote analysis, show the
                    // new frame now with the most recent analysis available. 
                    if (Service.FuseClientRemoteResults)
                    {
                        Service.RightImage.Source = Service.VisualizeResult(e.Frame);
                    }
                }));


            };

            // Set up a listener for when the client receives a new result from an API call. 
            Service.Grabber.NewResultAvailable += (s, e) =>
            {
                _ = dispatcher.BeginInvoke((Action)(() =>
                {
                    if (e.TimedOut)
                    {
                        Service.MessageArea.Text = "API call timed out.";
                    }
                    else if (e.Exception != null)
                    {
                        string apiName = "";
                        string message = e.Exception.Message;
                        if (e.Exception is FaceAPI.Models.APIErrorException faceEx)
                        {
                            apiName = "Face";
                            message = faceEx.Message;
                        }
                        else if (e.Exception is VisionAPI.Models.ComputerVisionErrorResponseException visionEx)
                        {
                            apiName = "Computer Vision";
                            message = visionEx.Message;
                        }
                        Service.MessageArea.Text = string.Format("{0} API call failed on frame {1}. Exception: {2}", apiName, e.Frame.Metadata.Index, message);
                    }
                    else
                    {
                        Service.LatestResultsToDisplay.Copy(e.Analysis);


                        // Display the image and visualization in the right pane. 
                        if (!Service.FuseClientRemoteResults)
                        {
                            
                            Service.RightImage.Source = Service.VisualizeResult(e.Frame);
                        }
                    }
                }));
            };

            // Create local face detector. 
            _ = Service.LocalFaceDetector.Load(@"Data/haarcascade_frontalface_default.xml");
            _ = Service.SmileDetector.Load(@"Data/haarcascade_smile.xml");



            return Service;
        }
    }
}

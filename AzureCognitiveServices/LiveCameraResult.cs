
using Microsoft.Toolkit.Mvvm.ComponentModel;
using OpenCvSharp;
using FaceAPI = Microsoft.Azure.CognitiveServices.Vision.Face.Models;
using VisionAPI = Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;

namespace AzureCognitiveServices
{
    public class LiveCameraResult:ObservableObject
    {
        //private bool newDataToggle;
        private FaceAPI.DetectedFace[] faces;
        public FaceAPI.DetectedFace[] Faces { 
            get=> faces;
            set {
                SetProperty(ref faces, value);
            }
        }
        public string[] CelebrityNames { get; set; } = null;
        public VisionAPI.ImageTag[] Tags { get; set; } = null;

        public void Copy(LiveCameraResult newResults)
        {
            Faces = newResults.Faces;
            CelebrityNames = newResults.CelebrityNames;
            Tags = newResults.Tags;
        }

    }
}
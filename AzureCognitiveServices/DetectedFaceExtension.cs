using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using FaceAPI = Microsoft.Azure.CognitiveServices.Vision.Face.Models;

namespace AzureCognitiveServices
{
    public static class DetectedFaceExtension
    {
        public static Rect? CalculateMouthRectangle(this FaceAPI.DetectedFace face)
        {
            if (face.FaceLandmarks.MouthLeft is not null && face.FaceLandmarks.MouthRight is not null)
            {
                return new Rect(
                    face.FaceLandmarks.MouthLeft.X, (face.FaceLandmarks.MouthLeft.Y + ((30 / 245.0) * face.FaceRectangle.Height)),
                    face.FaceLandmarks.MouthRight.X - face.FaceLandmarks.MouthLeft.X, ((30 / 245.0) * face.FaceRectangle.Height)
                    );
            }
            return null;
        }
    }
}

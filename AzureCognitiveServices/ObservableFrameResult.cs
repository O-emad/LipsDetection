using Microsoft.Toolkit.Mvvm.ComponentModel;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureCognitiveServices
{
    public class ObservableFrameResult: ObservableObject
    {
        private Rect faceRectangle;

        public Rect FaceRectangle {
            get=> faceRectangle;
            set => SetProperty(ref faceRectangle, value); }
    }
}

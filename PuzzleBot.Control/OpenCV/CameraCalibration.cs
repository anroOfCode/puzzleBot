// Copyright (c) 2016 Andrew Robinson. All rights reserved.

namespace PuzzleBot.Control.OpenCV
{
    public struct CameraParameters
    {
        public float FocalX;
        public float FocalY;
        public float CenterX;
        public float CenterY;
    }

    public struct DistortionCoefficients
    {
        public float K_1;
        public float K_2;
        public float P_1;
        public float P_2;
        public float K_3;
    }

    public sealed class CameraCalibration
    {
    }
}

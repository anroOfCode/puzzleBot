// Copyright (c) 2016 Andrew Robinson. All rights reserved.

using System.Runtime.InteropServices;

namespace PuzzleBot.Control.OpenCV
{
    public unsafe static class NativeMethods
    {
        const string c_dllName = "PuzzleBot.OpenCVInterop.dll";

        public struct CaptureEngine
        {
            public void* Handle;
        }
        public struct Mat
        {
            public void* Handle;
        }

        [DllImport(c_dllName, CharSet=CharSet.Ansi)]
        public static extern CaptureEngine CaptureEngine_Create(string captureUrl);

        [DllImport(c_dllName)]
        public static extern void CaptureEngine_Destroy(CaptureEngine handle);

        [DllImport(c_dllName)]
        public static extern bool CaptureEngine_TryGrabFrame(CaptureEngine handle, Mat* frame);

        [DllImport(c_dllName)]
        public static extern void Mat_Destroy(Mat handle);

        [DllImport(c_dllName)]
        public static extern int Mat_GetRows(Mat handle);

        [DllImport(c_dllName)]
        public static extern int Mat_GetColumns(Mat handle);

        [DllImport(c_dllName)]
        public static extern byte* Mat_GetData(Mat handle);

        [DllImport(c_dllName)]
        public static extern int Mat_GetDims(Mat handle);

        [DllImport(c_dllName)]
        public static extern void Mat_GetSteps(Mat handle, int[] steps);

        [DllImport(c_dllName)]
        public static extern int Mat_GetType(Mat handle);

        [DllImport(c_dllName)]
        public static extern int Mat_GetChannels(Mat handle);

        [DllImport(c_dllName)]
        public static extern Mat Mat_Create(int type, int channels, int rows, int cols, void* data);

        [DllImport(c_dllName)]
        public static extern Mat Mat_DrawCrosshair(Mat handle);

        [DllImport(c_dllName)]
        public static extern bool TryFindChessboardCorners(Mat img, int patternWidth, int patternHeight, float[] cornerPoints);

        [DllImport(c_dllName)]
        public static extern void ComputeCalibration(int patternWidth, int patternHeight,
            float mmPerSquare, int cameraWidth, int cameraHeight, float[] cornerPoints, int numBoards,
            double* rms, Mat* distCoeffs, Mat* cameraMatrix);

        [DllImport(c_dllName)]
        public static extern Mat Undistort(Mat img, Mat cameraMatrix, Mat distCoeffs);
    }
}

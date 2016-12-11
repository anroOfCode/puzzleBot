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

    public struct Point<T>
    {
        public T X;
        public T Y;
    }

    public sealed class ChessboardCornerFinder 
    {
        private int _width;
        private int _height;

        public ChessboardCornerFinder(int chessboardWidthInSquares, int chessboardHeightInSqaures)
        {
            _width = chessboardWidthInSquares;
            _height = chessboardHeightInSqaures;
        }

        public Point<float>[] TryFind(Mat img)
        {
            var pts = new float[_width * _height * 2];
            var didFind = NativeMethods.TryFindChessboardCorners(img.Handle, _width, _height, pts);
            if (!didFind) return null;
            var ret = new Point<float>[_width * _height];
            for (int i = 0; i < _width * _height; i++) {
                ret[i] = new Point<float>() { X = pts[i * 2], Y = pts[i * 2 + 1] };
            }
            return ret;
        }
    }

    public static class CameraCalibration
    {
        public static double Calibrate(int widthInSquares, int heightInSquares, float mmPerSquare, 
            Point<float>[][] cornerPts, Point<int> cameraSize, out Mat cameraMatrix, out Mat distCoeffs)
        {
            var flattenedPoints = new float[cornerPts.Length * widthInSquares * heightInSquares * 2];
            int flattenedPtIdx = 0;
            for (int i = 0; i < cornerPts.Length; i++) {
                for (int j = 0; j < widthInSquares * heightInSquares; j++) {
                    flattenedPoints[flattenedPtIdx] = cornerPts[i][j].X;
                    flattenedPoints[flattenedPtIdx + 1] = cornerPts[i][j].Y;
                    flattenedPtIdx += 2;
                }
            }

            NativeMethods.Mat innerDistCoeffs;
            NativeMethods.Mat innerCameraMatrix;
            double rms;
            unsafe
            {
                NativeMethods.ComputeCalibration(widthInSquares, heightInSquares, mmPerSquare, 
                    cameraSize.X, cameraSize.Y, flattenedPoints, cornerPts.Length, 
                    &rms, &innerDistCoeffs, &innerCameraMatrix);
            }
            cameraMatrix = new Mat(innerCameraMatrix);
            distCoeffs = new Mat(innerDistCoeffs);
            return rms;
        }

        public static Mat Undistort(Mat img, Mat cameraIntrin, Mat distCoeff)
        {
            return new Mat(NativeMethods.Undistort(img.Handle, cameraIntrin.Handle, distCoeff.Handle));
        }
    }
}

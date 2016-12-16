// Copyright (c) 2016 Andrew Robinson. All rights reserved.

using Newtonsoft.Json;
using System;

namespace PuzzleBot.Control.OpenCV
{
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
        public sealed class ChessboardParams
        {
            public int HorizontalSquareCount;
            public int VerticalSquareCount;
            public float SquareWidthInMm;
        }

        public static double Calibrate(ChessboardParams cbParams,
            Point<float>[][] cornerPts, Point<int> cameraSize, out Mat cameraMatrix, out Mat distCoeffs)
        {
            var flattenedPoints = new float[cornerPts.Length * cbParams.HorizontalSquareCount * cbParams.VerticalSquareCount * 2];
            int flattenedPtIdx = 0;
            for (int i = 0; i < cornerPts.Length; i++) {
                for (int j = 0; j < cbParams.HorizontalSquareCount * cbParams.VerticalSquareCount; j++) {
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
                NativeMethods.ComputeCalibration(cbParams.HorizontalSquareCount, cbParams.VerticalSquareCount, cbParams.SquareWidthInMm, 
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

        private const string c_componentName = "CameraCalibration";

        private static ChessboardParams ConfigureChessboard(IHost host)
        {
            host.WriteLogMessage(c_componentName, "Enter number of chessboard squares in a row:");
            var inRow = int.Parse(host.ReadLine());
            host.WriteLogMessage(c_componentName, "Enter number of chessboard squares in a column:");
            var inCol = int.Parse(host.ReadLine());
            host.WriteLogMessage(c_componentName, "Enter width of a single square in millimeters:");
            var mm = float.Parse(host.ReadLine());
            return new ChessboardParams { HorizontalSquareCount = inRow, VerticalSquareCount = inCol, SquareWidthInMm = mm };
        }

        public static ChessboardParams EnsureChessboardConfigured(IHost host)
        {
            var cbparams = host.GetParam<CameraCalibration.ChessboardParams>("Chessboard");
            if (cbparams == null) {
                cbparams = CameraCalibration.ConfigureChessboard(host);
                host.SaveParam("Chessboard", cbparams);
            }
            else {
                host.WriteLogMessage(c_componentName, "Found chessboard data:" +
                    Environment.NewLine + JsonConvert.SerializeObject(cbparams));
                host.WriteLogMessage(c_componentName, "Keep? (Y/N)");
                if (!host.ReadLine().Equals("Y")) {
                    cbparams = ConfigureChessboard(host);
                    host.SaveParam("Chessboard", cbparams);
                }
            }
            return cbparams;
        }

        //public static void RunCalibration(IHost host, CaptureEngine camera,
        //    int numTargets, ChessboardParams board, float mmPerSquare, 
        //    out Mat cameraMatrix, out Mat distCoeffs)
        //{

        //}
    }
}

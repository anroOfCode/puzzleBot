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
}

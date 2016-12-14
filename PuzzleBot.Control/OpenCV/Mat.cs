// Copyright (c) 2016 Andrew Robinson. All rights reserved.

using System;
using System.Diagnostics.Contracts;

namespace PuzzleBot.Control.OpenCV
{
    /// <summary>
    /// A wrapper around the OpenCV Mat type, currently only
    /// really support 2D matrices with U8 or floating point data.
    /// </summary>
    public unsafe sealed class Mat
        : IDisposable
    {
        public enum Types
        {
            CV_8U = 0,
            CV_32F = 5,
            CV_64F = 6
        }

        public Mat(NativeMethods.Mat inner)
        {
            Contract.Assert(inner.Handle != null);
            _mat = inner;
            _memPressure = Rows * Columns * 3;
            GC.AddMemoryPressure(_memPressure);

            _dims = NativeMethods.Mat_GetDims(_mat);
            _steps = new int[_dims];
            NativeMethods.Mat_GetSteps(_mat, _steps);
        }

        public Mat(Types type, int channels, int rows, int cols, void* data)
            : this(NativeMethods.Mat_Create((int)type, channels, rows, cols, data))
        {
        }

        public byte* RawData
        {
            get {
                if (_mat.Handle == null) return null;
                return NativeMethods.Mat_GetData(_mat);
            }
        }

        public int Rows 
        {
            get {
                if (_mat.Handle == null) return -1;
                return NativeMethods.Mat_GetRows(_mat);
            }
        }

        public int Columns 
        {
            get {
                if (_mat.Handle == null) return -1;
                return NativeMethods.Mat_GetColumns(_mat);
            }
        }

        public int Channels {
            get {
                if (_mat.Handle == null) return -1;
                return NativeMethods.Mat_GetChannels(_mat);
            }
        }

        public Types Type {
            get {
                if (_mat.Handle == null) return Types.CV_8U;
                return (Types)NativeMethods.Mat_GetType(_mat);
            }
        }

        public NativeMethods.Mat Handle { get { return _mat; } }

        public void* GetAddress(int x, int y)
        {
            return RawData + x * _steps[0] + y * _steps[1];
        }

        ~Mat()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (_mat.Handle != null) {
                NativeMethods.Mat_Destroy(_mat);
                GC.RemoveMemoryPressure(_memPressure);
                _mat.Handle = null;
            }
        }

        private NativeMethods.Mat _mat;
        private readonly long _memPressure;
        private readonly int[] _steps;
        private readonly int _dims;
    }

    public unsafe static class MatReader
    {
        public static Tuple<byte, byte, byte> GetColor(Mat source, int x, int y)
        {
            Contract.Assert(source != null);
            Contract.Assert(source.Channels == 3);
            Contract.Assert(source.Type == Mat.Types.CV_8U);
            var addr = (byte*)source.GetAddress(x, y);
            return Tuple.Create((byte)addr[0], (byte)addr[1], (byte)addr[3]);
        }

        public static float GetFloat(Mat source, int x, int y)
        {
            Contract.Assert(source != null);
            Contract.Assert(source.Channels == 1);
            Contract.Assert(source.Type == Mat.Types.CV_32F);
            return *(float*)source.GetAddress(x, y);
        }
    }
}

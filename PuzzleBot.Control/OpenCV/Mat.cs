// Copyright (c) 2016 Andrew Robinson. All rights reserved.

using System;
using System.Diagnostics.Contracts;

namespace PuzzleBot.Control.OpenCV
{
    public unsafe sealed class Mat
        : IDisposable
    {
        public Mat(NativeMethods.Mat inner)
        {
            Contract.Assert(inner.Handle != null);
            _mat = inner;
            _memPressure = Rows * Columns * 3;
            GC.AddMemoryPressure(_memPressure);
        }

        public byte* Data
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

        NativeMethods.Mat _mat;
        long _memPressure;
    }
}

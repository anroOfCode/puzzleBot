// Copyright (c) 2016 Andrew Robinson. All rights reserved.

using System;

namespace PuzzleBot.Control.OpenCV
{
    public unsafe sealed class Mat
        : IDisposable
    {
        public Mat(NativeMethods.Mat inner)
        {
            _mat = inner;
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
                _mat.Handle = null;
            }
        }

        NativeMethods.Mat _mat;
    }
}

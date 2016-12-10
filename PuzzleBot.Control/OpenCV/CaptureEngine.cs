// Copyright (c) 2016 Andrew Robinson. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuzzleBot.Control.OpenCV
{
    public sealed class CaptureEngine
        : IDisposable
    {
        public CaptureEngine(string url)
        {
            _capturer = NativeMethods.CaptureEngine_Create(url);
        }

        public Mat TryGrabFrame()
        {
            unsafe
            {
                NativeMethods.Mat ret;
                if (NativeMethods.CaptureEngine_TryGrabFrame(_capturer, &ret)) {
                    return new Mat(ret);
                }
            }
            return null;
        }

        ~CaptureEngine()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private unsafe void Dispose(bool disposing)
        {
            if (_capturer.Handle != null) {
                NativeMethods.CaptureEngine_Destroy(_capturer);
                _capturer.Handle = null;
            }
        }

        NativeMethods.CaptureEngine _capturer;
    }
}

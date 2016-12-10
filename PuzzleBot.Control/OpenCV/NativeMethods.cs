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
        public static extern Mat CaptureEngine_GrabFrame(CaptureEngine handle);

        [DllImport(c_dllName)]
        public static extern void Mat_Destroy(Mat handle);

        [DllImport(c_dllName)]
        public static extern int Mat_GetRows(Mat handle);

        [DllImport(c_dllName)]
        public static extern int Mat_GetColumns(Mat handle);

        [DllImport(c_dllName)]
        public static extern byte* Mat_GetData(Mat handle);
    }
}

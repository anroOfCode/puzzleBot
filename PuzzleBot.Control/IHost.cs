// Copyright (c) 2016 Andrew Robinson. All rights reserved.

using Newtonsoft.Json.Linq;
using System;
using System.Diagnostics.Contracts;
using System.IO;

namespace PuzzleBot.Control
{
    public interface IHost
    {
        T GetParam<T>(string name);
        void SaveParam<T>(string name, T val);

        void WriteLogMessage(string component, string msg);

        ICameraView CreateCameraView(string title);

        string ReadLine();

        void AddKeyListener(char keyChar, bool up, Action method);
    }

    public interface ICameraView
    {
        void UpdateImage(OpenCV.Mat image);
    }
}

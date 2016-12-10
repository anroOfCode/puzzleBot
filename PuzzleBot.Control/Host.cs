// Copyright (c) 2016 Andrew Robinson. All rights reserved.

namespace PuzzleBot.Control
{
    public interface IHost
    {
        T GetParam<T>(string name);
        void SaveParam<T>(string name, T val);

        void WriteLogMessage(string component, string msg);
    }
}

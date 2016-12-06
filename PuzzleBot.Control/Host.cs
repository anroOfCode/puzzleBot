using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuzzleBot.Control
{
    public interface IHost
    {
        T GetParam<T>(string name);
        void SaveParam<T>(string name, T val);

        void WriteLogMessage(string component, string msg);
    }
}

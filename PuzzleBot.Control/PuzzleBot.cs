using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuzzleBot.Control
{
    public class PuzzleBot
    {
        IHost _host;
        OpenCV.CaptureEngine _downwardCamera;
        OpenCV.CaptureEngine _upwardCamera;
        CncMachine _machine;

        public PuzzleBot(IHost host)
        {
            _host = host;
        }
    }
}

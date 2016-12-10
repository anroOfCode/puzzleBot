using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuzzleBot.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            var ce = new Control.OpenCV.CaptureEngine("http://localhost:8080/cam_1.cgi?.mjpg");
            while (true) {
                using (var mat = ce.GrabFrame())
                    System.Console.WriteLine($"{mat.Columns} x {mat.Rows} mat.");
            }

            System.Console.ReadLine();
        }
    }
}

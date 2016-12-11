using PuzzleBot.Control.OpenCV;
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
            var cv = new CameraView("Camera 1");
            int i = 0;

            var finder = new ChessboardCornerFinder(7, 5);

            while (true) {
                using (var mat = ce.TryGrabFrame())
                    if (mat != null) {
                        System.Console.WriteLine($"{i++}: {mat.Columns} x {mat.Rows} mat.");
                        if (finder.TryFind(mat) != null) {
                            System.Console.WriteLine("Found some corners!");
                        }
                        cv.UpdateImage(mat);
                    }
                    else
                        System.Console.WriteLine("No data.");
            }
            System.Console.ReadLine();
        }
    }
}

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

            while (true) {
                using (var mat = ce.TryGrabFrame())
                    if (mat != null) {
                        System.Console.WriteLine($"{i++}: {mat.Columns} x {mat.Rows} mat.");
                        cv.UpdateImage(mat);
                    }
                    else
                        System.Console.WriteLine("No data.");
            }
            System.Console.ReadLine();
        }
    }
}

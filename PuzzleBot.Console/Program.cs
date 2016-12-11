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
            var ce = new CaptureEngine("http://localhost:8080/cam_1.cgi?.mjpg");
            var cv = new CameraView("Camera 1");
            int i = 0;
            var finder = new ChessboardCornerFinder(7, 5);
            var boards = new List<Point<float>[]>();

            while (true) {
                using (var mat = ce.TryGrabFrame())
                    if (mat != null) {
                        Point<float>[] corners = null;
                        if ((corners = finder.TryFind(mat)) != null) {
                            System.Console.WriteLine("Found some corners!");
                            boards.Add(corners);
                            System.Threading.Thread.Sleep(1000);
                        }
                        cv.UpdateImage(mat);

                        if (boards.Count == 10) {
                            System.Console.WriteLine("Found ten boards. Calibrating.");
                            break;
                        }
                    }
                    else
                        System.Console.WriteLine("No data.");
            }

            Mat distCoff;
            Mat cameraMatrix;
            CameraCalibration.Calibrate(7, 5, 100, boards.ToArray(), new Point<int>() { X = 640, Y = 480 }, out cameraMatrix, out distCoff);

            var cvCorrected = new CameraView("Corrected");
            while (true) {
                using (var mat = ce.TryGrabFrame())
                    if (mat != null) {
                        cv.UpdateImage(mat);
                        using (var correctedMat = CameraCalibration.Undistort(mat, cameraMatrix, distCoff)) {
                            cvCorrected.UpdateImage(correctedMat);
                        }
                    }
            }

            System.Console.ReadLine();
        }
    }
}

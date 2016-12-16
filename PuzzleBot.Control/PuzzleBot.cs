using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using PuzzleBot.Control.OpenCV;

namespace PuzzleBot.Control
{
    public class PuzzleBot
    {
        IHost _host;
        OpenCV.CaptureEngine _downwardCamera;
        OpenCV.CaptureEngine _upwardCamera;

        ICameraView _downwardCameraView;
        ICameraView _upwardCameraView;
        Thread _viewUpdateThread;

        CncMachine _machine;

        const string c_componentName = "PuzzleBot";

        public PuzzleBot(IHost host)
        {
            _host = host;
            _host.WriteLogMessage(c_componentName, "Initializing PuzzleBot comm. channels.");
            _machine = new CncMachine(_host);
            _upwardCamera = new OpenCV.CaptureEngine(
                $"http://{_host.GetParam<string>("MachineHostName")}:{_host.GetParam<int>("UpCameraPort")}/?action=stream");
            _downwardCamera = new OpenCV.CaptureEngine(
                $"http://{_host.GetParam<string>("MachineHostName")}:{_host.GetParam<int>("DownCameraPort")}/?action=stream");
            _upwardCameraView = _host.CreateCameraView("Upward Camera");
            _downwardCameraView = _host.CreateCameraView("Downward Camera");
            _viewUpdateThread = new Thread(CameraViewUpdater);
            _viewUpdateThread.Start();

            _host.WriteLogMessage(c_componentName, "Press enter to home...");
            _host.ReadLine();
            _machine.PerformMechanicalHome();

            MainControlLoop();
        }

        void CameraViewUpdater()
        {
            while (true) {
                using (var upImage = _upwardCamera.TryGrabFrame()) {
                    if (upImage == null) continue;
                    upImage.DrawCrosshair();
                    _upwardCameraView.UpdateImage(upImage);
                }
                using (var downImage = _downwardCamera.TryGrabFrame()) {
                    if (downImage == null) continue;
                    downImage.DrawCrosshair();
                    _downwardCameraView.UpdateImage(downImage);
                }
            }
        }

        void MainControlLoop()
        {
            while (true) {
                _host.WriteLogMessage(c_componentName, "What would you like to do?" + Environment.NewLine +
                    "1 - Calibrate the downward camera" + Environment.NewLine +
                    "2 - Perform a mechanical homing operation" + Environment.NewLine +
                    "3 - Deenergize the motors" + Environment.NewLine);
                int opt = int.Parse(_host.ReadLine());
                switch (opt) {
                    case 1:
                        Routine_CameraCalibration();
                        break;
                    case 2:
                        Routine_MechHome();
                        break;
                    case 3:
                        Routine_Deenergize();
                        break;
                }
            }
        }

        void Routine_CameraCalibration()
        {

        }

        void Routine_MechHome()
        {
            _machine.PerformMechanicalHome();
        }

        void Routine_Deenergize()
        {
            _machine.Deenergize();
        }
    }
}

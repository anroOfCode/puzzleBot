﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using PuzzleBot.Control.OpenCV;
using Newtonsoft.Json.Linq;

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

        Mat _downwardCameraIntrinsic;
        Mat _downwardCameraExtrinstic;
        bool _downwardImageOverrideMode = false;

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
            _downwardCameraIntrinsic = _host.GetParam<JObject>("IntrinsticCalib").ToMat();
            _downwardCameraExtrinstic = _host.GetParam<JObject>("ExtrinsticCalib").ToMat();

            _viewUpdateThread = new Thread(CameraViewUpdater);
            _viewUpdateThread.Start();

            _host.WriteLogMessage(c_componentName, "Press enter to home...");
            _host.ReadLine();
            _machine.PerformMechanicalHome();

            AttachKeyHandlers();
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

                if (!_downwardImageOverrideMode) {
                    using (var downImage = _downwardCamera.TryGrabFrame()) {
                        if (downImage == null) continue;
                        if (_downwardCameraIntrinsic != null) {
                            using (var correctedDownImage = CameraCalibration.Undistort(downImage, _downwardCameraIntrinsic, _downwardCameraExtrinstic)) {
                                correctedDownImage.DrawCrosshair();
                                _downwardCameraView.UpdateImage(correctedDownImage);
                            }
                        }
                        else {
                            downImage.DrawCrosshair();
                            _downwardCameraView.UpdateImage(downImage);
                        }
                    }
                }
            }
        }

        private void MainControlLoop()
        {
            while (true) {
                _host.WriteLogMessage(c_componentName, "What would you like to do?" + Environment.NewLine +
                    "1 - Calibrate the downward camera" + Environment.NewLine +
                    "2 - Perform a mechanical homing operation" + Environment.NewLine +
                    "3 - Deenergize the motors" + Environment.NewLine +
                    "4 - Clear calibration data" + Environment.NewLine 
                );
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
                    case 4:
                        Routine_ClearCalibrationData();
                        break;
                }
            }
        }

        private void AttachKeyHandlers()
        {
            //Left = 37,
            //Up = 38,
            //Right = 39,
            //Down = 40,
            _host.SetKeyDelegate((char)38, false, false, () => _machine.StartJogTop());
            _host.SetKeyDelegate((char)38, true, false, () => _machine.StopJog());
            _host.SetKeyDelegate((char)40, false, false, () => _machine.StartJogBottom());
            _host.SetKeyDelegate((char)40, true, false, () => _machine.StopJog());
            _host.SetKeyDelegate((char)37, false, false, () => _machine.StartJogLeft());
            _host.SetKeyDelegate((char)37, true, false, () => _machine.StopJog());
            _host.SetKeyDelegate((char)39, false, false, () => _machine.StartJogRight());
            _host.SetKeyDelegate((char)39, true, false, () => _machine.StopJog());

            _host.SetKeyDelegate('U', false, false, () => _machine.StartJogUp());
            _host.SetKeyDelegate('U', true, false, () => _machine.StopJog());
            _host.SetKeyDelegate('D', false, false, () => _machine.StartJogDown());
            _host.SetKeyDelegate('D', true, false, () => _machine.StopJog());
            _host.SetKeyDelegate('L', false, false, () => _machine.StartJogCcw());
            _host.SetKeyDelegate('L', true, false, () => _machine.StopJog());
            _host.SetKeyDelegate('R', false, false, () => _machine.StartJogCw());
            _host.SetKeyDelegate('R', true, false, () => _machine.StopJog());

            _host.SetKeyDelegate((char)38, false, true, () => _machine.NudgeTop(_host.GetParam<double>("NudgeY")));
            _host.SetKeyDelegate((char)40, false, true, () => _machine.NudgeBottom(_host.GetParam<double>("NudgeY")));
            _host.SetKeyDelegate((char)37, false, true, () => _machine.NudgeLeft(_host.GetParam<double>("NudgeX")));
            _host.SetKeyDelegate((char)39, false, true, () => _machine.NudgeRight(_host.GetParam<double>("NudgeX")));
            _host.SetKeyDelegate('U', false, true, () => _machine.NudgeUp(_host.GetParam<double>("NudgeZ")));
            _host.SetKeyDelegate('D', false, true, () => _machine.NudgeDown(_host.GetParam<double>("NudgeZ")));
            _host.SetKeyDelegate('L', false, true, () => _machine.NudgeCw(_host.GetParam<double>("NudgeA")));
            _host.SetKeyDelegate('R', false, true, () => _machine.NudgeCcw(_host.GetParam<double>("NudgeA")));
        }

        void Routine_CameraCalibration()
        {
            _host.WriteLogMessage(c_componentName, "Ensuring chessboard configured...");
            var cbParams = CameraCalibration.EnsureChessboardConfigured(_host);
            var finder = new ChessboardCornerFinder(cbParams.HorizontalSquareCount, cbParams.VerticalSquareCount);
            List<Point<float>[]> calPoints = new List<OpenCV.Point<float>[]>();

            int camWidth = 0;
            int camHeight = 0;
            Action findCorner = () => {
                _downwardImageOverrideMode = true;
                while (true) {
                    using (var img = _downwardCamera.TryGrabFrame()) {
                        camWidth = img.Columns;
                        camHeight = img.Rows;
                        var result = finder.TryFind(img);
                        _downwardCameraView.UpdateImage(img);
                        if (result != null) {
                            _host.WriteLogMessage(c_componentName, "Found corners!");
                            calPoints.Add(result);
                            Thread.Sleep(100);
                            break;
                        }
                    }
                }
                _downwardImageOverrideMode = false;
            };

            Action findCornerSet = () => {
                findCorner();
                Thread.Sleep(100);
                findCorner();
                Thread.Sleep(100);
                findCorner();
                Thread.Sleep(100);
            };

            _host.WriteLogMessage(c_componentName, "Jog machine over first calibration target and press enter.");
            _host.ReadLine();

            while (true) {
                findCornerSet();
                _host.WriteLogMessage(c_componentName, "Jog machine over next calibration target and press enter or (N) to stop.");
                if (_host.ReadLine().Equals("N")) break;
            }

            Mat camMat;
            Mat distCoeff;
            var rms = CameraCalibration.Calibrate(cbParams, calPoints.ToArray(), 
                new Point<int>() { X = camWidth, Y = camHeight }, out camMat, out distCoeff);
            _host.WriteLogMessage(c_componentName, $"Camera claibrated, {rms} error.");

            _host.SaveParam("IntrinsticCalib", camMat.ToJObject());
            _host.SaveParam("ExtrinsticCalib", distCoeff.ToJObject());
            _host.WriteLogMessage(c_componentName, "Camera calibated!");
            _downwardCameraExtrinstic = distCoeff;
            _downwardCameraIntrinsic = camMat;
        }

        void Routine_MechHome()
        {
            _machine.PerformMechanicalHome();
        }

        void Routine_Deenergize()
        {
            _machine.Deenergize();
        }

        void Routine_ClearCalibrationData()
        {
            _downwardCameraIntrinsic = null;
            _downwardCameraExtrinstic = null;
        }
    }
}

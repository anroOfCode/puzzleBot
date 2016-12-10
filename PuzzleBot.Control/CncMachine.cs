// Copyright (c) 2016 Andrew Robinson. All rights reserved.

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Diagnostics.Contracts;
using System.Threading;

namespace PuzzleBot.Control
{
    public enum MoveType
    {
        Incremental,
        Absolute
    }

    public struct CoordCmd
    {
        public readonly double? X;
        public readonly double? Y;
        public readonly double? Z;
        public readonly double? A;
        public readonly MoveType Type;

        private CoordCmd(double? x, double? y, double? z, double? a, MoveType type)
        {
            Contract.Assert(x.HasValue || y.HasValue || z.HasValue || a.HasValue);
            X = x;
            Y = y;
            Z = z;
            A = a;
            Type = type;
        }

        public static CoordCmd IncrementalMove(double? x, double? y, double? z, double? a)
        {
            return new CoordCmd(x, y, z, a, MoveType.Incremental);
        }

        public static CoordCmd SetPosition(double? x, double? y, double? z, double? a)
        {
            return new CoordCmd(x, y, z, a, MoveType.Absolute);
        }

        public static CoordCmd SetXYPosition(double x, double y)
        {
            return new CoordCmd(x, y, null, null, MoveType.Absolute);
        }

        public static CoordCmd SetZPosition(double z)
        {
            return new CoordCmd(null, null, z, null, MoveType.Absolute);
        }
    }

    public sealed class CncMachine
    {
        private enum MachineStatus
        {
            Initializing = 0,
            Ready = 1, // After boot this will be the status.
            Alarm = 2,
            Stop = 3, // After running a command
            ProgEnd = 4,
            Run = 5,
            Hold = 6,
            Probe = 7,
            RunCycle = 8,
            Homing = 9
        }

        private const string ComponentName = "CncMachine";

        private readonly IHost _host;
        private readonly NetworkSerialClient _client;

        private ICoordTranslator _coordSystem;
        private Coord _lkpos;

        private MachineStatus _lastStatus;
        private bool _homed;
        private bool _error;

        private AutoResetEvent _transitionToMovement = new AutoResetEvent(false);
        private AutoResetEvent _transitionToIdle = new AutoResetEvent(false);

        // The logical coordinate system used by the machine. 
        public Coord Max { get { return _coordSystem.Max; } }
        public Coord Min { get { return _coordSystem.Min; } }

        private bool IsIdle { get { return IsIdleState(_lastStatus); } }

        public CncMachine(IHost host)
        {
            Contract.Assert(host != null);
            _host = host;

            _client = new NetworkSerialClient(
                _host.GetParam<string>("MachineHostName"),
                _host.GetParam<int>("MachinePort"),
                OnMsg
             );

            _coordSystem = new IdentityTranslator(
                new MachineCoordSystem(
                    new Coord(0, 0, -_host.GetParam<double>("MaxZ"), 0),
                    new Coord(
                        _host.GetParam<double>("MaxX"),
                        _host.GetParam<double>("MaxY"),
                        0,
                        _host.GetParam<double>("MaxA")
                    )
                )
            );

            // We need to request status a few times when the TinyG controller first boots
            // up for *some* reason.
            RequestStatus();
            RequestStatus();
            RequestStatus();
        }

        public void PerformMechanicalHome()
        {
            ThrowIfError();
            var j = new JObject();
            j["gc"] = "G28.2 X0 Y0 Z0";
            RawSend(j);
            WaitForMovement();
            WaitForIdle();
            _homed = true;
        }

        public void MoveTo(CoordCmd cmd, bool sync = true)
        {
            ThrowIfError();

            if (!_homed) {
                _host.WriteLogMessage(ComponentName, "MoveTo cancelled: machine not homed.");
                throw new Exception();
            }

            if (!IsIdle) {
                RequestStatus();
                WaitForIdle();
            }

            var dstCoord = FindNewCoord(cmd, _lkpos);
            if (!_coordSystem.BoundsCheck(dstCoord)) {
                _host.WriteLogMessage(ComponentName, "MoveTo cancelled: failed bounds check.");
                throw new Exception();
            }

            var cmdStr = "G0";
            var machineDst = _coordSystem.ToInner(dstCoord);

            if (!machineDst.IsFinite()) throw new Exception();

            if (cmd.X.HasValue) cmdStr += " X" + machineDst.X;
            if (cmd.Y.HasValue) cmdStr += " Y" + machineDst.Y;
            if (cmd.Z.HasValue) cmdStr += " Z" + machineDst.Z;
            if (cmd.A.HasValue) cmdStr += " A" + machineDst.A;

            var j = new JObject();
            j["gc"] = cmdStr;

            ResetWaiters();
            RawSend(j);
            WaitForMovement();

            if (sync) {
                WaitForIdle();
            }
        }

        private static Coord FindNewCoord(CoordCmd cmd, Coord lk)
        {
            if (cmd.Type == MoveType.Absolute) 
                return new Coord(cmd.X ?? lk.X, cmd.Y ?? lk.Y, cmd.Z ?? lk.Z, cmd.A ?? lk.A);
            else {
                return new Coord(
                    lk.X + cmd.X ?? 0,
                    lk.Y + cmd.Y ?? 0,
                    lk.Z + cmd.Z ?? 0,
                    lk.A + cmd.A ?? 0
                );
            }
        }

        public void CancelMove()
        {
            ThrowIfError();
            if (!_homed) {
                _host.WriteLogMessage(ComponentName, "MoveTo cancelled: machine not homed.");
                return;
            }

            if (!IsIdle) {
                RawSend("!%");
                WaitForIdle();
            }
        }

        public double ProbeZ()
        {
            ThrowIfError();
            return 0.0;
        }

        public void TurnPumpOn()
        {
            ThrowIfError();
            _host.WriteLogMessage(ComponentName, "TurnPumpOn invoked.");
            var j = new JObject();
            j["gc"] = "M3";
            RawSend(j);
        }

        public void TurnPumpOff()
        {
            ThrowIfError();
            _host.WriteLogMessage(ComponentName, "TurnPumpOff invoked.");
            var j = new JObject();
            j["gc"] = "M5";
            RawSend(j);
        }

        public void EngageSolenoid()
        {
            ThrowIfError();
            _host.WriteLogMessage(ComponentName, "EngageSolenoid invoked.");
            var j = new JObject();
            j["gc"] = "M8";
            RawSend(j);
        }

        public void DisengageSolenoid()
        {
            ThrowIfError();
            _host.WriteLogMessage(ComponentName, "DisengageSolenoid invoked.");
            var j = new JObject();
            j["gc"] = "M9";
            RawSend(j);
        }

        public void Reset()
        {
            _error = false;
            _homed = false;
        }

        private void RawSend(JObject msg)
        {
            _client.WriteLine(msg.ToString(Formatting.None));
        }

        private void RawSend(string msg)
        {
            _client.WriteLine(msg);
        }

        private void OnMsg(string msg)
        {
            _host.WriteLogMessage("CncMachine", msg);
            try {
                var obj = JObject.Parse(msg);
                if (obj["sr"] != null) {
                    var status = (MachineStatus)obj["sr"]["stat"].Value<int>();

                    _lkpos = _coordSystem.FromInner(new Coord(
                        obj["sr"]["posx"].Value<double>(),
                        obj["sr"]["posy"].Value<double>(),
                        obj["sr"]["posz"].Value<double>(),
                        obj["sr"]["posa"].Value<double>()
                    ));

                    if (status != _lastStatus) {
                        _host.WriteLogMessage("CncMachine", $"Transitioned from {_lastStatus} to {status}.");
                        if (IsIdleState(_lastStatus) && IsMovementState(status)) {
                            _transitionToMovement.Set();
                        }
                        else if (IsMovementState(_lastStatus) && IsIdleState(status)) {
                            _transitionToIdle.Set();
                        }
                        _lastStatus = status;
                    }
                }
                else if (obj["er"] != null) {
                    _host.WriteLogMessage(ComponentName, $"An unrecoverable error has occurred.");
                    _error = true;
                }
            }
            catch {
                _host.WriteLogMessage("CncMachine", "Unable to parse log message.");
            }
        }

        private void RequestStatus()
        {
            var j = new JObject();
            j["sr"] = null;
            RawSend(j);
        }

        private void ResetWaiters()
        {
            _transitionToIdle.Reset();
            _transitionToMovement.Reset();
        }

        private bool WaitForMovement()
        {
            return WaitForMovement(TimeSpan.FromSeconds(0.5));
        }

        private bool WaitForIdle()
        {
            return WaitForIdle(TimeSpan.FromSeconds(10.0));
        }

        private bool WaitForMovement(TimeSpan timeout)
        {
            if (!_transitionToMovement.WaitOne(timeout)) {
                _host.WriteLogMessage(ComponentName, "Warning: WaitForMovement event never fired.");
                return false;
            }
            return true;
        }

        private bool WaitForIdle(TimeSpan timeout)
        {
            if (!_transitionToIdle.WaitOne(timeout)) {
                _host.WriteLogMessage(ComponentName, "Warning: WaitForIdle event never fired.");
                return false;
            }
            return true;
        }

        private void ThrowIfError()
        {
            if (_error) throw new Exception();
        }

        private static bool IsMovementState(MachineStatus status)
        {
            return
                status == MachineStatus.Homing ||
                status == MachineStatus.Run ||
                status == MachineStatus.Probe;
        }

        private static bool IsIdleState(MachineStatus status)
        {
            return status == MachineStatus.Stop || status == MachineStatus.Ready;
        }
    }
}

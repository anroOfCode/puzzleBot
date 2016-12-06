using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        MoveType Type;

        private CoordCmd(double? x, double? y, double? z, double? a, MoveType type)
        {
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
        private readonly IHost _host;
        private readonly NetworkSerialClient _client;

        private ICoordTranslator _coordSystem;
        private Coord _lkpos;

        private Coord? _srcPos;
        private Coord? _dstPos;
        private CoordCmd? _curCmd;

        private MachineStatus _lastStatus;

        // The logical coordinate system used by the machine. This system will
        // change after homing operations.
        public Coord Max { get { return _coordSystem.Max; } }
        public Coord Min { get { return _coordSystem.Min; } }

        private bool IsIdle {  get { return _lastStatus == MachineStatus.Stop; } }

        public CncMachine(IHost host)
        {
            Contract.Assert(host != null);
            _host = host;

            _host.GetParam<string>("MachineHostName");
        }

        public void PerformMechanicalHome()
        {
            // 1. Send G28.2 X0 Y0 Z0
            // 2. Wait for homing to complete
        }

        public void PerformOpticalHome()
        {
        }

        public void MoveTo(CoordCmd pos, bool sync)
        {
        }

        public void CancelMove()
        {
            if (!IsIdle) {
                RawSend("!%");
                WaitForIdle();
            }
        }

        public double ProbeZ()
        {
            return 0.0;
        }

        public void TurnPumpOn()
        {
            var j = new JObject();
            j["gc"] = "M3";
            RawSend(j);
        }

        public void TurnPumpOff()
        {
            var j = new JObject();
            j["gc"] = "M5";
            RawSend(j);
        }

        public void EngageSolenoid()
        {
            var j = new JObject();
            j["gc"] = "M8";
            RawSend(j);
        }

        public void DisengageSolenoid()
        {
            var j = new JObject();
            j["gc"] = "M9";
            RawSend(j);
        }

        private void RawSend(JObject msg)
        {

        }

        private void RawSend(string msg)
        {

        }

        private void OnMsg(string msg)
        {

        }

        private void WaitForIdle()
        {

        }
    }

    public enum MachineStatus
    {
        Initializing = 0,
        Ready = 1,
        Alarm = 2,
        Stop = 3,
        ProgEnd = 4,
        Run = 5,
        Hold = 6,
        Probe = 7,
        RunCycle = 8,
        Homing = 9
    }

    public enum MachineMotionMode
    {
        StraightTraverse = 0,
        StraightFeed = 1,
        ArcTraverse = 2,
        NoModeActive = 3
    }

    public enum MachineUnits
    {
        Inches = 0,
        Millimeters = 1,
        Degrees = 2
    }

    public enum MachineDistanceMode
    {
        Absolute = 0,
        Incremental = 1
    }
}

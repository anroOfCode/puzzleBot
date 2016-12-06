using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuzzleBot.Control
{
    public static class CncMachineUtils
    {
        public static void StartJogLeft(this CncMachine machine)
        {
            machine.CancelMove();
            machine.MoveTo(CoordCmd.SetPosition(machine.Min.X, null, null, null), false);
        }

        public static void StartJogRight(this CncMachine machine)
        {
            machine.CancelMove();
            machine.MoveTo(CoordCmd.SetPosition(machine.Max.X, null, null, null), false);
        }

        public static void StartJogTop(this CncMachine machine)
        {
            machine.CancelMove();
            machine.MoveTo(CoordCmd.SetPosition(null, machine.Max.Y, null, null), false);
        }

        public static void StartJogBottom(this CncMachine machine)
        {
            machine.CancelMove();
            machine.MoveTo(CoordCmd.SetPosition(null, machine.Min.Y, null, null), false);
        }

        public static void StartJogUp(this CncMachine machine)
        {
            machine.CancelMove();
            machine.MoveTo(CoordCmd.SetPosition(null, null, machine.Max.Z, null), false);
        }

        public static void StartJogDown(this CncMachine machine)
        {
            machine.CancelMove();
            machine.MoveTo(CoordCmd.SetPosition(null, null, machine.Min.Z, null), false);
        }

        public static void StartJogCw(this CncMachine machine)
        {
            machine.CancelMove();
            machine.MoveTo(CoordCmd.SetPosition(null, null, null, machine.Max.A), false);
        }

        public static void StartJogCcw(this CncMachine machine)
        {
            machine.CancelMove();
            machine.MoveTo(CoordCmd.SetPosition(null, null, null, machine.Min.A), false);
        }

        public static void NudgeLeft(this CncMachine machine, double nudge)
        {
            machine.MoveTo(CoordCmd.IncrementalMove(-nudge, null, null, null));
        }

        public static void NudgeRight(this CncMachine machine, double nudge)
        {
            machine.MoveTo(CoordCmd.IncrementalMove(nudge, null, null, null));
        }

        public static void NudgeTop(this CncMachine machine, double nudge)
        {
            machine.MoveTo(CoordCmd.IncrementalMove(null, nudge, null, null));
        }

        public static void NudgeBottom(this CncMachine machine, double nudge)
        {
            machine.MoveTo(CoordCmd.IncrementalMove(null, -nudge, null, null));
        }

        public static void NudgeUp(this CncMachine machine, double nudge)
        {
            machine.MoveTo(CoordCmd.IncrementalMove(null, null, nudge, null));
        }

        public static void NudgeDown(this CncMachine machine, double nudge)
        {
            machine.MoveTo(CoordCmd.IncrementalMove(null, null, -nudge, null));
        }

        public static void NudgeCw(this CncMachine machine, double nudge)
        {
            machine.MoveTo(CoordCmd.IncrementalMove(null, null, null, nudge));
        }

        public static void NudgeCcw(this CncMachine machine, double nudge)
        {
            machine.MoveTo(CoordCmd.IncrementalMove(null, null, null, -nudge));
        }

        public static void StopJog(this CncMachine machine)
        {
            machine.CancelMove();
        }
    }

}

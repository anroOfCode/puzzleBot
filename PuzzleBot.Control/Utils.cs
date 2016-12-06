using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuzzleBot.Control
{
    public static class Utils
    {
        public static bool IsFinite(double val)
        {
            return !double.IsInfinity(val) && !double.IsNaN(val);
        }
    }
}

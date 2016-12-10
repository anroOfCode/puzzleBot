// Copyright (c) 2016 Andrew Robinson. All rights reserved.

namespace PuzzleBot.Control
{
    public static class Utils
    {
        public static bool IsFinite(this double val)
        {
            return !double.IsInfinity(val) && !double.IsNaN(val);
        }

        public static bool IsFinite(this Coord val)
        {
            return
                val.X.IsFinite() &&
                val.Y.IsFinite() &&
                val.Z.IsFinite() &&
                val.A.IsFinite();
        }
    }
}

using System.Diagnostics.Contracts;

namespace PuzzleBot.Control
{
    public struct Coord
    {
        public readonly double X;
        public readonly double Y;
        public readonly double Z;
        public readonly double A;

        public Coord(double x, double y, double z, double a)
        {
            X = x;
            Y = y;
            Z = z;
            A = a;
        }
    }

    public interface ICoordSystem
    {
        Coord Max { get; }
        Coord Min { get; }

        bool BoundsCheck(Coord c);
    }

    public interface ICoordTranslator
        : ICoordSystem
    {
        ICoordSystem Inner { get; }
        Coord ToInner(Coord c);
        Coord FromInner(Coord c);
    }

    public static class CoordSystemUtils
    {
        public static bool BoundsCheck(this ICoordSystem system, Coord c)
        {
            var min = system.Min;
            var max = system.Max;
            return
                c.X >= min.X && c.X <= max.X &&
                c.Y >= min.Y && c.Y <= max.Y &&
                c.Z >= min.Z && c.Z <= max.Z &&
                c.A >= min.A && c.A <= max.A;
        }
    }

    public sealed class MachineCoordSystem
        : ICoordSystem
    {
        private readonly Coord _min;
        private readonly Coord _max;

        public MachineCoordSystem(Coord min, Coord max)
        {
            Contract.Assert(min.X < max.X);
            Contract.Assert(min.Y < max.Y);
            Contract.Assert(min.Z < max.Z);
            Contract.Assert(min.A < max.A);
            _min = min;
            _max = max;
        }

        public Coord Max { get { return _max; } }
        public Coord Min { get { return _min; } }

        public bool BoundsCheck(Coord c)
        {
            return CoordSystemUtils.BoundsCheck(this, c);
        }
    }

    public sealed class ScalingCoordSystem
        : ICoordTranslator
    {
        private readonly ICoordSystem _inner;
        private readonly double _xAdj;
        private readonly double _yAdj;
        private readonly double _zAdj;

        public ScalingCoordSystem(ICoordSystem inner, double xAdj, double yAdj, double zAdj)
        {
            Contract.Assert(inner != null);
            Contract.Assert(xAdj > 0 && Utils.IsFinite(xAdj));
            Contract.Assert(yAdj > 0 && Utils.IsFinite(yAdj));
            Contract.Assert(zAdj > 0 && Utils.IsFinite(zAdj));
            _inner = inner;
            _xAdj = xAdj;
            _yAdj = yAdj;
            _zAdj = zAdj;
        }

        public ICoordSystem Inner { get { return _inner; } }
        public Coord Max { get { return FromInner(Inner.Max); } }
        public Coord Min { get { return FromInner(Inner.Min); } }

        public bool BoundsCheck(Coord c)
        {
            return _inner.BoundsCheck(ToInner(c));
        }

        /// <summary>
        /// Converts a coordiante in inner system coordinates to the outer system coordinates.
        /// </summary>
        public Coord FromInner(Coord c)
        {
            return new Coord(c.X / _xAdj, c.Y / _yAdj, c.Z / _zAdj, c.A);
        }

        /// <summary>
        /// Converts a coordiante in this system's coordinates to the inner system coordiantes.
        /// </summary>
        public Coord ToInner(Coord c)
        {
            return new Coord(c.X * _xAdj, c.Y * _yAdj, c.Z * _zAdj, c.A);
        }
    }

    /// <summary>
    /// A ShearingCoordSystem assumes that the physical X-axis is the ground truth for alignment
    /// in the machine and is parallel to the logical X axis. It assumes that the Y axis is skewed
    /// by some angle and accepts a correction factor that represents the units of X travel introduced
    /// for every positive unit of Y travel.
    /// </summary>
    public sealed class ShearingCoordSystem
        : ICoordTranslator
    {

        private readonly double _xShear;
        private readonly ICoordSystem _inner;

        public ShearingCoordSystem(ICoordSystem inner, double xShear)
        {
            Contract.Assert(inner != null);
            Contract.Assert(xShear > -1.0 && xShear < 1.0);
            _inner = inner;
            _xShear = xShear;
        }

        public ICoordSystem Inner { get { return _inner; } }
        public Coord Max { get { return FromInner(_inner.Max); } }
        public Coord Min { get { return FromInner(_inner.Min); } }

        public bool BoundsCheck(Coord c)
        {
            return _inner.BoundsCheck(ToInner(c));
        }

        public Coord FromInner(Coord c)
        {
            return new Coord(c.X, c.Y - c.X * _xShear, c.Z, c.A);
        }

        public Coord ToInner(Coord c)
        {
            return new Coord(c.X, c.Y + c.X * _xShear, c.Z, c.A);
        }
    }

    public sealed class IdentityTranslator
        : ICoordTranslator
    {
        private readonly ICoordSystem _inner;

        public IdentityTranslator(ICoordSystem inner)
        {
            Contract.Assert(inner != null);
            _inner = inner;
        }

        public ICoordSystem Inner { get { return _inner; } }

        public Coord Min { get { return _inner.Min; } }
        public Coord Max { get { return _inner.Max; } }

        public Coord ToInner(Coord c)
        {
            return c;
        }

        public Coord FromInner(Coord c)
        {
            return c;
        }

        public bool BoundsCheck(Coord c)
        {
            return _inner.BoundsCheck(c);
        }
    }

    public static class CoordSystemBuilder
    {
        public static ICoordTranslator BuildCompositeCoordSystem(
            double xShear, double xAdj, double yAdj, double zAdj, Coord machineMin, Coord machineMax)
        {
            return new ShearingCoordSystem(
                new ScalingCoordSystem(
                    new MachineCoordSystem(machineMin, machineMax), xAdj, yAdj, zAdj), xShear);
        }

        public static ICoordTranslator BuildDefaultSystem(Coord machineMin, Coord machineMax)
        {
            return new IdentityTranslator(new MachineCoordSystem(machineMin, machineMax));
        }
    }
}

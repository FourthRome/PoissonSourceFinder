using System;

namespace Computation
{
    // The intermidiate type for point sources
    public struct CarthesianPoint
    {
        //---------------------------------------------------
        // Public properties and their backing private fields 
        //---------------------------------------------------
        public double X { get; set; }  // Coordinate 1/3
        public double Y { get; set; }  // Coordinate 2/3
        public double Z { get; set; }  // Coordinate 3/3

        //-------------
        // Constructors
        //-------------
        public CarthesianPoint(double x, double y, double z) : this()
        {
            X = x;
            Y = y;
            Z = z;
        }

        //---------------
        // Public methods
        //---------------
        public override string ToString()
        {
            return $"({X},{Y},{Z})";
        }


        //---------------
        // Static methods
        //---------------
        public static CarthesianPoint FromPolarCoordinates(double rho, double phi, double theta)
        {
            double x = rho * Math.Cos(phi) * Math.Sin(theta);
            double y = rho * Math.Sin(phi) * Math.Sin(theta);
            double z = rho * Math.Cos(theta);
            return new CarthesianPoint(x, y, z);
        }

        public static CarthesianPoint FromPolarPoint(PointSource source)
        {
            return FromPolarCoordinates(source.Rho, source.Phi, source.Theta);
        }

        public static CarthesianPoint operator -(CarthesianPoint a)
        {
            return new CarthesianPoint(-a.X, -a.Y, -a.Z);
        }

        public static CarthesianPoint operator+(CarthesianPoint a, CarthesianPoint b)
        {
            return new CarthesianPoint(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
        }

        public static CarthesianPoint operator-(CarthesianPoint a, CarthesianPoint b)
        {
            return new CarthesianPoint(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
        }

        public static CarthesianPoint operator*(CarthesianPoint a, double scale)
        {
            return new CarthesianPoint(a.X * scale, a.Y * scale, a.Z * scale);
        }

        public static CarthesianPoint operator *(double scale, CarthesianPoint a)
        {
            return a * scale;
        }
    }
}

using System;

namespace Computation
{
    // One point source of unit magnitude
    public struct PointSource
    {
        //---------------------------------------------------
        // Public properties and their backing private fields 
        //---------------------------------------------------
        private double _rho;
        public double Rho  // Coordinate 1/3
        {
            get => _rho;
            set
            {
                //if (value < 0)
                //{
                //    throw new ArgumentOutOfRangeException("[ERROR] PointSource.Rho.setter: Radial distance cannot be negative");
                //}
                _rho = value;
            }
        }
        public double Phi { get; set; }  // Coordinate 2/3
        public double Theta { get; set; }  // Coordinate 3/3

        //-------------
        // Constructors
        //-------------
        public PointSource(double rho, double phi, double theta) : this()
        {
            Phi = phi;
            Theta = theta;
            Rho = rho;
        }

        //---------------
        // Public methods
        //---------------

        // Distance to any given point
        public double SquareDistanceFrom(double rho, double phi, double theta)
        {
            return Rho * Rho + rho * rho - 2 * Rho * rho * AngleCosBetweenVectors(rho, phi, theta);
        }

        // Part of distance's computation, can be made private
        public double AngleCosBetweenVectors(double rho, double phi, double theta)
        {
            return Math.Cos(phi - Phi) * Math.Sin(theta) * Math.Sin(Theta) + Math.Cos(theta) * Math.Cos(Theta);
        }

        //---------------
        // Static methods
        //---------------
        public static PointSource FromCarthesianPoint(CarthesianPoint source)
        {
            double rho = Math.Sqrt(Math.Pow(source.X, 2) + Math.Pow(source.Y, 2) + Math.Pow(source.Z, 2));
            double phi = Math.Atan2(source.Y, source.X);
            double theta = Math.Acos(source.Z / rho);
            return new PointSource(rho, phi, theta);
        }

    }
}

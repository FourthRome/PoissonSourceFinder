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
        public PointSource(double rho=0.0, double phi=0.0, double theta=0.0) : this()
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
        
    }
}

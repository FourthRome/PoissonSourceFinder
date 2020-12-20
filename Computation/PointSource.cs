using System;
using System.Collections.Generic;
using System.Text;

namespace Computation
{
    public struct PointSource
    {
        private double _rho;
        public double Rho
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
        public double Phi { get; set; }
        public double Theta { get; set; }
    
        public PointSource(double rho=0.0, double phi=0.0, double theta=0.0) : this()
        {
            Phi = phi;
            Theta = theta;
            Rho = rho;
        }

        public double AngleCosBetweenVectors(double rho, double phi, double theta)
        {
            return Math.Cos(phi - Phi) * Math.Sin(theta) * Math.Sin(Theta) + Math.Cos(theta) * Math.Cos(Theta);
        }
        public double SquareDistanceFrom(double rho, double phi, double theta)
        {
            return Rho * Rho + rho * rho - 2 * Rho * rho * AngleCosBetweenVectors(rho, phi, theta);
        }
    }
}

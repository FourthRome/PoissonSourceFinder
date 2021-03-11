namespace Computation
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    // Class to abstract away sources' data from learning process
    public class SourceGroup
    {
        //------------------
        // Public properties
        //------------------

        public Point[] Sources { get; set; } // Sources' coordinates

        public int SourceAmount { get => Sources.Length; } // Amount of sources

        //-------------
        // Constructors
        //-------------
        public SourceGroup(Point[] sources = null)
        {
            Sources = sources;
        }

        //---------------
        // Public methods
        //---------------

        // Finds potential's normal derivative at given coordinates
        public double NormalDerivative(double rho, double phi, double theta) // TODO: check the safety of the code for arbitrary coordinates
        {
            double result = 0;
            for (int i = 0; i < SourceAmount; ++i)
            {
                double temp = Math.Pow(Sources[i].Rho, 2) - Math.Pow(rho, 2);
                temp /= Math.Pow(Sources[i].SquareDistanceFrom(rho, phi, theta), 1.5);
                result += temp;
            }

            return result / (4 * Math.PI * rho);
        }

        // Component of component of the loss function's gradient, 1/4
        private double CommonDerivativeComponent(double rho, double phi, double theta) // TODO: check the safety of the code for arbitrary coordinates
        {
            double result = 0.0;

            for (int i = 0; i < SourceAmount; ++i)
            {
                result += (Math.Pow(rho, 2) - Math.Pow(Sources[i].Rho, 2)) / Math.Pow(Sources[i].SquareDistanceFrom(rho, phi, theta), 1.5);
            }

            result /= 4 * Math.PI * rho;
            result += GroundTruthNormalDerivative(rho, phi, theta);
            // result *= Math.Sin(theta);  // COMMENTED OUT: probably a mistake
            return result;
        }
    }
}

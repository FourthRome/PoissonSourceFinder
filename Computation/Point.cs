namespace Computation
{
    using System;

    public class Point
    {
        //---------------------------------------------------
        // Public properties and their backing private fields
        //---------------------------------------------------
        public double X { get; set; }

        public double Y { get; set; }

        public double Z { get; set; }

        public double Rho { get => Math.Sqrt(SquareNorm()); }

        public double Phi { get => Math.Atan2(Y, X); }

        public double Theta { get => Math.Acos(Z / Rho); }

        //-------------
        // Constructors
        //-------------
        public Point(double x, double y, double z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public Point(Point other)
            : this(other.X, other.Y, other.Z)
        {
        }

        public Point(SphericalVector positionVec)
        {
            if (!positionVec.IsPositionVector)
            {
                throw new NotImplementedException("[INCORRECT USAGE] Point can only be initialized with a SphericalVector if it is position vector.");
            }

            OverwriteWithSphericalPositionVector(positionVec);
        }

        //---------------
        // Public methods
        //---------------
        public override string ToString()
        {
            return $"Carthesian: ({X},{Y},{Z}), Spherical: ({Rho}, {Phi}, {Theta})";
        }

        // Used in normalization and to get Rho
        public double SquareNorm()
        {
            return Math.Pow(X, 2) + Math.Pow(Y, 2) + Math.Pow(Z, 2);
        }

        // Used when we need to add a vector to the current point, but it is in spherical coordinates
        public void AddSphericalVector(SphericalVector vec)
        {
            double rho = Rho + vec.Rho;
            double phi = Phi + vec.Phi;
            double theta = Theta + vec.Theta;

            OverwriteWithSphericalPositionVector(new SphericalVector(rho, phi, theta, makePositionVector: true));
        }

        // Distance to any given point
        public double SquareDistanceFrom(double rho, double phi, double theta)
        {
            return (Rho * Rho) + (rho * rho) - (2 * Rho * rho * AngleCosBetweenVectors(phi, theta));
        }

        // Component of component of the loss function's gradient, 2/4
        private double RhoDerivativeComponent(double rho, double phi, double theta) // TODO: check the safety of the code for arbitrary coordinates
        {
            // Computation
            double result = (Math.Cos(phi - Phi) * Math.Sin(theta) * Math.Sin(Theta)) + (Math.Cos(theta) * Math.Cos(Theta));
            result = Rho - (rho * result);
            result *= Math.Pow(Rho, 2) - Math.Pow(rho, 2);
            result *= 3 / (2 * Math.PI * Math.Pow(SquareDistanceFrom(rho, phi, theta), 2.5));
            result += -Rho / (Math.PI * Math.Pow(SquareDistanceFrom(rho, phi, theta), 1.5));
            // result *= Radius;  // COMMENTED OUT: probably a mistake
            result /= rho;
            return result;
        }

        // Component of component of the loss function's gradient, 3/4
        private double PhiDerivativeComponent(double rho, double phi, double theta, int sourceNumber) // TODO: check the safety of the code for arbitrary coordinates
        {
            // Computation
            double result = (Math.Pow(rho, 2) - Math.Pow(Rho, 2)) * Rho;
            result *= Math.Sin(phi - Phi) * Math.Sin(theta) * Math.Sin(Theta);
            result /= Math.Pow(SquareDistanceFrom(rho, phi, theta), 2.5);
            result *= 3 / (2 * Math.PI); // COMMENTED OUT R^2: probably a mistake

            return result;
        }

        // Component of component of the loss function's gradient, 4/4
        private double ThetaDerivativeComponent(double rho, double phi, double theta, int sourceNumber) // TODO: check the safety of the code for arbitrary coordinates
        {
            // Computation
            double result = (Math.Pow(rho, 2) - Math.Pow(Rho, 2)) * Rho;
            result *= (Math.Cos(phi - Phi) * Math.Sin(theta) * Math.Cos(Theta)) - (Math.Cos(theta) * Math.Sin(Theta));
            result /= Math.Pow(SquareDistanceFrom(rho, phi, theta), 2.5);
            result *= 3 / (2 * Math.PI);  // COMMENTED OUT R^2: probably a mistake

            return result;
        }

        //----------------------
        // Public static methods
        //----------------------

        public static Point operator +(Point point, SphericalVector vec)
        {
            Point result = new Point(point);
            result.AddSphericalVector(vec);
            return result;
        }

        //----------------
        // Private methods
        //----------------
        // Part of distance's computation
        private double AngleCosBetweenVectors(double phi, double theta)
        {
            return (Math.Cos(phi - Phi) * Math.Sin(theta) * Math.Sin(Theta)) + (Math.Cos(theta) * Math.Cos(Theta));
        }

        // To update the Point's coordinates using position vector
        private void OverwriteWithSphericalPositionVector(SphericalVector vec)
        {
            if (!vec.IsPositionVector)
            {
                throw new NotImplementedException("[INCORRECT USAGE] If you want to make a SphericalVector a Point, make sure to normalize it with MakePositional first.");
            }

            X = vec.Rho * Math.Cos(vec.Phi) * Math.Sin(vec.Theta);
            Y = vec.Rho * Math.Sin(vec.Phi) * Math.Sin(vec.Theta);
            Z = vec.Rho * Math.Cos(vec.Theta);
        }
    }
}

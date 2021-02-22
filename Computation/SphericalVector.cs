namespace Computation
{
    using System;

    // "Spherical" is a notion of the class's usage, not the internal behaviour. It is generally a regular 3D-space vector by inner semantics.
    public class SphericalVector
    {
        //------------------
        // Public properties
        //------------------
        public double Rho { get; set; }

        public double Phi { get; set; }

        public double Theta { get; set; }

        public bool IsPositionVector { get; private set; } // Just to make sure the vector is used as expected

        //-------------
        // Constructors
        //-------------
        public SphericalVector(double rho, double phi, double theta, bool makePositionVector = false)
        {
            Rho = rho;
            Phi = phi;
            Theta = theta;
            IsPositionVector = makePositionVector;

            if (IsPositionVector)
            {
                MakePositional();
            }
        }

        //----------------------
        // Public static methods
        //----------------------
        // To avoid accumulation of rounding errors, I do not scale the vector in-place
        // (which would override the vector a lot in iterations), but rather get a new scaled one
        public static SphericalVector ScaledVersion(SphericalVector source, double scaleFactor)
        {
            if (source.IsPositionVector)
            {
                throw new NotSupportedException("[INCORRECT USAGE] Positional SphericalVector should only be used for conversion to Point.");
            }

            return new SphericalVector(source.Rho * scaleFactor, source.Phi * scaleFactor, source.Theta * scaleFactor);
        }

        public static SphericalVector operator -(SphericalVector vec)
        {
            if (vec.IsPositionVector)
            {
                throw new NotImplementedException("[INCORRECT USAGE] Positional SphericalVactor should not be negated or subtracted from anything.");
            }

            return new SphericalVector(-vec.Rho, -vec.Phi, -vec.Theta);
        }

        //---------------
        // Public methods
        //---------------
        public override string ToString()
        {
            return $"({Rho}, phi: {Phi}, theta: {Theta})";
        }

        // To return to standard rho >= 0, phi from 0 to 2Pi, theta from 0 to Pi
        public void MakePositional()
        {
            if (Rho < 0)
            {
                Rho = -Rho;
                Phi += Math.PI;
                Theta = Math.PI - Theta;
            }

            Phi %= 2 * Math.PI;
            if (Phi < 0)
            {
                Phi += 2 * Math.PI;
            }

            Theta %= 2 * Math.PI;
            if (Theta > Math.PI)
            {
                Theta = (2 * Math.PI) - Theta;
            }
            else if (Theta < -Math.PI)
            {
                Theta = (2 * Math.PI) + Theta;
            }
            else if (Theta < 0)
            {
                Theta += Math.PI;
            }
        }
    }
}

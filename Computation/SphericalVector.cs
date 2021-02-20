namespace Computation
{
    // "Spherical" is a notion of the class's usage, not the internal behaviour. It is generally a regular 3D-space vector by inner semantics.
    public class SphericalVector
    {
        public double Scale { get; set; } // Is used to scale the vector, because vector objects are to be often changed, and I don't want to lose info
                                           // due to cumulative calculation errors.

        public double Rho { get; set; }

        public double Phi { get; set; }

        public double Theta { get; set; }

        public SphericalVector(double rho, double phi, double theta, double scale = 1.0)
        {
            Rho = rho;
            Phi = phi;
            Theta = theta;
            Scale = scale;
        }
    }
}

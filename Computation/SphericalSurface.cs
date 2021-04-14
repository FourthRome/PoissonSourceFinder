namespace Computation
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    // Class to abstract away surface's details
    public class SphericalSurface
    {
        //------------------
        // Public properties
        //------------------
        public double Radius { get; set; } // Sphere's radius

        public List<(double, double)> AzimuthalRanges { get; set; } // Phis' range

        public List<(double, double)> PolarRanges { get; set; } //  Thetas' range

        //-------------
        // Constructors
        //-------------
        public SphericalSurface(
            double radius,
            List<(double, double)> azRanges = null,
            List<(double, double)> polRanges = null)
        {
            Radius = radius;
            AzimuthalRanges = azRanges;
            PolarRanges = polRanges;

            if (AzimuthalRanges == null)
            {
                AzimuthalRanges = new List<(double, double)>();
            }

            if (PolarRanges == null)
            {
                PolarRanges = new List<(double, double)>();
            }
        }

        //---------------
        // Public methods
        //---------------
        public void AddAzimuthalRange((double, double) range)
        {
            AzimuthalRanges.Add(range);
        }

        public void AddPolarRange((double, double) range)
        {
            PolarRanges.Add(range);
        }

        public void AddAzimuthalRange(double begin, double end)
        {
            AddAzimuthalRange((begin, end));
        }

        public void AddPolarRange(double begin, double end)
        {
            AddPolarRange((begin, end));
        }
    }
}

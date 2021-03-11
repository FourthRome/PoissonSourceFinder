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

        public List<Tuple<double, double>> AzimuthalRanges { get; set; } // Phis' range

        public List<Tuple<double, double>> PolarRanges { get; set; } //  Thetas' range

        public double AzimuthalStep { get; set; } // Hyperparameter: width of the grid cell for integral computation

        public double PolarStep { get; set; } // Hyperparameter: height of the grid cell for integral computation

        //-------------
        // Constructors
        //-------------
        public SphericalSurface(double radius, List<Tuple<double, double>> azRanges, List<Tuple<double, double>> polRanges)
        {
            Radius = radius;
            AzimuthalRanges = azRanges;
            PolarRanges = polRanges;
        }

        //---------------
        // Public methods
        //---------------

        //-----------------------------------------------------------
        // Integral computation methods, will possibly become private
        //-----------------------------------------------------------

        // General method for integrals' computation, sum of integrals over all areas
        private double IntegralOverSurface(Func<double, double, double, double> func)
        {
            double result = 0.0;
            foreach (var aziRange in AzimuthalRanges)
            {
                foreach (var polRange in PolarRanges)
                {
                    result += IntegralOverRectangularArea(func, aziRange, polRange);
                }
            }
            return result;
        }

        // General method for integrals' computation over one area
        private double IntegralOverRectangularArea(
            Func<double, double, double, double> func,
            Tuple<double, double> aziRange,
            Tuple<double, double> polRange)
        {
            // How many points should the grid consist of
            int aziCount = Convert.ToInt32(Math.Floor((aziRange.Item2 - aziRange.Item1) / AzimuthalStep));
            int polCount = Convert.ToInt32(Math.Floor((polRange.Item2 - polRange.Item1) / PolarStep));

            double result = 0.0;
            Task<double>[] tasks = new Task<double>[aziCount];
            for (int col = 0; col < aziCount; ++col)
            {
                tasks[col] = Task.Factory.StartNew(
                    objCol =>
                    {
                        int i = (int)objCol;
                        double localResult = 0.0;

                        for (int j = 0; j < polCount; ++j)
                        {
                            localResult += AzimuthalStep * Math.Pow(Radius, 2)
                            * (Math.Cos(polRange.Item1 + (j * PolarStep)) - Math.Cos(polRange.Item1 + ((j + 1) * PolarStep)))
                            * func(Radius, aziRange.Item1 + ((i + 0.5) * AzimuthalStep), polRange.Item1 + ((j + 0.5) * PolarStep));
                        }
                        return localResult;
                    },
                    col);
            }

            for (int col = 0; col < aziCount; ++col)
            {
                result += tasks[col].Result;
            }

            return result;
        }
    }
}

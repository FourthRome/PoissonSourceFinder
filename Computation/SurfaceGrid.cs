namespace Computation
{
    using System;
    using System.Collections.Generic;

    public class SurfaceGrid
    {
        //------------------
        // Public properties
        //------------------
        public double AzimuthalStep { get; private set; }

        public double PolarStep { get; private set; }

        public double[] Rows { get; private set; }

        public double[] Cols { get; private set; }

        public int RowsNumber { get; private set; }

        public int ColsNumber { get; private set; }

        public int NodesNumber { get; private set; }

        //-------------
        // Constructors
        //-------------
        public SurfaceGrid(double azimuthalStep, double polarStep, List<(double, double)> azimuthalRanges, List<(double, double)> polarRanges)
        {
            AzimuthalStep = azimuthalStep;
            PolarStep = polarStep;
            List<double> rows = new ();
            List<double> cols = new ();
            RowsNumber = 0;
            ColsNumber = 0;

            foreach (var range in azimuthalRanges)
            {
                int aziCount = Convert.ToInt32(Math.Floor((range.Item2 - range.Item1) / AzimuthalStep));
                ColsNumber += aziCount;
                for (int i = 0; i <= aziCount; ++i)
                {
                    cols.Add(range.Item1 + (i * AzimuthalStep));
                }
            }

            foreach (var range in polarRanges)
            {
                int polCount = Convert.ToInt32(Math.Floor((range.Item2 - range.Item1) / PolarStep));
                RowsNumber += polCount;
                for (int i = 0; i <= polCount; ++i)
                {
                    rows.Add(range.Item1 + (i * PolarStep));
                }
            }

            NodesNumber = RowsNumber * ColsNumber;
        }
    }
}

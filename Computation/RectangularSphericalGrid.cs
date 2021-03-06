﻿namespace Computation
{
    using System;
    using System.Collections.Generic;

    public class RectangularSphericalGrid : SphericalGrid
    {
        //------------------
        // Public properties
        //------------------
        public double AzimuthalStep { get; private set; }

        public double PolarStep { get; private set; }

        //-------------
        // Constructors
        //-------------
        public RectangularSphericalGrid(SphericalSurface surface, double azimuthalStep, double polarStep)
            : this(surface.Radius, surface.AzimuthalRanges, surface.PolarRanges, azimuthalStep, polarStep)
        {
        }

        public RectangularSphericalGrid(
            double radius,
            List<(double, double)> azimuthalRanges,
            List<(double, double)> polarRanges,
            double azimuthalStep,
            double polarStep)
            : base(radius)
        {
            AzimuthalStep = azimuthalStep;
            PolarStep = polarStep;
            ElementsNumber = 0;

            foreach (var aziRange in azimuthalRanges)
            {
                foreach (var polRange in polarRanges)
                {
                    double aziOffset = aziRange.Item1;
                    double polOffset = polRange.Item1;

                    int aziCount = Convert.ToInt32(Math.Floor((aziRange.Item2 - aziRange.Item1) / AzimuthalStep));
                    int polCount = Convert.ToInt32(Math.Floor((polRange.Item2 - polRange.Item1) / PolarStep));

                    ElementsNumber += aziCount * polCount;

                    for (int i = 0; i < aziCount; ++i)
                    {
                        for (int j = 0; j < polCount; ++j)
                        {
                            (double, double)[] borderNodes = new (double, double)[]
                            {
                                (aziOffset + (i * AzimuthalStep), polOffset + (j * PolarStep)),
                                (aziOffset + ((i + 1) * AzimuthalStep), polOffset + (j * PolarStep)),
                                (aziOffset + (i * AzimuthalStep), polOffset + ((j + 1) * PolarStep)),
                                (aziOffset + ((i + 1) * AzimuthalStep), polOffset + ((j + 1) * PolarStep)),
                            };

                            var element = new SphericalGridElement(SPHERICAL_GRID_TYPE.RECTANGULAR, borderNodes);
                            Elements.Add(element);
                            Square += SphericalGridElement.GetSquare(element);
                        }
                    }
                }
            }
        }
    }
}

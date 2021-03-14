namespace ConsoleTestApp
{
    using System;
    using System.Collections.Generic;
    using Computation;

    public class Program
    {
        public static void Main()
        {
            //--------------------------------------------------
            // Provide hyperparameters for the searching process
            //--------------------------------------------------
            double radius = 1.0;
            double azimuthalStep = 1e-2;
            double polarStep = 1e-2;
            double smallestRho = 0;
            double biggestRho = radius - 1e-2;
            double errorMargin = 1e-2;

            //--------------------
            // Set up real sources
            //--------------------
            SourceGroup groundTruth = new SourceGroup(new Point[]
            {
                (0.5, 0, 0),
                (0, 0.5, 0),
                (0, 0, 0.5),
            });

            //---------------------------------
            // Set up initial predicted sources
            //---------------------------------
            SourceGroup initialSources = new SourceGroup(new Point[]
            {
                (0.4, 0.1, 0.1),
                (0.2, 0.4, 0.3),
                (-0.3, -0.3, 0.3),
            });

            //--------------------------------------
            // Set up part of the sphere's surface S
            //--------------------------------------
            SphericalSurface surface = new SphericalSurface(radius, azimuthalStep, polarStep);

            // Full sphere
            surface.AddAzimuthalRange(0.0, 2 * Math.PI);
            surface.AddPolarRange(0, Math.PI);

            //// Hemisphere x > 0
            //surface.AddAzimuthalRange(0, Math.PI / 2);
            //surface.AddAzimuthalRange(3 * Math.PI / 2, 2 * Math.PI);
            //surface.AddPolarRange(0, Math.PI);

            //// Hemisphere x < 0
            //surface.AddAzimuthalRange(Math.PI / 2, 3 * Math.PI / 2);
            //surface.AddPolarRange(0, Math.PI);

            //// Hemisphere y > 0
            //surface.AddAzimuthalRange(0, Math.PI);
            //surface.AddPolarRange(0, Math.PI);

            //// Hemisphere y < 0
            //surface.AddAzimuthalRange(Math.PI, 2 * Math.PI);
            //surface.AddPolarRange(0, Math.PI);

            //// Hemisphere z > 0
            //surface.AddAzimuthalRange(0, 2 * Math.PI);
            //surface.AddPolarRange(0, Math.PI / 2);

            //// Hemisphere z < 0
            //surface.AddAzimuthalRange(0, 2 * Math.PI);
            //surface.AddPolarRange(Math.PI / 2, Math.PI);

            //// Eighth of a sphere x > 0, y > 0, z > 0
            //surface.AddAzimuthalRange(0, Math.PI / 2);
            //surface.AddPolarRange(0, Math.PI / 2);

            //// Eighth of a sphere x < 0, y < 0, z < 0
            //surface.AddAzimuthalRange(Math.PI, 3 * Math.PI / 2);
            //surface.AddPolarRange(Math.PI / 2, Math.PI);

            //-------------------------------
            // Set up model's hyperparameters
            //-------------------------------
            Model prediction = new Model(initialSources, surface, groundTruthNormalDerivative: groundTruth.NormalDerivative)
            {
                SmallestRho = smallestRho,
                BiggestRho = biggestRho,
                ErrorMargin = errorMargin,
            };

            //-------------------------
            // Start prediction process
            //-------------------------
            prediction.SearchForSources();

            //------------------
            // Print the results
            //------------------
            Console.WriteLine($"Final results:");
            Console.WriteLine($"Model's target value: {prediction.TargetFunction()}");

            int count = 0; // TODO: there was an analog to Python's .enumerate(); find it
            Console.WriteLine($"\nSources' calculated coordinates:\n");
            foreach (var source in prediction.Group.Sources)
            {
                Console.WriteLine($"Source {count}: {source}");
                Console.WriteLine();
                ++count;
            }

            count = 0;
            Console.WriteLine($"\nSources' real coordinates:\n");
            foreach (var source in groundTruth.Sources)
            {
                Console.WriteLine($"Source {count}: {source}");
                Console.WriteLine();
                ++count;
            }

            Console.ReadLine();
        }
    }
}

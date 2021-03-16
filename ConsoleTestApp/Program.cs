namespace ConsoleTestApp
{
    using System;
    using System.Collections.Generic;
    using Computation;
    using Contracts;

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
            SourceGroup groundTruth = new (new Point[]
            {
                (0.7, 0.7, 0),
                (0.8, -0.59, 0),
            });

            //---------------------------------
            // Set up initial predicted sources
            //---------------------------------
            SourceGroup initialSources = new (new Point[]
            {
                (0.3, 0.4, 0.1),
                (0.4, -0.4, -0.1),
            });

            //--------------------------------------
            // Set up part of the sphere's surface S
            //--------------------------------------
            SphericalSurface surface = new (radius, azimuthalStep, polarStep);

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
            Model model = new (initialSources, surface, groundTruthNormalDerivative: groundTruth.NormalDerivative)
            {
                SmallestRho = smallestRho,
                BiggestRho = biggestRho,
                ErrorMargin = errorMargin,
            };

            //------------------------
            // Register event handlers
            //------------------------
            model.ModelEvent += ModelEventCallback;

            //------------------------
            // Start inference process
            //------------------------
            model.SearchForSources();

            //------------------
            // Print the results
            //------------------
            Console.WriteLine($"Final results:");
            Console.WriteLine();
            Console.WriteLine($"Model's target value: {model.Score}");
            Console.WriteLine();

            Console.WriteLine($"Sources' real coordinates:");
            foreach (var source in groundTruth.Sources)
            {
                Console.WriteLine(source);
            }

            Console.WriteLine($"Sources' calculated coordinates:");
            foreach (var source in model.Group.Sources)
            {
                Console.WriteLine(source);
            }

            Console.ReadLine();
        }

        public static void ModelEventCallback(object sender, ModelEventArgs<Point> args)
        {
            Console.WriteLine(args.Message);
            if (args.Group != null)
            {
                foreach (var source in args.Group)
                {
                    Console.WriteLine(source);
                }
            }

            Console.WriteLine();
        }
    }
}

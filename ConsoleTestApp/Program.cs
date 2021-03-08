namespace ConsoleTestApp
{
    using System;
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
            double biggestRho = radius - 1e-3;
            double errorMargin = 1e-3;

            //--------------------
            // Set up real sources
            //--------------------
            Point[] sources = new Point[]
            {
                new Point(0.5, 0.5, 0),
                new Point(0.6, -0.6, 0),
                //new Point(0.7, 0.7, 0),
                //new Point(0.8, -0.59, 0),
            };
            Model groundTruth = new Model(radius, sources);

            //---------------------------------
            // Set up initial predicted sources
            //---------------------------------
            Point[] initialSources = new Point[]
            {
                new Point(0.3, 0.4, 0.1),
                new Point(0.4, -0.4, -0.1),
                //new Point(0.2, 0.3, 0.2),
                //new Point(0.3, -0.3, -0.2),
            };

            //-------------------------------
            // Set up model's hyperparameters
            //-------------------------------
            Model prediction = new Model(radius, initialSources, groundTruthNormalDerivative: groundTruth.NormalDerivative)
            {
                // TODO: separate learning process from info about sources
                AzimuthalStep = azimuthalStep,
                PolarStep = polarStep,
                SmallestRho = smallestRho,
                BiggestRho = biggestRho,
                ErrorMargin = errorMargin,
            };

            //--------------------------------------
            // Set up part of the sphere's surface S
            //--------------------------------------
            // Full sphere
            //prediction.AzimuthalRanges.Add(new Tuple<double, double>(0, 2 * Math.PI));
            //prediction.PolarRanges.Add(new Tuple<double, double>(0, Math.PI));

            // Hemisphere x > 0
            //prediction.AzimuthalRanges.Add(new Tuple<double, double>(0, Math.PI / 2));
            //prediction.AzimuthalRanges.Add(new Tuple<double, double>(3 * Math.PI / 2, 2 * Math.PI));
            //prediction.PolarRanges.Add(new Tuple<double, double>(0, Math.PI));

            // Hemisphere x < 0
            //prediction.AzimuthalRanges.Add(new Tuple<double, double>(Math.PI / 2, 3 * Math.PI / 2));
            //prediction.PolarRanges.Add(new Tuple<double, double>(0, Math.PI));

            // Hemisphere y > 0
            //prediction.AzimuthalRanges.Add(new Tuple<double, double>(0, Math.PI));
            //prediction.PolarRanges.Add(new Tuple<double, double>(0, Math.PI));

            // Hemisphere y < 0
            prediction.AzimuthalRanges.Add(new Tuple<double, double>(Math.PI, 2 * Math.PI));
            prediction.PolarRanges.Add(new Tuple<double, double>(0, Math.PI));

            // Hemisphere z > 0
            //prediction.AzimuthalRanges.Add(new Tuple<double, double>(0, 2 * Math.PI));
            //prediction.PolarRanges.Add(new Tuple<double, double>(0, Math.PI / 2));

            // Hemisphere z < 0
            //prediction.AzimuthalRanges.Add(new Tuple<double, double>(0, 2 * Math.PI));
            //prediction.PolarRanges.Add(new Tuple<double, double>(Math.PI / 2, Math.PI));

            // Eighth of a sphere x > 0, y > 0, z > 0
            //prediction.AzimuthalRanges.Add(new Tuple<double, double>(0, Math.PI / 2));
            //prediction.PolarRanges.Add(new Tuple<double, double>(0, Math.PI / 2));

            // Eighth of a sphere x < 0, y < 0, z < 0
            //prediction.AzimuthalRanges.Add(new Tuple<double, double>(Math.PI, 3 * Math.PI / 2));
            //prediction.PolarRanges.Add(new Tuple<double, double>(Math.PI / 2, Math.PI));

            //-------------------------
            // Start prediction process
            //-------------------------
            prediction.SearchForSources();

            //------------------
            // Print the results
            //------------------
            Console.WriteLine($"Final results:");
            Console.WriteLine($"Real target value: {groundTruth.TargetFunction()}, model's target value: {prediction.TargetFunction()}");

            int count = 0; // TODO: there was an analog to Python's .enumerate(); find it

            Console.WriteLine($"\nSources' calculated coordinates:\n");
            foreach (var source in prediction.Sources)
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

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
            double biggestRho = radius - 1e-2;
            double errorMargin = 1e-2;

            //--------------------
            // Set up real sources
            //--------------------
            Point[] sources = new Point[]
            {
                new Point(new SphericalVector(0.8, 0.0, Math.PI / 2, makePositionVector: true)),
            };
            Model groundTruth = new Model(radius, sources);

            //---------------------------------
            // Set up initial predicted sources
            //---------------------------------
            Point[] initialSources = new Point[]
            {
                new Point(new SphericalVector(0.5, 0.0, Math.PI / 2, makePositionVector: true)),
            };

            //-------------------------------
            // Set up model's hyperparameters
            //-------------------------------
            Model prediction = new Model(radius, initialSources, groundTruthNormalDerivative: groundTruth.NormalDerivative);
            // TODO: separate learning process from info about sources
            prediction.AzimuthalStep = azimuthalStep;
            prediction.PolarStep = polarStep;
            prediction.SmallestRho = smallestRho;
            prediction.BiggestRho = biggestRho;
            prediction.ErrorMargin = errorMargin;

            //--------------------------------------
            // Set up part of the sphere's surface S
            //--------------------------------------
            prediction.AzimuthalRanges.Add(new Tuple<double, double>(0, 2 * Math.PI));

            prediction.PolarRanges.Add(new Tuple<double, double>(Math.PI / 4, 3 * Math.PI / 4));

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

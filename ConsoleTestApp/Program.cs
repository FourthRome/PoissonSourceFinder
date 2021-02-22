namespace ConsoleTestApp
{
    using System;
    using Computation;

    public class Program
    {
        public static void Main()
        {
            //Point[] sources = new Point[]
            //{
            //    new Point(new SphericalVector(0.7, 2, Math.PI / 4, makePositionVector: true)),
            //    new Point(new SphericalVector(0.3, Math.PI, 3 * Math.PI / 8, makePositionVector: true)),
            //    new Point(new SphericalVector(0.5, 4, 0, makePositionVector: true)),
            //};
            //Model groundTruth = new Model(1.0, 3, sources);
            //Model prediction = new Model(1.0, 3, groundTruthNormalDerivative: groundTruth.NormalDerivative);
            //prediction.AzimuthalRanges.Add(new Tuple<double, double>(0, Math.PI));
            //prediction.PolarRanges.Add(new Tuple<double, double>(Math.PI / 2, Math.PI));
            //prediction.SearchForSources();

            Point[] sources = new Point[]
            {
                new Point(new SphericalVector(0.8, 0.0, Math.PI / 2, makePositionVector: true)),
            };
            Model groundTruth = new Model(1.0, 1, sources);
            Model prediction = new Model(1.0, 1, groundTruthNormalDerivative: groundTruth.NormalDerivative);
            prediction.AzimuthalRanges.Add(new Tuple<double, double>(Math.PI - 0.02, Math.PI + 0.02));
            prediction.PolarRanges.Add(new Tuple<double, double>(Math.PI / 4, 3 * Math.PI / 4));
            prediction.SearchForSources();

            Console.WriteLine($"\nFinal results:\n");
            int count = 0; // TODO: there was an analog to Python's .enumerate(); find it
            foreach (var source in prediction.Sources)
            {
                Console.WriteLine($"Source {count}'s coordinates are: {source}");
                Console.WriteLine();
                ++count;
            }

            count = 0;
            Console.WriteLine($"\nSources' real coordinates:\n");
            foreach (var source in groundTruth.Sources)
            {
                Console.WriteLine($"Source {count}'s coordinates are: {source}");
                Console.WriteLine();
                ++count;
            }

            Console.ReadLine();
        }
    }
}

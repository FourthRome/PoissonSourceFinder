using System;
using Computation;

namespace ConsoleTestApp
{
    class Program
    {
        static void Main(string[] args)
        {


            PointSource[] sources = new PointSource[] { new PointSource(0.5, 0.0, Math.PI / 4), new PointSource(0.3, Math.PI, 3 * Math.PI / 8) };
            Model groundTruth = new Model(1.0, 2, sources);
            Model prediction = new Model(1.0, 2, groundTruthNormalDerivative: groundTruth.NormalDerivative);
            prediction.AzimuthalRanges.Add(new Tuple<double, double>(0.0, 2 * Math.PI));
            prediction.PolarRanges.Add(new Tuple<double, double>(0.0, Math.PI));
            prediction.SearchForSources();

            //PointSource[] sources = new PointSource[] { new PointSource(0.8, 0.0, Math.PI / 2) };
            //Model groundTruth = new Model(1.0, 1, sources);
            //Model prediction = new Model(1.0, 1, groundTruthNormalDerivative: groundTruth.NormalDerivative);
            //prediction.AzimuthalRanges.Add(new Tuple<double, double>(Math.PI - 0.02, Math.PI + 0.02));
            //prediction.PolarRanges.Add(new Tuple<double, double>(Math.PI / 4, 3 * Math.PI / 4));
            //prediction.SearchForSources();


            Console.WriteLine($"\nFinal results:\n");
            int count = 0; // TODO: there was an analog to Python's .enumerate(); find it
            foreach (var source in prediction.Sources)
            {
                Console.WriteLine($"Source {count}'s coordinates are:");
                Console.WriteLine($"-rho: {source.Rho}");
                Console.WriteLine($"-phi: {source.Phi}");
                Console.WriteLine($"-theta: {source.Theta}");
                Console.WriteLine();
                ++count;
            }

            count = 0;
            Console.WriteLine($"\nSources' real coordinates:\n");
            foreach (var source in groundTruth.Sources)
            {
                Console.WriteLine($"Source {count}'s coordinates are:");
                Console.WriteLine($"-rho: {source.Rho}");
                Console.WriteLine($"-phi: {source.Phi}");
                Console.WriteLine($"-theta: {source.Theta}");
                Console.WriteLine();
                ++count;
            }

            Console.ReadLine();

        }
    }
}

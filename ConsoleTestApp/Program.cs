using System;
using Computation;

namespace ConsoleTestApp
{
    class Program
    {
        static void Main(string[] args)
        {


            PointSource[] sources = new PointSource[] { new PointSource(0.5, Math.PI / 2 + 0.01, Math.PI / 2), new PointSource(0.5, 3 * Math.PI / 2 + 0.01, Math.PI / 2) };
            Model groundTruth = new Model(1.0, 2, sources);
            Model prediction = new Model(1.0, 2, groundTruthNormalDerivative: groundTruth.NormalDerivative);
            prediction.AzimuthalRanges.Add(new Tuple<double, double>(0.0, 2 * Math.PI));
            prediction.PolarRanges.Add(new Tuple<double, double>(0.0, Math.PI));
            prediction.SearchForSources();

            //PointSource[] sources = new PointSource[] { new PointSource(0.5) };
            //Model groundTruth = new Model(1.0, 1, sources);
            //Model prediction = new Model(1.0, 1, groundTruthNormalDerivative: groundTruth.NormalDerivative);
            //prediction.AzimuthalRanges.Add(new Tuple<double, double>(0.0, 2 * Math.PI));
            //prediction.PolarRanges.Add(new Tuple<double, double>(0.0, Math.PI));
            //prediction.SearchForSources();

            Console.WriteLine(prediction.LocalLossFunction(0.0, 0.0));
            foreach (var source in prediction.Sources)
            {
                Console.WriteLine(source.Rho);
                Console.WriteLine(source.Phi);
                Console.WriteLine(source.Theta);
            }
            Console.ReadLine();

        }
    }
}

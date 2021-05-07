namespace Computation
{
    using System;
    using System.Collections.Generic;
    using LossFunctionExtensions;

    public class GroundTruth
    {
        //--------------------
        // Protected constants
        //--------------------
        protected const int TaskBatchSize = 100; // TODO: find a better way to abstract this

        //------------------
        // Public properties
        //------------------
        public Dictionary<(double, double), double> ErrorsOnElements { get; protected set; }

        public double Delta { get; protected set; }

        public double RelativeDelta { get; protected set; }

        public double ErrorFreeDerivativeNorm { get; protected set; }

        //---------------------
        // Protected properties
        //---------------------
        protected SourceGroup Group { get; set; }

        protected SphericalGrid Grid { get; set; }

        protected Dictionary<(double, double), double> CachedValues { get; set; }

        protected double[] ErrorsOnElementsArray { get; set; } // TODO: There must be a better way to store that info

        //-------------
        // Constructors
        //-------------
        public GroundTruth(SourceGroup group, SphericalGrid grid, double relativeDelta = 0.0)
        {
            Group = group;
            Grid = grid;
            RelativeDelta = relativeDelta;
            ErrorFreeDerivativeNorm = Math.Sqrt(group.TargetFunction(grid, (rho, phi, theta) => 0.0));
            Delta = RelativeDelta * ErrorFreeDerivativeNorm;
            PrepareErrors();
            CacheGroundTruth();
        }

        //---------------
        // Public methods
        //---------------
        public double CachedNormalDerivative(double rho, double phi, double theta)
        {
            // We ignore `rho` parameter as it is here to comply with the integration interface
            return CachedValues[(phi, theta)];
        }

        //------------------
        // Protected methods
        //------------------
        protected void PrepareErrors()
        {
            ErrorsOnElementsArray = new double[Grid.ElementsNumber];
            var rand = new Random();
            double norm = 0.0;

            for (int i = 0; i < ErrorsOnElementsArray.Length; ++i)
            {
                double temp = (rand.NextDouble() * 2) - 1.0;
                ErrorsOnElementsArray[i] = temp * Delta / Math.Sqrt(Grid.Elements[i].Square);
                norm += Math.Pow(temp, 2);
            }

            norm = Math.Sqrt(norm);
            for (int i = 0; i < ErrorsOnElementsArray.Length; ++i)
            {
                ErrorsOnElementsArray[i] /= norm;
            }
        }

        protected void CacheGroundTruth()
        {
            CachedValues = new Dictionary<(double, double), double>();
            ErrorsOnElements = new Dictionary<(double, double), double>();

            for (int i = 0; i < Grid.ElementsNumber; ++i)
            {
                (double, double) key = Grid.Elements[i].CentralNode;
                double value = Group.NormalDerivative(
                                Grid.Radius,
                                Grid.Elements[i].CentralNode.Item1,
                                Grid.Elements[i].CentralNode.Item2);
                value += ErrorsOnElementsArray[i];
                CachedValues.Add(key, value);
                ErrorsOnElements.Add(key, ErrorsOnElementsArray[i]);
            }
        }
    }
}

namespace Computation
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

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
        public GroundTruth(SourceGroup group, SphericalGrid grid, double delta = 0.0)
        {
            Group = group;
            Grid = grid;
            Delta = delta;
            PrepareErrors();
            CacheGroundTruth();
            ErrorsOnElements = new ();
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
                ErrorsOnElementsArray[i] = (rand.NextDouble() * 2) - 1;
                norm += Math.Pow(ErrorsOnElementsArray[i], 2.0);
            }

            norm = Math.Sqrt(norm);

            for (int i = 0; i < ErrorsOnElementsArray.Length; ++i)
            {
                ErrorsOnElementsArray[i] = ErrorsOnElementsArray[i] * Delta / norm;
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

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

        //---------------------
        // Protected properties
        //---------------------
        protected SourceGroup Group { get; set; }

        protected SphericalGrid Grid { get; set; }

        protected Dictionary<(double, double), double> CachedValues { get; set; }

        //-------------
        // Constructors
        //-------------
        public GroundTruth(SourceGroup group, SphericalGrid grid)
        {
            Group = group;
            Grid = grid;
            CacheGroundTruth();
            ErrorsOnElements = new ();
        }

        //------------------
        // Protected methods
        //------------------
        protected void CacheGroundTruth()
        {
            CachedValues = new Dictionary<(double, double), double>();

            for (int i = 0; i < Grid.ElementsNumber; ++i)
            {
                (double, double) key = Grid.Elements[i].CentralNode;
                double value = Group.NormalDerivative(
                                Grid.Radius,
                                Grid.Elements[i].CentralNode.Item1,
                                Grid.Elements[i].CentralNode.Item2);
                CachedValues.Add(key, value);
            }
        }
    }
}

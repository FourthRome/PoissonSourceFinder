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
        public double[] ErrorsOnElements { get; protected set; }

        //---------------------
        // Protected properties
        //---------------------
        protected SourceGroup Group { get; set; }

        protected SphericalGrid Grid { get; set; }

        protected double[] CachedValues { get; set; }

        //-------------
        // Constructors
        //-------------
        public GroundTruth(SourceGroup group, SphericalGrid grid)
        {
            Group = group;
            Grid = grid;
            CacheGroundTruth();
            ErrorsOnElements = new double[Grid.ElementsNumber];
        }

        //------------------
        // Protected methods
        //------------------
        protected void CacheGroundTruth()
        {
            CachedValues = new double[Grid.ElementsNumber];

            int taskNumber = Convert.ToInt32(Math.Ceiling((double)Grid.ElementsNumber / TaskBatchSize));
            Task[] tasks = new Task[taskNumber];

            for (int i = 0; i < taskNumber; ++i)
            {
                tasks[i] = Task.Factory.StartNew(
                    boxedTaskIdx =>
                    {
                        int offset = (int)boxedTaskIdx * TaskBatchSize;
                        int stopCondition = Math.Min(offset + TaskBatchSize, Grid.ElementsNumber);
                        for (int j = offset; j < stopCondition; ++j)
                        {
                            CachedValues[j] = Group.NormalDerivative(
                                Grid.Radius,
                                Grid.Elements[j].CentralNode.Item1,
                                Grid.Elements[j].CentralNode.Item2);
                        }
                    },
                    i);
            }

            Task.WaitAll(tasks);
        }
    }
}

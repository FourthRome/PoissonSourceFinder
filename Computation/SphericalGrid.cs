namespace Computation
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public abstract class SphericalGrid
    {
        //----------------------------------------------------
        // Constants TODO: should be abstracted somewhere else
        //----------------------------------------------------
        protected const int ElementsBatchSize = 100;

        //------------------
        // Public properties
        //------------------
        public List<SphericalGridElement> Elements { get; protected set; }

        public int ElementsNumber { get; protected set; }

        public double Radius { get; protected set; }

        //-------------
        // Constructors
        //-------------
        protected SphericalGrid(double radius)
        {
            Radius = radius;
            Elements = new ();
        }

        //---------------
        // Public methods
        //---------------
        public double Integrate(Func<double, double, double, double> func)
        {
            double result = 0.0;
            int tasksNumber = Convert.ToInt32(Math.Ceiling((double)ElementsNumber / ElementsBatchSize));
            Task<double>[] tasks = new Task<double>[tasksNumber];

            for (int i = 0; i < tasksNumber; ++i)
            {
                tasks[i] = Task.Factory.StartNew(
                    boxedTaskNumber =>
                    {
                        int offset = (int)boxedTaskNumber * ElementsBatchSize;
                        int stopCondition = Math.Min(offset + ElementsBatchSize, ElementsNumber);
                        double localResult = 0.0;

                        for (int j = offset; j < stopCondition; ++j)
                        {
                            localResult += Elements[j].Square * func(Radius, Elements[j].CentralNode.Item1, Elements[j].CentralNode.Item2);
                        }

                        return localResult;
                    },
                    i);
            }

            for (int i = 0; i < tasksNumber; ++i)
            {
                result += tasks[i].Result;
            }

            return result;
        }
    }
}

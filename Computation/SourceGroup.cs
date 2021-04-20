namespace Computation
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Text;

    using Contracts;

    // Class to abstract away sources' data from learning process
    public class SourceGroup : ISourceGroup<Point>, IEnumerable
    {
        //------------------
        // Public properties
        //------------------
        public Point[] Sources { get; set; } // Sources' coordinates TODO: make SourceGroup immutable

        public int SourceAmount { get => Sources.Length; }

        public double Noise { get; set; } // TODO: find a better way to distort function

        //-------------
        // Constructors
        //-------------
        public SourceGroup(Point[] sources = null, double noise = 0.0)
        {
            Sources = sources;
            Noise = noise;
        }

        public SourceGroup(SourceGroup group, double noise = 0.0)
        {
            Sources = new Point[group.SourceAmount];
            for (int i = 0; i < group.SourceAmount; ++i)
            {
                Sources[i] = group.Sources[i];
            }

            Noise = noise;
        }

        public SourceGroup(List<Point> list, double noise = 0.0)
        {
            Sources = list.ToArray(); // TODO: Check if it makes sense to make a copy here
            Noise = noise;
        }

        //---------------
        // Public methods
        //---------------

        // Finds potential's normal derivative at given coordinates
        public double NormalDerivative(double rho, double phi, double theta) // TODO: check the safety of the code for arbitrary coordinates
        {
            double result = 0;
            for (int i = 0; i < SourceAmount; ++i)
            {
                double temp = Math.Pow(Sources[i].Rho, 2) - Math.Pow(rho, 2);
                temp /= Math.Pow(Sources[i].SquareDistanceFrom(rho, phi, theta), 1.5);
                result += temp;
            }

            return (result / (4 * Math.PI * rho)) + Noise; //TODO: dind a better way to distort function
        }

        // IEnumerable details
        IEnumerator IEnumerable.GetEnumerator()
        {
            return (IEnumerator)GetEnumerator();
        }

        // IEnumerable details
        public SourceGroupEnum GetEnumerator()
        {
            return new SourceGroupEnum(Sources);
        }

        //----------------------
        // Public static methods
        //----------------------
        public static double DistanceBetween(SourceGroup a, SourceGroup b)
        {
            // TODO: probably make this function not just compare i-th elements, but check all the permutations
            double result = 0.0;
            for (int i = 0; i < a.SourceAmount; ++i)
            {
                result += (a.Sources[i] - b.Sources[i]).SquareNorm();
            }

            return Math.Sqrt(result);
        }
    }

    // IEnumerable details
    public class SourceGroupEnum : IEnumerator
    {
        private readonly Point[] sources;
        private int position = -1;

        public SourceGroupEnum(Point[] sources)
        {
            this.sources = sources;
        }

        public bool MoveNext()
        {
            position++;
            return position < sources.Length;
        }

        public void Reset()
        {
            position = -1;
        }

        object IEnumerator.Current
        {
            get
            {
                return Current;
            }
        }

        public Point Current
        {
            get
            {
                try
                {
                    return sources[position];
                }
                catch (IndexOutOfRangeException)
                {
                    throw new InvalidOperationException();
                }
            }
        }
    }
}

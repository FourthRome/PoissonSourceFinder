namespace Computation
{
    using System;
    using System.Collections.Generic;

    public enum SPHERICAL_GRID_TYPE
    {
        RECTANGULAR,
    }

    public class SphericalGridElement
    {
        //------------------
        // Public properties
        //------------------
        public double Square { get; protected set; }

        public SPHERICAL_GRID_TYPE Type { get; protected set; }

        public (double, double)[] BorderNodes { get; protected set; }

        //-------------
        // Constructors
        //-------------
        public SphericalGridElement(SPHERICAL_GRID_TYPE type, (double, double)[] borderNodes)
        {
            Type = type;
            BorderNodes = borderNodes;
            Square = GetSquare(this);
        }

        //-----------------------
        // Private static methods
        //-----------------------
        protected static double GetSquare(SphericalGridElement element)
        {
            double result = 0.0;
            if (element.Type == SPHERICAL_GRID_TYPE.RECTANGULAR)
            {
                double biggerTheta = element.BorderNodes[2].Item2;
                double smallerTheta = element.BorderNodes[0].Item2;
                double leftPhi = element.BorderNodes[0].Item1;
                double rightPhi = element.BorderNodes[1].Item1;
                if (rightPhi < leftPhi)
                {
                    rightPhi += 2 * Math.PI;
                }

                result = (rightPhi - leftPhi) * (Math.Cos(smallerTheta) - Math.Cos(biggerTheta));
            }

            return result;
        }
    }
}

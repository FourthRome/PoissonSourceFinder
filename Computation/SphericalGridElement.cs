﻿namespace Computation
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
        public SPHERICAL_GRID_TYPE Type { get; protected set; }

        public double Square { get; protected set; }

        public (double, double)[] BorderNodes { get; protected set; }

        public (double, double) CentralNode { get; protected set; }

        //---------------------
        // Protected properties
        //---------------------
        protected double Radius { get; set; }

        //-------------
        // Constructors
        //-------------
        public SphericalGridElement(SPHERICAL_GRID_TYPE type, (double, double)[] borderNodes, double radius = 1.0)
        {
            Type = type;
            BorderNodes = borderNodes;
            // TODO: check if there's a need for copying
            //BorderNodes = new (double, double)[borderNodes.Length];
            //Array.Copy(borderNodes, BorderNodes, borderNodes.Length);
            Radius = radius;
            Square = GetSquare(this);
            CentralNode = GetCentralNode(this);
        }

        //-----------------------
        // Private static methods
        //-----------------------
        public static double GetSquare(SphericalGridElement element)
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

                result = Math.Pow(element.Radius, 2) * (rightPhi - leftPhi) * (Math.Cos(smallerTheta) - Math.Cos(biggerTheta));
            }

            return result;
        }

        protected static (double, double) GetCentralNode(SphericalGridElement element)
        {
            (double, double) result = (0.0, 0.0);

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

                double middlePhi = (leftPhi + ((rightPhi - leftPhi) / 2)) % (2 * Math.PI);
                double middleTheta = smallerTheta + ((biggerTheta - smallerTheta) / 2);

                result = (middlePhi, middleTheta);
            }

            return result;
        }
    }
}

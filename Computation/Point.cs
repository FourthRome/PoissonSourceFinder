﻿namespace Computation
{
    using System;

    using Contracts;

    public class Point : IPoint
    {
        //---------------------------------------------------
        // Public properties and their backing private fields
        //---------------------------------------------------
        public double X { get; private set; } // TODO: Make Point immutable type

        public double Y { get; private set; }

        public double Z { get; private set; }

        public double Rho { get => Math.Sqrt(SquareNorm()); }

        public double Phi { get => Math.Atan2(Y, X); }

        public double Theta { get => Math.Acos(Z / Rho); }

        //-------------
        // Constructors
        //-------------
        public Point(double x, double y, double z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public Point(Point other)
            : this(other.X, other.Y, other.Z)
        {
        }

        public Point((double, double, double) coords)
            : this(coords.Item1, coords.Item2, coords.Item3)
        {
        }

        public Point(SphericalVector positionVec)
        {
            if (!positionVec.IsPositionVector)
            {
                throw new NotImplementedException("[INCORRECT USAGE] Point can only be initialized with a SphericalVector if it is position vector.");
            }

            OverwriteWithSphericalPositionVector(positionVec);
        }

        //---------------
        // Public methods
        //---------------
        public override string ToString()
        {
            return $"Carthesian: ({X},{Y},{Z}), Spherical: ({Rho}, {Phi}, {Theta})";
        }

        public string ToStringCartesian()
        {
            return $"{X} {Y} {Z}";
        }

        public string ToStringCartesianCsv()
        {
            return $"{X},{Y},{Z},";
        }

        // Used in normalization and to get Rho
        public double SquareNorm()
        {
            return Math.Pow(X, 2) + Math.Pow(Y, 2) + Math.Pow(Z, 2);
        }

        // Used when we need to add a vector to the current point, but it is in spherical coordinates
        public void AddSphericalVector(SphericalVector vec)
        {
            double rho = Rho + vec.Rho;
            double phi = Phi + vec.Phi;
            double theta = Theta + vec.Theta;

            OverwriteWithSphericalPositionVector(new SphericalVector(rho, phi, theta, makePositionVector: true));
        }

        // Distance to any given point
        public double SquareDistanceFrom(double rho, double phi, double theta)
        {
            return (Rho * Rho) + (rho * rho) - (2 * Rho * rho * AngleCosBetweenVectors(phi, theta));
        }

        //----------------------
        // Public static methods
        //----------------------

        public static Point operator +(Point point, SphericalVector vec)
        {
            Point result = new (point);
            result.AddSphericalVector(vec);
            return result;
        }

        public static Point operator -(Point a, Point b)
        {
            return new Point(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
        }

        // This is required for a simplified experiment's data format
        public static implicit operator Point((double, double, double) coords)
        {
            return new Point(coords);
        }

        //----------------
        // Private methods
        //----------------
        // Part of distance's computation
        public double AngleCosBetweenVectors(double phi, double theta)
        {
            return (Math.Cos(phi - Phi) * Math.Sin(theta) * Math.Sin(Theta)) + (Math.Cos(theta) * Math.Cos(Theta));
        }

        // To update the Point's coordinates using position vector
        private void OverwriteWithSphericalPositionVector(SphericalVector vec)
        {
            if (!vec.IsPositionVector)
            {
                throw new NotImplementedException("[INCORRECT USAGE] If you want to make a SphericalVector a Point, make sure to normalize it with MakePositional first.");
            }

            X = vec.Rho * Math.Cos(vec.Phi) * Math.Sin(vec.Theta);
            Y = vec.Rho * Math.Sin(vec.Phi) * Math.Sin(vec.Theta);
            Z = vec.Rho * Math.Cos(vec.Theta);
        }
    }
}

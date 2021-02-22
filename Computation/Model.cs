namespace Computation
{
    using System;
    using System.Collections.Generic;

    // Class to hold all the info related to a single computational experiment
    public class Model
    {
        //------------------
        // Public properties
        //------------------

        public Point[] Sources { get; set; } // Candidate sources' coordinates

        public int SourceAmount { get; set; } // Amount of sources

        public double Radius { get; set; } // Sphere's radius

        public double BiggestRho { get; set; } // Upper boundary for sources' coordinates

        public double SmallestRho { get; set; } // Lower boundary for sources' coordinates

        public List<Tuple<double, double>> AzimuthalRanges { get; set; } // Part of the sphere surface we'll be processing

        public List<Tuple<double, double>> PolarRanges { get; set; } // Same here

        public Func<double, double, double> GroundTruthNormalDerivative { get; set; } // Loss function uses it

        public double AzimuthalStep { get; set; } // Hyperparameter: width of the grid cell for integral computation

        public double PolarStep { get; set; } // Hyperparameter: height of the grid cell for integral computation

        public double ErrorMargin { get; set; } // When to stop minimizing loss function; also the lower boundary for sources' coordinates TODO: use a separate member for the latter (maybe)

        //-------------
        // Constructors
        //-------------
        public Model(double radius, Point[] sources = null, int sourceAmount = 1, Func<double, double, double> groundTruthNormalDerivative = null)
        {
            // Plain old initialization
            Radius = radius;
            SourceAmount = sourceAmount;
            GroundTruthNormalDerivative = groundTruthNormalDerivative;
            AzimuthalRanges = new List<Tuple<double, double>>();
            PolarRanges = new List<Tuple<double, double>>();

            // TODO: make initialization of all members obligatory

            // Sources initialization
            if (sources != null)
            {
                Sources = sources;
                SourceAmount = sources.Length;
            }
            else
            {
                Sources = new Point[SourceAmount];
                for (int i = 0; i < SourceAmount; ++i)
                {
                    // Outside of the smallest suitable sphere, and spread apart horizontally
                    Sources[i] = new Point(new SphericalVector(Radius / 2, i * 2 * Math.PI / SourceAmount, Math.PI / 2, makePositionVector: true));
                }
            }
        }

        //---------------
        // Public methods
        //---------------

        //---------------------------------------
        // Current state statistics, to be public
        //---------------------------------------

        // Finds potential's normal derivative at given coordinates
        public double NormalDerivative(double phi, double theta)
        {
            double result = 0;
            for (int i = 0; i < SourceAmount; ++i)
            {
                double temp = Math.Pow(Sources[i].Rho, 2) - Math.Pow(Radius, 2);
                temp /= Math.Pow(Sources[i].SquareDistanceFrom(Radius, phi, theta), 1.5);
                result += temp;
            }

            return result / (4 * Math.PI * Radius);
        }

        //--------------------------------------------------------------------------------------------------------
        // Minimization problem's internals, possibly to become private (except for the "SearchForSources" and "TargetFunction" methods)
        //--------------------------------------------------------------------------------------------------------

        // Loss function, an integral, to be minimized
        public double TargetFunction()
        {
            return IntegralOverSurface(LocalLossFunction);
        }

        // Essentials of the loss function, to be integrated
        public double LocalLossFunction(double phi, double theta, int sourceNumber = -1) // TODO: find a way to avoid this dirty fictional parameter hack
        {
            return Math.Pow(GroundTruthNormalDerivative(phi, theta) - NormalDerivative(phi, theta), 2) * Math.Pow(Radius, 2) * Math.Sin(theta);
        }

        // The main method, finding the sources, including all stages
        public void SearchForSources()
        {
            double descentRate = 1.0;  // Hyperparameter: how fast should we descend TODO: descent logic should be revised
            int stepCount = 1;  // Statistics for the log

            // Declare necessary data structures
            Point[] oldSources = new Point[SourceAmount];
            SphericalVector[] proposedMove = new SphericalVector[SourceAmount];

            while (TargetFunction() > ErrorMargin)
            {
                // Here goes a single step of the gradient descent

                // Diagnostic output
                Console.WriteLine($"________________________________________Starting step {stepCount}________________________________________");

                // Backup old sources
                for (int i = 0; i < SourceAmount; ++i)
                {
                    oldSources[i] = Sources[i];
                }

                // Diagnostic output
                for (int i = 0; i < SourceAmount; ++i)
                {
                    Console.WriteLine($"\nSource {i}'s coordinates before the step: {Sources[i]}");
                }

                // Coefficient for gradient normalization
                // double normalizer = 0.0;

                // Compute the steps towards the antigradient
                for (int i = 0; i < SourceAmount; ++i)
                {
                    proposedMove[i] = new SphericalVector(
                        -descentRate * GradComponentRho(i),
                        -descentRate * GradComponentPhi(i),
                        -descentRate * GradComponentTheta(i));

                    // normalizer += proposedMove[i].SquareNorm();

                    // Diagnostic output
                    Console.WriteLine($"\nStep's initial components for source {i} before normalization are {proposedMove[i]}");
                }

                // Make the largest step that improves quality
                // Initial step
                double oldTargetValue = TargetFunction();
                for (int i = 0; i < SourceAmount; ++i)
                {
                    Sources[i] = oldSources[i] + SphericalVector.ScaledVersion(proposedMove[i], 1.0);

                    // Diagnostic output
                    Console.WriteLine($"\nSource {i}'s coordinates after initial step: {Sources[i]}");
                }

                // Diagnostic output
                Console.WriteLine($"\n________________________________________Starting step reduction________________________________________");

                int reductionCount = 0;
                double stepFraction = 0.5;  // If the step will not actually minimize loss, we halve it and try again
                while (CoordinatesOutOfBorders() || TargetFunction() > oldTargetValue)
                {
                    reductionCount += 1;
                    // Diagnostic output
                    Console.WriteLine($"________________________________________Reduction for step {stepCount}: {reductionCount}________________________________________");
                    Console.WriteLine($"Was out of borders: {CoordinatesOutOfBorders()}, old target value: {oldTargetValue}, new target value: {TargetFunction()}");

                    for (int i = 0; i < SourceAmount; ++i)
                    {
                        Sources[i] = oldSources[i] + SphericalVector.ScaledVersion(proposedMove[i], stepFraction);
                        stepFraction *= 0.5;

                        // Diagnostic output
                        Console.WriteLine($"Now trying coordinates for source {i}: {Sources[i]}");
                    }
                }

                // Diagnostic output
                Console.WriteLine($"\n________________________________________Final values for step {stepCount} after {reductionCount} reductions________________________________________");
                Console.WriteLine($"Old target value: {oldTargetValue}, new target value: {TargetFunction()}");

                for (int i = 0; i < SourceAmount; ++i)
                {
                    Console.WriteLine($"Source {i}'s coordinates: {Sources[i]}");
                }

                // Diagnostic output
                Console.WriteLine();

                // Update statistics
                stepCount += 1;
            }
        }

        // Method to check that the sources' coordinates are within reasonable limits WITHOUT CHANGING THEM
        private bool CoordinatesOutOfBorders()
        {
            for (int i = 0; i < SourceAmount; ++i)
            {
                // Rho should lie between internal and external spheres' radiuses (the lower boundary is needed because of the gradient's properties)
                if (Sources[i].Rho > BiggestRho || Sources[i].Rho < SmallestRho)
                {
                    return true;
                }

                // Theta should lie within [0; Pi] segment, but, unlike phi, does not form circular trajectory, we should account for that
                if (Sources[i].Theta < 0 || Sources[i].Theta > Math.PI)
                {
                    return true;
                }
            }

            return false;
        }

        //-----------------------------------------------------------
        // Gradient computation specifics, possibly to become private
        //-----------------------------------------------------------

        // Component of the loss function's gradient, an integral, 1/3
        private double GradComponentRho(int sourceNumber)
        {
            return IntegralOverSurface(LocalGradComponentRho, sourceNumber);
        }

        // Component of the loss function's gradient, an integral, 2/3
        private double GradComponentPhi(int sourceNumber)
        {
            return IntegralOverSurface(LocalGradComponentPhi, sourceNumber);
        }

        // Component of the loss function's gradient, an integral, 3/3
        private double GradComponentTheta(int sourceNumber)
        {
            return IntegralOverSurface(LocalGradComponentTheta, sourceNumber);
        }

        // Component of the loss function's gradient, to be integrated, 1/3
        private double LocalGradComponentRho(double phi, double theta, int sourceNumber)
        {
            return CommonDerivativeComponent(phi, theta) * RhoDerivativeComponent(phi, theta, sourceNumber);
        }

        // Component of the loss function's gradient, to be integrated, 2/3
        private double LocalGradComponentPhi(double phi, double theta, int sourceNumber)
        {
            return CommonDerivativeComponent(phi, theta) * PhiDerivativeComponent(phi, theta, sourceNumber);
        }

        // Component of the loss function's gradient, to be integrated, 3/3
        private double LocalGradComponentTheta(double phi, double theta, int sourceNumber)
        {
            return CommonDerivativeComponent(phi, theta) * ThetaDerivativeComponent(phi, theta, sourceNumber);
        }

        // Component of component of the loss function's gradient, 1/4
        private double CommonDerivativeComponent(double phi, double theta)
        {
            double result = 0.0;

            for (int i = 0; i < SourceAmount; ++i)
            {
                result += (Math.Pow(Radius, 2) - Math.Pow(Sources[i].Rho, 2)) / Math.Pow(Sources[i].SquareDistanceFrom(Radius, phi, theta), 1.5);
            }

            result /= 4 * Math.PI * Radius;
            result += GroundTruthNormalDerivative(phi, theta);
            // result *= Math.Sin(theta);  // COMMENTED OUT: probably a mistake
            return result;
        }

        // Component of component of the loss function's gradient, 2/4
        private double RhoDerivativeComponent(double phi, double theta, int sourceNumber)
        {
            // Shortcuts
            Point source_i = Sources[sourceNumber];
            double rho_i = Sources[sourceNumber].Rho;
            double phi_i = Sources[sourceNumber].Phi;
            double theta_i = Sources[sourceNumber].Theta;

            // Computation
            double result = (Math.Cos(phi - phi_i) * Math.Sin(theta) * Math.Sin(theta_i)) + (Math.Cos(theta) * Math.Cos(theta_i));
            result = rho_i - (Radius * result);
            result *= Math.Pow(rho_i, 2) - Math.Pow(Radius, 2);
            result *= 3 / (2 * Math.PI * Math.Pow(source_i.SquareDistanceFrom(Radius, phi, theta), 2.5));
            result += -rho_i / (Math.PI * Math.Pow(source_i.SquareDistanceFrom(Radius, phi, theta), 1.5));
            // result *= Radius;  // COMMENTED OUT: probably a mistake
            result /= Radius;
            return result;
        }

        // Component of component of the loss function's gradient, 3/4
        private double PhiDerivativeComponent(double phi, double theta, int sourceNumber)
        {
            // Shortcuts
            Point source_i = Sources[sourceNumber];
            double rho_i = Sources[sourceNumber].Rho;
            double phi_i = Sources[sourceNumber].Phi;
            double theta_i = Sources[sourceNumber].Theta;

            // Computation
            double result = (Math.Pow(Radius, 2) - Math.Pow(rho_i, 2)) * rho_i;
            result *= Math.Sin(phi - phi_i) * Math.Sin(theta) * Math.Sin(theta_i);
            result /= Math.Pow(source_i.SquareDistanceFrom(Radius, phi, theta), 2.5);
            result *= 3 / (2 * Math.PI); // COMMENTED OUT R^2: probably a mistake

            return result;
        }

        // Component of component of the loss function's gradient, 4/4
        private double ThetaDerivativeComponent(double phi, double theta, int sourceNumber)
        {
            // Shortcuts
            Point source_i = Sources[sourceNumber];
            double rho_i = Sources[sourceNumber].Rho;
            double phi_i = Sources[sourceNumber].Phi;
            double theta_i = Sources[sourceNumber].Theta;

            // Computation
            double result = (Math.Pow(Radius, 2) - Math.Pow(rho_i, 2)) * rho_i;
            result *= (Math.Cos(phi - phi_i) * Math.Sin(theta) * Math.Cos(theta_i)) - (Math.Cos(theta) * Math.Sin(theta_i));
            result /= Math.Pow(source_i.SquareDistanceFrom(Radius, phi, theta), 2.5);
            result *= 3 / (2 * Math.PI);  // COMMENTED OUT R^2: probably a mistake

            return result;
        }

        //---------------------------------------------------------
        // Integral computation methods, possibly to become private
        //---------------------------------------------------------

        // General method for integrals' computation, sum of integrals over all areas
        private double IntegralOverSurface(Func<double, double, int, double> func, int sourceNumber = -1)
        {
            double result = 0.0;
            foreach (var aziRange in AzimuthalRanges)
            {
                foreach (var polRange in PolarRanges)
                {
                    result += IntegralOverRectangularArea(func, aziRange, polRange, sourceNumber);
                }
            }

            return result;
        }

        // General method for integrals' computation over one area
        private double IntegralOverRectangularArea(Func<double, double, int, double> func, Tuple<double, double> aziRange, Tuple<double, double> polRange, int sourceNumber = -1)
        {
            // How many points should the grid consist of
            int aziCount = Convert.ToInt32(Math.Floor((aziRange.Item2 - aziRange.Item1) / AzimuthalStep));
            int polCount = Convert.ToInt32(Math.Floor((polRange.Item2 - polRange.Item1) / PolarStep));

            double result = 0.0;
            for (int i = 0; i < aziCount; ++i)
            {
                for (int j = 0; j < polCount; ++j)
                {
                    result += AzimuthalStep * Math.Pow(Radius, 2) * (Math.Cos(polRange.Item1 + (j * PolarStep)) - Math.Cos(polRange.Item1 + ((j + 1) * PolarStep)))
                        * func(aziRange.Item1 + ((i + 0.5) * AzimuthalStep), polRange.Item1 + ((j + 0.5) * PolarStep), sourceNumber);
                }
            }

            return result;
        }
    }
}

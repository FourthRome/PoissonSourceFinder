namespace Computation
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    // Class to hold all the info related to a single computational experiment
    public class Model
    {
        //------------------
        // Public properties
        //------------------
        public double BiggestRho { get; set; } // Upper boundary for sources' coordinates

        public double SmallestRho { get; set; } // Lower boundary for sources' coordinates

        public Func<double, double, double> GroundTruthNormalDerivative { get; set; } // Loss function uses it

        public double ErrorMargin { get; set; } // When to stop minimizing loss function; also the lower boundary for sources' coordinates TODO: use a separate member for the latter (maybe)

        //-------------
        // Constructors
        //-------------
        public Model(Point[] sources = null, Func<double, double, double> groundTruthNormalDerivative)
        {
            // Plain old initialization
            GroundTruthNormalDerivative = groundTruthNormalDerivative;

            // TODO: make initialization of all members obligatory

            // Sources initialization
            if (sources != null)
            {
                Sources = sources;
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

                // Make the largest step that improves quality  // TODO: it's not the largest yet
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

                    if (!CoordinatesOutOfBorders() && reductionCount > 20)
                    {
                        // Diagnostic output
                        Console.WriteLine($"#################### Stopping reduction: too many steps #########################");
                        break;
                    }

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
        private double LocalGradComponentRho(double rho, double phi, double theta, int sourceNumber)
        {
            return CommonDerivativeComponent(rho, phi, theta) * RhoDerivativeComponent(rho, phi, theta, sourceNumber);
        }

        // Component of the loss function's gradient, to be integrated, 2/3
        private double LocalGradComponentPhi(double rho, double phi, double theta, int sourceNumber)
        {
            return CommonDerivativeComponent(rho, phi, theta) * PhiDerivativeComponent(rho, phi, theta, sourceNumber);
        }

        // Component of the loss function's gradient, to be integrated, 3/3
        private double LocalGradComponentTheta(double rho, double phi, double theta, int sourceNumber)
        {
            return CommonDerivativeComponent(rho, phi, theta) * ThetaDerivativeComponent(rho, phi, theta, sourceNumber);
        }

        // Component of component of the loss function's gradient, 1/4
        private double CommonDerivativeComponent(double rho, double phi, double theta) // TODO: check the safety of the code for arbitrary coordinates
        {
            double result = 0.0;

            for (int i = 0; i < SourceAmount; ++i)
            {
                result += (Math.Pow(rho, 2) - Math.Pow(Sources[i].Rho, 2)) / Math.Pow(Sources[i].SquareDistanceFrom(rho, phi, theta), 1.5);
            }

            result /= 4 * Math.PI * rho;
            result += GroundTruthNormalDerivative(phi, theta);
            // result *= Math.Sin(theta);  // COMMENTED OUT: probably a mistake
            return result;
        }
    }
}

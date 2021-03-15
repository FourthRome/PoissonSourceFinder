namespace Computation
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using LossFunctionExtensions;

    // Class to hold all the info related to a single computational experiment
    public class Model
    {
        //------------------
        // Public properties
        //------------------
        public double BiggestRho { get; set; } // Upper boundary for sources' coordinates

        public double SmallestRho { get; set; } // Lower boundary for sources' coordinates

        public Func<double, double, double, double> GroundTruthNormalDerivative { get; set; } // Loss function uses it

        public SphericalSurface Surface { get; set; }

        public SourceGroup Group { get; set; }

        public double ErrorMargin { get; set; } // When to stop minimizing loss function; also the lower boundary for sources' coordinates TODO: use a separate member for the latter (maybe)

        //-------------
        // Constructors
        //-------------
        public Model(SourceGroup group, SphericalSurface surface, Func<double, double, double, double> groundTruthNormalDerivative)
        {
            // Plain old initialization
            Group = group;
            Surface = surface;
            GroundTruthNormalDerivative = groundTruthNormalDerivative;
        }

        //---------------
        // Public methods
        //---------------

        public void FindInitialSources()
        {

        }

        // A shortcut for target function based on current model state
        public double TargetFunction()
        {
            return Group.TargetFunction(Surface, GroundTruthNormalDerivative);
        }

        // The main method, finding the sources, including all stages
        public void SearchForSources()
        {
            double descentRate = 1.0;  // Hyperparameter: how fast should we descend TODO: descent logic should be revised
            int stepCount = 1;  // Statistics for the log
            int sourceAmount = Group.SourceAmount; // Just an alias

            // Declare necessary data structures
            SourceGroup oldSources;
            SphericalVector[] proposedMove = new SphericalVector[sourceAmount];

            while (TargetFunction() > ErrorMargin)
            {
                // Here goes a single step of the gradient descent

                // Diagnostic output
                Console.WriteLine($"________________________________________Starting step {stepCount}________________________________________");

                // Backup old sources
                oldSources = new SourceGroup(Group);

                // Diagnostic output
                for (int i = 0; i < sourceAmount; ++i)
                {
                    Console.WriteLine($"\nSource {i}'s coordinates before the step: {Group.Sources[i]}");
                }

                // Coefficient for gradient normalization
                // double normalizer = 0.0;

                // Compute the steps towards the antigradient
                for (int i = 0; i < sourceAmount; ++i)
                {
                    proposedMove[i] = new SphericalVector(
                        -descentRate * Group.GradComponentRho(Surface, GroundTruthNormalDerivative, i),
                        -descentRate * Group.GradComponentPhi(Surface, GroundTruthNormalDerivative, i),
                        -descentRate * Group.GradComponentTheta(Surface, GroundTruthNormalDerivative, i));

                    // normalizer += proposedMove[i].SquareNorm();

                    // Diagnostic output
                    Console.WriteLine($"\nStep's initial components for source {i} before normalization are {proposedMove[i]}");
                }

                // Make the largest step that improves quality  // TODO: it's not the largest yet
                // Initial step
                double oldTargetValue = TargetFunction();
                for (int i = 0; i < sourceAmount; ++i)
                {
                    Group.Sources[i] = oldSources.Sources[i] + SphericalVector.ScaledVersion(proposedMove[i], 1.0);

                    // Diagnostic output
                    Console.WriteLine($"\nSource {i}'s coordinates after initial step: {Group.Sources[i]}");
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

                    for (int i = 0; i < sourceAmount; ++i)
                    {
                        Group.Sources[i] = oldSources.Sources[i] + SphericalVector.ScaledVersion(proposedMove[i], stepFraction);
                        stepFraction *= 0.5;

                        // Diagnostic output
                        Console.WriteLine($"Now trying coordinates for source {i}: {Group.Sources[i]}");
                    }
                }

                // Diagnostic output
                Console.WriteLine($"\n________________________________________Final values for step {stepCount} after {reductionCount} reductions________________________________________");
                Console.WriteLine($"Old target value: {oldTargetValue}, new target value: {TargetFunction()}");

                for (int i = 0; i < sourceAmount; ++i)
                {
                    Console.WriteLine($"Source {i}'s coordinates: {Group.Sources[i]}");
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
            foreach (var source in Group.Sources)
            {
                // Rho should lie between internal and external spheres' radiuses (the lower boundary is needed because of the gradient's properties)
                if (source.Rho > BiggestRho || source.Rho < SmallestRho)
                {
                    return true;
                }

                // Theta should lie within [0; Pi] segment, but, unlike phi, does not form circular trajectory, we should account for that
                if (source.Theta < 0 || source.Theta > Math.PI)
                {
                    return true;
                }
            }

            return false;
        }
    }
}

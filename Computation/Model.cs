namespace Computation
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Contracts;
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

        public int SourceAmount { get => Group.SourceAmount; }

        public double ErrorMargin { get; set; } // When to stop minimizing loss function; also the lower boundary for sources' coordinates TODO: use a separate member for the latter (maybe)

        // Event with info about inference process
        public delegate void ModelEventHandler(object sender, ModelEventArgs<Point> args);

        public event ModelEventHandler ModelEvent;

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

        public SphericalVector[] ComputeProposedMove()
        {
            SphericalVector[] result = new SphericalVector[Group.SourceAmount];
            for (int i = 0; i < Group.SourceAmount; ++i)
            {
                result[i] = new SphericalVector(
                    -Group.GradComponentRho(Surface, GroundTruthNormalDerivative, i),
                    -Group.GradComponentPhi(Surface, GroundTruthNormalDerivative, i),
                    -Group.GradComponentTheta(Surface, GroundTruthNormalDerivative, i));
            }

            return result;
        }

        public void InvokeModelEvent(string message, SourceGroup group = null)
        {
            OnModelEvent(new ModelEventArgs<Point>(message, group));
        }

        // The main method, finding the sources, including all stages
        public void SearchForSources()
        {
            int stepCount = 1;  // Statistics for the log
            int sourceAmount = Group.SourceAmount; // Just an alias

            // Declare necessary data structures
            SourceGroup oldSources;
            SphericalVector[] proposedMove;

            while (TargetFunction() > ErrorMargin)
            {
                // Here goes a single step of the gradient descent

                InvokeModelEvent($"Starting step {stepCount}"); // Output

                oldSources = new SourceGroup(Group); // Backup old sources

                InvokeModelEvent($"Sources coordinates before the step"); // Output
                InvokeModelEvent($"Coordinates", Group);

                proposedMove = ComputeProposedMove(); // Compute the steps towards the antigradient

                // Make the largest step that improves quality  // TODO: it's not the largest yet
                // Initial step
                double oldTargetValue = TargetFunction();
                for (int i = 0; i < sourceAmount; ++i)
                {
                    Group.Sources[i] = oldSources.Sources[i] + SphericalVector.ScaledVersion(proposedMove[i], 1.0);
                }

                InvokeModelEvent($"Starting step reduction"); // Output

                int reductionCount = 0;
                double stepFraction = 0.5;  // If the step will not actually minimize loss, we halve it and try again
                while (CoordinatesOutOfBorders() || TargetFunction() > oldTargetValue)
                {
                    reductionCount += 1;

                    if (!CoordinatesOutOfBorders() && reductionCount > 20)
                    {
                        InvokeModelEvent($"Stopping reduction: too many steps"); // Output
                        break;
                    }

                    for (int i = 0; i < sourceAmount; ++i)
                    {
                        Group.Sources[i] = oldSources.Sources[i] + SphericalVector.ScaledVersion(proposedMove[i], stepFraction);
                        stepFraction *= 0.5;
                    }
                }

                InvokeModelEvent($"Final values for step {stepCount} after {reductionCount} reductions"); // Output
                InvokeModelEvent($"Old target value: {oldTargetValue}, new target value: {TargetFunction()}");
                InvokeModelEvent("Coordinates", Group);

                stepCount += 1;  // Update statistics
            }
        }

        //------------------------------
        // Protected and private methods
        //------------------------------
        protected virtual void OnModelEvent(ModelEventArgs<Point> e)
        {
            ModelEventHandler handler = ModelEvent;
            handler?.Invoke(this, e);
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

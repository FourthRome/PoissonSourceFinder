namespace Computation
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Combinatorics.Collections;

    using Contracts;
    using LossFunctionExtensions;

    // Class to hold all the info related to a single computational experiment
    public class Model
    {
        //------------------
        // Public properties
        //------------------

        public SourceGroup Group { get; set; }

        public int SourceAmount { get => Group.SourceAmount; }

        public SphericalSurface Surface { get; set; }

        public Func<double, double, double, double> GroundTruthNormalDerivative { get; set; } // Loss function uses it

        public double SmallestRho { get; set; } // Lower boundary for sources' coordinates

        public double BiggestRho { get; set; } // Upper boundary for sources' coordinates

        public double ErrorMargin { get; set; } // When to stop minimizing loss function

        public double Score { get; private set; }

        // Event with info about inference process
        public delegate void ModelEventHandler(object sender, ModelEventArgs<Point> args);

        public event ModelEventHandler ModelEvent;

        //-------------
        // Constructors
        //-------------
        public Model(
            SphericalSurface surface,
            Func<double, double, double, double> groundTruthNormalDerivative,
            double smallestRho,
            double biggestRho,
            double errorMargin,
            SourceGroup startingGroup)
        {
            // Plain old initialization
            Surface = surface;
            GroundTruthNormalDerivative = groundTruthNormalDerivative;
            SmallestRho = smallestRho;
            BiggestRho = biggestRho;
            ErrorMargin = errorMargin;

            // Initialize sources and Score
            Group = startingGroup;
            Score = TargetFunction(Group);
        }

        public Model(
            SphericalSurface surface,
            Func<double, double, double, double> groundTruthNormalDerivative,
            double smallestRho,
            double biggestRho,
            double errorMargin,
            int sourceAmount)
        {
            // Plain old initialization
            Surface = surface;
            SmallestRho = smallestRho;
            BiggestRho = biggestRho;
            GroundTruthNormalDerivative = groundTruthNormalDerivative;
            ErrorMargin = errorMargin;

            // Initialize sources and Score
            (Group, Score) = GetBestInitialSources(sourceAmount);
        }

        //---------------
        // Public methods
        //---------------
        public void InvokeModelEvent(string message, SourceGroup group = null)
        {
            OnModelEvent(new ModelEventArgs<Point>(message, group));
        }

        public Point[] GetAllInitialSources()
        {
            return new Point[] // TODO: Make this a) smarter and b) not hard-coded
            {
                (0.5, 0.0, 0.0),
                (-0.5, 0.0, 0.0),
                (0.0, 0.5, 0.0),
                (0.0, -0.5, 0.0),
                (0.0, 0.0, 0.5),
                (0.0, 0.0, -0.5),
                (0.0, 0.0, 0.0),
            };
        }

        public (SourceGroup, double) GetBestInitialSources(int sourceAmount)
        {
            SourceGroup candidate, bestCandidate = null;
            double bestScore = -1.0;
            Point[] initials = GetAllInitialSources();
            foreach (List<Point> combination in new Combinations<Point>(initials, sourceAmount))
            {
                candidate = new SourceGroup(combination);
                double score = TargetFunction(candidate);

                if (bestScore < 0.0 || score < bestScore)
                {
                    bestScore = score;
                    bestCandidate = candidate;
                }
            }

            return (bestCandidate, bestScore);
        }

        // A shortcut for target function based on current model state
        public double TargetFunction(SourceGroup group)
        {
            return group.TargetFunction(Surface, GroundTruthNormalDerivative);
        }

        public SphericalVector[] GetMoveFromAntigradient()
        {
            SphericalVector[] result = new SphericalVector[Group.SourceAmount];
            double rate = 1.0;
            for (int i = 0; i < Group.SourceAmount; ++i)
            {
                result[i] = new SphericalVector(
                    -rate * Group.GradComponentRho(Surface, GroundTruthNormalDerivative, i),
                    -rate * Group.GradComponentPhi(Surface, GroundTruthNormalDerivative, i),
                    -rate * Group.GradComponentTheta(Surface, GroundTruthNormalDerivative, i));
            }

            return result;
        }

        public SourceGroup GetMoveResult(SphericalVector[] move, double scale)
        {
            Point[] result = new Point[SourceAmount];
            for (int i = 0; i < SourceAmount; ++i)
            {
                result[i] = Group.Sources[i] + SphericalVector.ScaledVersion(move[i], scale);
            }

            return new SourceGroup(result);
        }

        public SourceGroup[] GetMoveCandidates(SphericalVector[] move, double scale)
        {
            SourceGroup[] result = new SourceGroup[SourceAmount + 1];

            result[0] = GetMoveResult(move, scale);
            for (int i = 0; i < SourceAmount; ++i)
            {
                result[i + 1] = new (Group); // First candidate - antigradient step
                result[i + 1].Sources[i] = result[0].Sources[i]; // Others - single components of the antigradient
            }

            return result;
        }

        public (SourceGroup, double) GetBestMoveCandidate(SourceGroup[] candidates)
        {
            double bestScore = TargetFunction(candidates[0]);
            SourceGroup bestCandidate = candidates[0];

            for (int i = 1; i < candidates.Length; ++i)
            {
                if (OutOfBorders(candidates[i]))
                {
                    continue;
                }

                double score = TargetFunction(candidates[i]);
                if (score < bestScore)
                {
                    bestScore = score;
                    bestCandidate = candidates[i];
                }
            }

            return (bestCandidate, bestScore);
        }

        public (SourceGroup, double, int) GetBestMove(SphericalVector[] move, double scoreToBeat)
        {
            int reductionCount = 0;
            double stepFraction = 1.0;
            SourceGroup candidate;
            double score;
            (candidate, score) = GetBestMoveCandidate(GetMoveCandidates(move, stepFraction)); // TODO: choose smarter

            InvokeModelEvent($"Starting step reduction"); // Output
            while (OutOfBorders(candidate) || score > scoreToBeat)
            {
                if (!OutOfBorders(candidate))
                {
                    reductionCount += 1;
                }

                if (reductionCount > 20)
                {
                    InvokeModelEvent($"Stopping reduction: too many steps"); // Output
                    break;
                }

                stepFraction *= 0.5; // If the step will not actually minimize loss, we halve it and try again
                (candidate, score) = GetBestMoveCandidate(GetMoveCandidates(move, stepFraction));
            }

            return (candidate, score, reductionCount);
        }

        // The main method, finding the sources, including all stages
        public void SearchForSources()
        {
            int stepCount = 0;

            // Declare necessary data structures
            SourceGroup candidate;
            SphericalVector[] proposedMove;
            double score = Score;

            while (score > ErrorMargin)
            {
                stepCount += 1;

                InvokeModelEvent($"Starting step {stepCount}"); // Output
                InvokeModelEvent($"Sources coordinates before the step");
                InvokeModelEvent($"Coordinates", Group);

                proposedMove = GetMoveFromAntigradient();

                int reductionCount;
                double scoreToBeat = TargetFunction(Group);
                (candidate, score, reductionCount) = GetBestMove(proposedMove, scoreToBeat);

                Group = candidate;
                Score = score;

                InvokeModelEvent($"Final values for step {stepCount} after {reductionCount} reductions"); // Output
                InvokeModelEvent($"Old target value: {scoreToBeat}, new target value: {score}");
                InvokeModelEvent("Coordinates", Group);
            }

            InvokeModelEvent($"Stopping at {stepCount} iterations");
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
        private bool OutOfBorders(SourceGroup group)
        {
            foreach (var source in group.Sources)
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

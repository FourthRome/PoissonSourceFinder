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

        public double Score { get; private set; }

        public double ErrorMargin { get; set; } // When to stop minimizing loss function

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
        public void InvokeModelEvent(string message, SourceGroup group = null)
        {
            OnModelEvent(new ModelEventArgs<Point>(message, group));
        }

        public void FindInitialSources()
        {
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
            (candidate, score) = GetBestMoveCandidate(GetMoveCandidates(move, stepFraction));

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

            //double betterScore;
            //double enhancementCount = 0;
            //stepFraction *= 2;
            //SourceGroup betterCandidate;
            //(betterCandidate, betterScore) = GetBestMoveCandidate(GetMoveCandidates(move, stepFraction));
            //InvokeModelEvent($"Starting step enhancement"); // Output
            //while (!OutOfBorders(betterCandidate) && betterScore < score)
            //{
            //    enhancementCount += 1;
            //    candidate = betterCandidate;
            //    score = betterScore;

            //    if (enhancementCount > 20)
            //    {
            //        InvokeModelEvent($"Stopping enhancement: too many steps"); // Output
            //        break;
            //    }

            //    stepFraction *= 2;
            //    (betterCandidate, betterScore) = GetBestMoveCandidate(GetMoveCandidates(move, stepFraction));
            //}

            return (candidate, score, reductionCount);
        }

        // The main method, finding the sources, including all stages
        public void SearchForSources()
        {
            int stepCount = 0;

            // Declare necessary data structures
            SourceGroup candidate;
            SphericalVector[] proposedMove;
            double score = TargetFunction(Group);

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

                Group = candidate; // TODO: don't just move towards antigradient, choose
                Score = score;

                InvokeModelEvent($"Final values for step {stepCount} after {reductionCount} reductions"); // Output
                InvokeModelEvent($"Old target value: {scoreToBeat}, new target value: {score}");
                InvokeModelEvent("Coordinates", Group);
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

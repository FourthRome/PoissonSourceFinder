namespace LossFunctionExtensions
{
    using System;

    using Computation;

    public static class LossFunctionExtensions
    {
        // Component of component of the loss function's gradient, 1/4
        public static double CommonDerivativeComponent(
            this SourceGroup group,
            double rho,
            double phi,
            double theta,
            Func<double, double, double, double> groundTruthNormalDerivative) // TODO: check the safety of the code for arbitrary coordinates
        {
            double result = group.NormalDerivative(rho, phi, theta);
            result -= groundTruthNormalDerivative(rho, phi, theta);
            return result;
        }

        // Component of the loss function's gradient, an integral, 1/3
        public static double GradComponentRho(
            this SourceGroup group,
            SphericalGrid grid,
            Func<double, double, double, double> groundTruthNormalDerivative,
            int sourceNumber)
        {
            // TODO: do not create new function every time, cache it
            return grid.Integrate((double rho, double phi, double theta) =>
            {
                return group.CommonDerivativeComponent(rho, phi, theta, groundTruthNormalDerivative) *
                    group.RhoDerivativeComponentFactory(sourceNumber)(rho, phi, theta);
            });
        }

        // Component of the loss function's gradient, an integral, 2/3
        public static double GradComponentPhi(
            this SourceGroup group,
            SphericalGrid grid,
            Func<double, double, double, double> groundTruthNormalDerivative,
            int sourceNumber)
        {
            return grid.Integrate((double rho, double phi, double theta) =>
            {
                return group.CommonDerivativeComponent(rho, phi, theta, groundTruthNormalDerivative) *
                    group.PhiDerivativeComponentFactory(sourceNumber)(rho, phi, theta);
            });
        }

        // Component of the loss function's gradient, an integral, 3/3
        public static double GradComponentTheta(
            this SourceGroup group,
            SphericalGrid grid,
            Func<double, double, double, double> groundTruthNormalDerivative,
            int sourceNumber)
        {
            return grid.Integrate((double rho, double phi, double theta) =>
            {
                return group.CommonDerivativeComponent(rho, phi, theta, groundTruthNormalDerivative) *
                    group.ThetaDerivativeComponentFactory(sourceNumber)(rho, phi, theta);
            });
        }

        // Loss function over the specified surface
        public static double TargetFunction(
            this SourceGroup group,
            SphericalGrid grid,
            Func<double, double, double, double> groundTruthNormalDerivative)
        {
            return grid.Integrate((double rho, double phi, double theta) =>
            {
                return Math.Pow(group.CommonDerivativeComponent(rho, phi, theta, groundTruthNormalDerivative), 2) * Math.Sin(theta);

                // TODO: Check which of the two calculations is correct
                //return Math.Pow(groundTruthNormalDerivative(rho, phi, theta) - group.NormalDerivative(rho, phi, theta), 2);
            });
        }

        // TODO: cache the factories' returns if it is necessary
        public static Func<double, double, double, double> RhoDerivativeComponentFactory(this SourceGroup group, int sourceNumber)
        {
            return (double rho, double phi, double theta) =>
            {
                Point source = group.Sources[sourceNumber];
                double result = source.AngleCosBetweenVectors(phi, theta);
                result = rho * (source.Rho - result);
                result *= Math.Pow(rho, 2) - Math.Pow(source.Rho, 2);
                result *= 6;
                result /= Math.Pow(source.SquareDistanceFrom(rho, phi, theta), 2.5);
                result += 4 * source.Rho / Math.Pow(source.SquareDistanceFrom(rho, phi, theta), 1.5);
                return result;
            };
        }

        // TODO: cache the factories' returns if it is necessary
        public static Func<double, double, double, double> PhiDerivativeComponentFactory(this SourceGroup group, int sourceNumber)
        {
            return (double rho, double phi, double theta) =>
            {
                Point source = group.Sources[sourceNumber];
                // Computation
                double result = (Math.Pow(rho, 2) - Math.Pow(source.Rho, 2)) * source.Rho;
                result *= Math.Sin(phi - source.Phi) * Math.Sin(theta) * Math.Sin(source.Theta);
                result /= Math.Pow(source.SquareDistanceFrom(rho, phi, theta), 2.5);
                result *= -6;

                return result;
            };
        }

        // TODO: cache the factories' returns if it is necessary
        public static Func<double, double, double, double> ThetaDerivativeComponentFactory(this SourceGroup group, int sourceNumber)
        {
            return (double rho, double phi, double theta) =>
            {
                Point source = group.Sources[sourceNumber];
                // Computation
                double result = (Math.Pow(rho, 2) - Math.Pow(source.Rho, 2)) * source.Rho;
                result *= (Math.Cos(phi - source.Phi) * Math.Sin(theta) * Math.Cos(source.Theta)) - (Math.Cos(theta) * Math.Sin(source.Theta));
                result /= Math.Pow(source.SquareDistanceFrom(rho, phi, theta), 2.5);
                result *= -6;

                return result;
            };
        }
    }
}
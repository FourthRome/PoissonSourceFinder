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
            double result = 0.0;

            foreach (var source in group.Sources)
            {
                result += (Math.Pow(rho, 2) - Math.Pow(source.Rho, 2)) / Math.Pow(source.SquareDistanceFrom(rho, phi, theta), 1.5);
            }

            result /= 4 * Math.PI * rho;
            result += groundTruthNormalDerivative(rho, phi, theta);
            // result *= Math.Sin(theta);  // COMMENTED OUT: probably a mistake
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
                return Math.Pow(groundTruthNormalDerivative(rho, phi, theta) - group.NormalDerivative(rho, phi, theta), 2) *
                    Math.Pow(rho, 2) * Math.Sin(theta);

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
                double result = (Math.Cos(phi - source.Phi) * Math.Sin(theta) * Math.Sin(source.Theta)) + (Math.Cos(theta) * Math.Cos(source.Theta));
                result = source.Rho - (rho * result);
                result *= Math.Pow(source.Rho, 2) - Math.Pow(rho, 2);
                result *= 3 / (2 * Math.PI * Math.Pow(source.SquareDistanceFrom(rho, phi, theta), 2.5));
                result += -source.Rho / (Math.PI * Math.Pow(source.SquareDistanceFrom(rho, phi, theta), 1.5));
                // result *= Radius;  // COMMENTED OUT: probably a mistake
                result /= rho;
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
                result *= 3 / (2 * Math.PI); // COMMENTED OUT R^2: probably a mistake

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
                result *= 3 / (2 * Math.PI);  // COMMENTED OUT R^2: probably a mistake

                return result;
            };
        }
    }
}
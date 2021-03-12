namespace LossFunctionExtensions
{
    using System;

    using Computation;

    public static class LossFunctionExtensions
    {
        // Component of component of the loss function's gradient, 1/4
        public static double CommonDerivativeComponent(this SourceGroup group, double rho, double phi, double theta) // TODO: check the safety of the code for arbitrary coordinates
        {
            double result = 0.0;

            foreach (var source in group.Sources)
            {
                result += (Math.Pow(rho, 2) - Math.Pow(source.Rho, 2)) / Math.Pow(source.SquareDistanceFrom(rho, phi, theta), 1.5);
            }

            result /= 4 * Math.PI * rho;
            // result += GroundTruthNormalDerivative(rho, phi, theta); // TODO: This has to be somewhere else!
            // result *= Math.Sin(theta);  // COMMENTED OUT: probably a mistake
            return result;
        }

        // Component of component of the loss function's gradient, 2/4
        public static double RhoDerivativeComponent(this Point source, double rho, double phi, double theta) // TODO: check the safety of the code for arbitrary coordinates
        {
            // Computation
            double result = (Math.Cos(phi - source.Phi) * Math.Sin(theta) * Math.Sin(source.Theta)) + (Math.Cos(theta) * Math.Cos(source.Theta));
            result = source.Rho - (rho * result);
            result *= Math.Pow(source.Rho, 2) - Math.Pow(rho, 2);
            result *= 3 / (2 * Math.PI * Math.Pow(source.SquareDistanceFrom(rho, phi, theta), 2.5));
            result += -source.Rho / (Math.PI * Math.Pow(source.SquareDistanceFrom(rho, phi, theta), 1.5));
            // result *= Radius;  // COMMENTED OUT: probably a mistake
            result /= rho;
            return result;
        }

        // Component of component of the loss function's gradient, 3/4
        public static double PhiDerivativeComponent(this Point source, double rho, double phi, double theta, int sourceNumber) // TODO: check the safety of the code for arbitrary coordinates
        {
            // Computation
            double result = (Math.Pow(rho, 2) - Math.Pow(source.Rho, 2)) * source.Rho;
            result *= Math.Sin(phi - source.Phi) * Math.Sin(theta) * Math.Sin(source.Theta);
            result /= Math.Pow(source.SquareDistanceFrom(rho, phi, theta), 2.5);
            result *= 3 / (2 * Math.PI); // COMMENTED OUT R^2: probably a mistake

            return result;
        }

        // Component of component of the loss function's gradient, 4/4
        public static double ThetaDerivativeComponent(this Point source, double rho, double phi, double theta, int sourceNumber) // TODO: check the safety of the code for arbitrary coordinates
        {
            // Computation
            double result = (Math.Pow(rho, 2) - Math.Pow(source.Rho, 2)) * source.Rho;
            result *= (Math.Cos(phi - source.Phi) * Math.Sin(theta) * Math.Cos(source.Theta)) - (Math.Cos(theta) * Math.Sin(source.Theta));
            result /= Math.Pow(source.SquareDistanceFrom(rho, phi, theta), 2.5);
            result *= 3 / (2 * Math.PI);  // COMMENTED OUT R^2: probably a mistake

            return result;
        }
    }
}
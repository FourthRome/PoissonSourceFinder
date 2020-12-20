using System;
using System.Diagnostics;
using System.Collections.Generic;

namespace Computation
{
    public class Model
    {
        //------------------
        // Public properties
        //------------------
        
        public PointSource[] Sources { get; set; } // Current sources' coordinates
        public int SourceAmount { get; set; } // Amount of sources
        public double Radius { get; set; }
        public double RadiusMargin { get; set; }
        public List<Tuple<double, double>> AzimuthalRanges { get; set; }
        public List<Tuple<double, double>> PolarRanges { get; set; }
        public Func<double, double, double> GroundTruthNormalDerivative { get; set; }

        public double AzimuthalStep { get; set; }
        public double PolarStep { get; set; }
        public double ErrorMargin { get; set; }

        public Model(double radius, int sourceAmount, PointSource[] sources=null, Func<double, double, double> groundTruthNormalDerivative=null)
        {
            Radius = radius;
            SourceAmount = sourceAmount;
            if (sources != null)
            {
                Sources = sources;
            }
            else
            {
                Sources = new PointSource[SourceAmount];
                for (int i = 0; i < SourceAmount; ++i )
                {
                    Sources[i] = new PointSource(ErrorMargin, i * 2 * Math.PI / SourceAmount);
                }
            }
            GroundTruthNormalDerivative = groundTruthNormalDerivative;

            AzimuthalRanges = new List<Tuple<double, double>>();
            PolarRanges = new List<Tuple<double, double>>();

            AzimuthalStep = 1e-2;
            PolarStep = 1e-2;
            RadiusMargin = Radius - 1e-2;
            ErrorMargin = 1e-2;
        }

        public double NormalDerivative(double phi, double theta)
        {
            double result = 0;
            for (int i = 0; i < SourceAmount; ++i)
            {
                result += (Math.Pow(Sources[i].Rho, 2) - Math.Pow(Radius, 2)) / Math.Pow(Sources[i].SquareDistanceFrom(Radius, phi, theta), 1.5);
            }
            return result / (4 * Math.PI * Radius);
        }


        public double LocalLossFunction(double phi, double theta, int sourceNumber=0)
        {
            return Math.Pow(GroundTruthNormalDerivative(phi, theta) - NormalDerivative(phi, theta), 2) * Math.Pow(Radius, 2) * Math.Sin(theta);
        }

        public double IntegralOverSurface(Func<double, double, int, double> func, int sourceNumber=0)
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

        public double IntegralOverRectangularArea(Func<double, double, int, double> func, Tuple<double, double> aziRange, Tuple<double, double> polRange, int sourceNumber=0)
        {
            int aziCount = Convert.ToInt32(Math.Floor((aziRange.Item2 - aziRange.Item1) / AzimuthalStep));
            int polCount = Convert.ToInt32(Math.Floor((polRange.Item2 - polRange.Item1) / PolarStep));

            double result = 0.0;

            for (int i = 0; i < aziCount; ++i)
            {
                for (int j = 0; j < polCount; ++j)
                {
                    result += AzimuthalStep * PolarStep * func(aziRange.Item1 + (i + 0.5) * AzimuthalStep, polRange.Item1 + (j + 0.5) * PolarStep, sourceNumber);
                }
            }

            return result;
        }

        public double TargetFunction()
        {
            // Integral of loss function over the surface
            return IntegralOverSurface(LocalLossFunction);
        }

        public void SearchForSources()
        {
            double descentRate = 1.0;
            int stepCount = 1;
            while (TargetFunction() > ErrorMargin)
            {
                Trace.WriteLine($"Starting step {stepCount}");
                double[] rhoSteps = new double[SourceAmount];
                double[] phiSteps = new double[SourceAmount];
                double[] thetaSteps = new double[SourceAmount];

                for (int i = 0; i < SourceAmount; ++i)
                {
                    Trace.WriteLine($"Source {i}'s coordinates before steps: {Sources[i].Rho}, {Sources[i].Phi}, {Sources[i].Theta}");
                }

                for (int i = 0; i < SourceAmount; ++i)
                {
                    rhoSteps[i] = descentRate * Math.Pow(Radius - Sources[i].Rho, 2) * GradComponentRho(i);
                    phiSteps[i] = -descentRate * Math.Pow(Radius - Sources[i].Rho, 2) * GradComponentPhi(i);
                    thetaSteps[i] = -descentRate * Math.Pow(Radius - Sources[i].Rho, 2) * GradComponentTheta(i);


                    Trace.WriteLine($"Steps for source {i} are {rhoSteps[i]}, {phiSteps[i]}, {thetaSteps[i]}");
                }

                for (int i = 0; i < SourceAmount; ++i)
                {
                    Sources[i].Rho += rhoSteps[i];
                    Sources[i].Phi += phiSteps[i];
                    Sources[i].Theta += thetaSteps[i];

                    Trace.WriteLine($"Source {i}'s coordinates before validation: {Sources[i].Rho}, {Sources[i].Phi}, {Sources[i].Theta}");
                }

                ValidateCoordinates();

                for (int i = 0; i < SourceAmount; ++i)
                {
                    Trace.WriteLine($"Source {i}'s coordinates after validation: {Sources[i].Rho}, {Sources[i].Phi}, {Sources[i].Theta}");
                }
                stepCount += 1;
            }
        }

        double GradComponentRho(int sourceNumber)
        {
            return IntegralOverSurface(LocalGradComponentRho, sourceNumber);
        }

        double GradComponentPhi(int sourceNumber)
        {
            return IntegralOverSurface(LocalGradComponentPhi, sourceNumber) * 3 * Math.Pow(Radius, 2) / (2 * Math.PI);
        }

        double GradComponentTheta(int sourceNumber)
        {
            return IntegralOverSurface(LocalGradComponentTheta, sourceNumber) * 3 * Math.Pow(Radius, 2) / (2 * Math.PI);
        }

        double LocalGradComponentRho(double phi, double theta, int sourceNumber)
        {
            return CommonDerivativeComponent(phi, theta) * RhoDerivativeComponent(phi, theta, sourceNumber);
        }

        double LocalGradComponentPhi(double phi, double theta, int sourceNumber)
        {
            return CommonDerivativeComponent(phi, theta) * PhiDerivativeComponent(phi, theta, sourceNumber);
        }

        double LocalGradComponentTheta(double phi, double theta, int sourceNumber)
        {
            return CommonDerivativeComponent(phi, theta) * ThetaDerivativeComponent(phi, theta, sourceNumber);
        }

        double CommonDerivativeComponent(double phi, double theta)
        {
            double result = 0.0;

            for (int i = 0; i < SourceAmount; ++i)
            {
                result += (Math.Pow(Radius, 2) - Math.Pow(Sources[i].Rho, 2)) / Math.Pow(Sources[i].SquareDistanceFrom(Radius, phi, theta), 1.5);
            }
            result /= 4 * Math.PI * Radius;
            result += GroundTruthNormalDerivative(phi, theta);
            result *= Math.Sin(theta);
            return result;
        }

        double RhoDerivativeComponent(double phi, double theta, int sourceNumber)
        {
            double result = 0.0;
            PointSource source_i = Sources[sourceNumber];
            double rho_i = Sources[sourceNumber].Rho;
            double phi_i = Sources[sourceNumber].Phi;
            double theta_i = Sources[sourceNumber].Theta;

            result += - rho_i / (Math.PI * Math.Pow(source_i.SquareDistanceFrom(Radius, phi, theta), 1.5));
            result += 3 * Math.Pow(rho_i, 2) * (rho_i - Radius * (Math.Cos(phi - phi_i) * Math.Sin(theta) * Math.Sin(theta_i) + Math.Cos(theta) * Math.Cos(theta_i)))
                / (2 * Math.PI * Math.Pow(source_i.SquareDistanceFrom(Radius, phi, theta), 2.5));
            result *= Radius;

            return result;
        }

        double PhiDerivativeComponent(double phi, double theta, int sourceNumber)
        {
            double result = 0.0;
            PointSource source_i = Sources[sourceNumber];
            double rho_i = Sources[sourceNumber].Rho;
            double phi_i = Sources[sourceNumber].Phi;
            double theta_i = Sources[sourceNumber].Theta;

            result += (Math.Pow(Radius, 2) - Math.Pow(rho_i, 2)) * rho_i * Math.Sin(phi - phi_i) * Math.Sin(theta) * Math.Sin(theta_i);
            result /= Math.Pow(source_i.SquareDistanceFrom(Radius, phi, theta), 2.5);
            return result;
        }

        double ThetaDerivativeComponent(double phi, double theta, int sourceNumber)
        {
            double result = 0.0;
            PointSource source_i = Sources[sourceNumber];
            double rho_i = Sources[sourceNumber].Rho;
            double phi_i = Sources[sourceNumber].Phi;
            double theta_i = Sources[sourceNumber].Theta;

            result += (Math.Pow(Radius, 2) - Math.Pow(rho_i, 2)) * rho_i * (Math.Cos(phi - phi_i) * Math.Sin(theta) * Math.Cos(theta_i) - Math.Cos(theta) * Math.Sin(theta_i));
            result /= Math.Pow(source_i.SquareDistanceFrom(Radius, phi, theta), 2.5);
            return result;
        }

        // Make sure that the sources' coordinates are within reasonable limits
        void ValidateCoordinates()
        {
            for (int i = 0; i < SourceAmount; ++i)
            {
                Sources[i].Rho = Math.Max(Math.Min(Sources[i].Rho, RadiusMargin), ErrorMargin);
                Sources[i].Phi = Sources[i].Phi % (2 * Math.PI);
                if (Sources[i].Phi < 0)
                {
                    Sources[i].Phi += 2 * Math.PI;
                }
                Sources[i].Theta = Sources[i].Theta % (2 * Math.PI);
                if (Sources[i].Theta < 0)
                {
                    Sources[i].Theta += 2 * Math.PI;
                }
                if (Sources[i].Theta > Math.PI)
                {
                    Sources[i].Theta = 2 * Math.PI - Sources[i].Theta;
                }
            }
        }
    }
}

using System;
using System.Diagnostics;
using System.Collections.Generic;

namespace Computation
{
    // Class to hold all the info related to a single computational experiment
    public class Model
    {
        //------------------
        // Public properties
        //------------------
        
        public PointSource[] Sources { get; set; }  // Candidate sources' coordinates
        public int SourceAmount { get; set; }  // Amount of sources
        public double Radius { get; set; }  // Sphere's radius
        public double BiggestRho { get; set; }  // Upper boundary for sources' coordinates
        public double SmallestRho { get; set; }  // Lower boundary for sources' coordinates
        public List<Tuple<double, double>> AzimuthalRanges { get; set; }  // Part of the sphere surface we'll be processing
        public List<Tuple<double, double>> PolarRanges { get; set; }  // Same here
        public Func<double, double, double> GroundTruthNormalDerivative { get; set; }  // Loss function uses it
        public double AzimuthalStep { get; set; }  // Hyperparameter: width of the grid cell for integral computation
        public double PolarStep { get; set; }  // Hyperparameter: height of the grid cell for integral computation
        public double ErrorMargin { get; set; }  // When to stop minimizing loss function; also the lower boundary for sources' coordinates TODO: use a separate member for the latter (maybe)

        //-------------
        // Constructors
        //-------------
        public Model(double radius, int sourceAmount, PointSource[] sources=null, Func<double, double, double> groundTruthNormalDerivative=null)
        {
            // Plain old initialization
            Radius = radius;
            SourceAmount = sourceAmount;
            GroundTruthNormalDerivative = groundTruthNormalDerivative;
            AzimuthalRanges = new List<Tuple<double, double>>();
            PolarRanges = new List<Tuple<double, double>>();

            // Setting the hyperparameters TODO: set them from outside
            // IMPORTANT: should be before sources' initialization, as the initial coordinates depend on ErrorMargin
            AzimuthalStep = 1e-3;
            PolarStep = 1e-3;
            SmallestRho = 0;
            BiggestRho = Radius - 1e-2;
            ErrorMargin = 1e-8;

            if (sources != null)
            {
                Sources = sources;
            }
            else
            {
                Sources = new PointSource[SourceAmount];
                for (int i = 0; i < SourceAmount; ++i)
                {
                    Sources[i] = new PointSource(Radius / 2, i * 2 * Math.PI / SourceAmount, Math.PI / 2);  // Outside of the smallest suitable sphere, and spread apart horizontally
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
        // Minimization problem's internals, possibly to become private (except for the "SearchForSources" method)
        //--------------------------------------------------------------------------------------------------------

        // Loss function, an integral, to be minimized
        public double TargetFunction()
        {
            return IntegralOverSurface(LocalLossFunction);
        }

        // Essentials of the loss function, to be integrated
        public double LocalLossFunction(double phi, double theta, int sourceNumber=-1)  // TODO: find a way to avoid this dirty fictional parameter hack
        {
            return Math.Pow(GroundTruthNormalDerivative(phi, theta) - NormalDerivative(phi, theta), 2) * Math.Pow(Radius, 2) * Math.Sin(theta);
        }

        //// TEST: The brand new main method, finding the sources, including all stages
        //public void SearchForSources()
        //{
        //    double descentRate = 1.0;  // Hyperparameter: how fast should we descend TODO: descent logic should be revised
        //    int stepCount = 1;  // Statistics for the log

        //    // Declare and initialize necessary data structures
        //    PointSource[] oldSources = new PointSource[SourceAmount];
        //    for (int i = 0; i < SourceAmount; ++i)
        //    {
        //        oldSources[i] = new PointSource(Sources[i].Rho, Sources[i].Phi, Sources[i].Theta);  // We initialize this just to be safe
        //    }

        //    CarthesianPoint[] proposedMove = new CarthesianPoint[SourceAmount];
        //    for (int i = 0; i < SourceAmount; ++i)
        //    {
        //        proposedMove[i] = new CarthesianPoint(0, 0, 0);  // We initialize this just to be safe
        //    }

        //    CarthesianPoint[] stepCandidates = new CarthesianPoint[SourceAmount];
        //    for (int i = 0; i < SourceAmount; ++i)
        //    {
        //        stepCandidates[i] = new CarthesianPoint(0, 0, 0);  // We initialize this just to be safe
        //    }



        //    while (TargetFunction() > ErrorMargin)
        //    {
        //        // Here goes a single step of the gradient descent

        //        // Diagnostic output
        //        Console.WriteLine($"\n\n________________________________________Starting step {stepCount}________________________________________");

        //        // Backup old sources
        //        for (int i = 0; i < SourceAmount; ++i)
        //        {
        //            oldSources[i].Rho = Sources[i].Rho;
        //            oldSources[i].Phi = Sources[i].Phi;
        //            oldSources[i].Theta = Sources[i].Theta;
        //        }

        //        // Diagnostic output
        //        for (int i = 0; i < SourceAmount; ++i)
        //        {
        //            Console.WriteLine($"Source {i}'s coordinates before the step: {oldSources[i].Rho}, {oldSources[i].Phi}, {oldSources[i].Theta}");
        //        }

        //        // TEST: normalize the move's components
        //        double normalizer = 0.0;

        //        // Compute the steps towards the antigradient
        //        for (int i = 0; i < SourceAmount; ++i)
        //        {
        //            proposedMove[i] = CarthesianPoint.FromPolarCoordinates(-descentRate * GradComponentRho(i), -descentRate * GradComponentPhi(i), -descentRate * GradComponentTheta(i));
        //            normalizer += proposedMove[i].SquareNorm();

        //            // Diagnostic output
        //            Console.WriteLine($"Step's initial components (x, y, z) for source {i} before normalization are {proposedMove[i].X}, {proposedMove[i].Y}, {proposedMove[i].Z}");
        //            Console.WriteLine($"Step's initial components (rho, phi, theta) for source {i} before normalization are {proposedMove[i].Rho}, {proposedMove[i].Phi}, {proposedMove[i].Theta}");
        //        }

        //        // TEST: Normalization
        //        normalizer = Math.Sqrt(normalizer);
        //        for (int i = 0; i < SourceAmount; ++i)
        //        {
        //            proposedMove[i] /= normalizer;

        //            // Diagnostic output
        //            Console.WriteLine($"Step's initial components (x, y, z) for source {i} after normalization are {proposedMove[i].X}, {proposedMove[i].Y}, {proposedMove[i].Z}");
        //            Console.WriteLine($"Step's initial components (rho, phi, theta) for source {i} after normalization are {proposedMove[i].Rho}, {proposedMove[i].Phi}, {proposedMove[i].Theta}");
        //        }

        //        // Make the initial step
        //        double oldTargetValue = TargetFunction();
        //        for (int i = 0; i < SourceAmount; ++i)
        //        {
        //            Sources[i] = PointSource.FromCarthesianPoint(CarthesianPoint.FromPolarPoint(oldSources[i]) + proposedMove[i]);

        //            // Diagnostic output
        //            Console.WriteLine($"Source {i}'s coordinates after initial step: {Sources[i].Rho}, {Sources[i].Phi}, {Sources[i].Theta}");
        //        }

        //        // Diagnostic output
        //        Console.WriteLine($"________________________________________Starting step reduction________________________________________");

        //        int reductionCount = 0;
        //        double stepFraction = 0.5;  // If the step will not actually minimize loss, we halve it and try again
        //        while (CoordinatesOutOfBorders() || TargetFunction() > oldTargetValue)
        //        {

        //            ++reductionCount;
        //            // Diagnostic output
        //            Console.WriteLine($"________________________________________Reduction for step {stepCount}: {reductionCount}________________________________________");
        //            Console.WriteLine($"Was out of borders: {CoordinatesOutOfBorders()}, old target value: {oldTargetValue}, new target value: {TargetFunction()}");  // TODO: not safe here

        //            for (int i = 0; i < SourceAmount; ++i)
        //            {
        //                proposedMove[i] *= stepFraction;
        //                Sources[i] = PointSource.FromCarthesianPoint(CarthesianPoint.FromPolarPoint(oldSources[i]) + proposedMove[i]);

        //                // Diagnostic output
        //                Console.WriteLine($"Now trying (rho, phi, theta) coordinates for source {i}: {Sources[i].Rho}, {Sources[i].Phi}, {Sources[i].Theta}");
        //            }
        //            stepFraction /= 2;
        //        }

        //        // Diagnostic output
        //        Console.WriteLine($"________________________________________Final values for step {stepCount} after {reductionCount} reductions________________________________________");
        //        Console.WriteLine($"Old target value: {oldTargetValue}, new target value: {TargetFunction()}");

        //        for (int i = 0; i < SourceAmount; ++i)
        //        {
        //            Console.WriteLine($"Source {i}'s coordinates: {Sources[i].Rho}, {Sources[i].Phi}, {Sources[i].Theta}");
        //        }

        //        // The step was made without proper validation, can be non-standard now, so (CAN IT?):
        //        ValidateCoordinates();

        //        // Diagnostic output
        //        for (int i = 0; i < SourceAmount; ++i)
        //        {
        //            Console.WriteLine($"Source {i}'s coordinates after (fictional) validation: {Sources[i].Rho}, {Sources[i].Phi}, {Sources[i].Theta}");
        //        }

        //        // Update statistics
        //        stepCount += 1;
        //    }
        //}

        //// Method to check that the sources' coordinates are within reasonable limits WITHOUT CHANGING THEM
        //bool CoordinatesOutOfBorders()
        //{
        //    for (int i = 0; i < SourceAmount; ++i)
        //    {
        //        // Rho should lie between internal and external spheres' radiuses (the lower boundary is needed because of the gradient's properties)
        //        if (Sources[i].Rho > BiggestRho || Sources[i].Rho < SmallestRho)
        //        {
        //            return true;
        //        }
        //    }

        //    return false;
        //}

        //// Method to make sure that the sources' coordinates are within reasonable limits (now not needed)
        //void ValidateCoordinates()
        //{
        //}


        // The main method, finding the sources, including all stages
        public void SearchForSources()
        {
            double descentRate = 1.0;  // Hyperparameter: how fast should we descend TODO: descent logic should be revised
            int stepCount = 1;  // Statistics for the log
            
            // Declare and initialize necessary data structures
            PointSource[] oldSources = new PointSource[SourceAmount];
            for (int i = 0; i < SourceAmount; ++i)
            {
                oldSources[i] = new PointSource(Sources[i].Rho, Sources[i].Phi, Sources[i].Theta);  // We initialize this just to be safe
            }

            PointSource[] proposedMove = new PointSource[SourceAmount];
            for (int i = 0; i < SourceAmount; ++i)
            {
                proposedMove[i] = new PointSource(0, 0, 0);  // We initialize this just to be safe
            }

            while (TargetFunction() > ErrorMargin)
            {
                // Here goes a single step of the gradient descent

                // Diagnostic output
                Console.WriteLine($"\n\n________________________________________Starting step {stepCount}________________________________________");

                // Backup old sources
                for (int i = 0; i < SourceAmount; ++i)
                {
                    oldSources[i].Rho = Sources[i].Rho;
                    oldSources[i].Phi = Sources[i].Phi;
                    oldSources[i].Theta = Sources[i].Theta;
                }

                // Diagnostic output
                for (int i = 0; i < SourceAmount; ++i)
                {
                    Console.WriteLine($"Source {i}'s coordinates before the step: {Sources[i].Rho}, {Sources[i].Phi}, {Sources[i].Theta}");
                }

                // Coefficient for gradient normalization
                //double normalizer = 0.0;

                // Compute the steps towards the antigradient
                for (int i = 0; i < SourceAmount; ++i)
                {
                    proposedMove[i] = new PointSource(-descentRate * GradComponentRho(i), -descentRate * GradComponentPhi(i), -descentRate * GradComponentTheta(i));
                    //normalizer += proposedMove[i].SquareNorm();

                    // Diagnostic output
                    Console.WriteLine($"Step's initial components (rho, phi, theta) for source {i} before normalization are {proposedMove[i].Rho}, {proposedMove[i].Phi}, {proposedMove[i].Theta}");
                }
                
                // Normalization
                //normalizer = Math.Sqrt(normalizer);
                //for (int i = 0; i < SourceAmount; ++i)
                //{
                //    proposedMove[i] /= normalizer;

                //    // Diagnostic output
                //    Console.WriteLine($"Step's components for source {i} after normalization are {proposedMove[i].Rho}, {proposedMove[i].Phi}, {proposedMove[i].Theta}");
                //}

                // Make the largest step that improves quality
                // Initial step
                double oldTargetValue = TargetFunction();
                for (int i = 0; i < SourceAmount; ++i)
                {
                    Sources[i] = oldSources[i] + proposedMove[i];
                    Sources[i].Validate();

                    // Diagnostic output
                    Console.WriteLine($"Source {i}'s coordinates after initial step: {Sources[i].Rho}, {Sources[i].Phi}, {Sources[i].Theta}");
                }

                // Diagnostic output
                Console.WriteLine($"________________________________________Starting step reduction________________________________________");

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
                        proposedMove[i] *= stepFraction;
                        Sources[i] = oldSources[i] + proposedMove[i];
                        Sources[i].Validate();

                        // Diagnostic output
                        Console.WriteLine($"Now trying coordinates for source {i}: {Sources[i].Rho}, {Sources[i].Phi}, {Sources[i].Theta}");
                    }
                }

                // Diagnostic output
                Console.WriteLine($"________________________________________Final values for step {stepCount} after {reductionCount} reductions________________________________________");
                Console.WriteLine($"Old target value: {oldTargetValue}, new target value: {TargetFunction()}");

                for (int i = 0; i < SourceAmount; ++i)
                {
                    Console.WriteLine($"Source {i}'s coordinates: {Sources[i].Rho}, {Sources[i].Phi}, {Sources[i].Theta}");
                }

                // The step was made without proper validation, can be non-standard now, so:
                //ValidateCoordinates();

                // Diagnostic output
                //for (int i = 0; i < SourceAmount; ++i)
                //{
                //    Console.WriteLine($"Source {i}'s coordinates after validation: {Sources[i].Rho}, {Sources[i].Phi}, {Sources[i].Theta}");
                //}

                // Update statistics
                stepCount += 1;
            }
        }

        // Method to check that the sources' coordinates are within reasonable limits WITHOUT CHANGING THEM
        bool CoordinatesOutOfBorders()
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

        // Method to make sure that the sources' coordinates are within reasonable limits
        void ValidateCoordinates()
        {
            for (int i = 0; i < SourceAmount; ++i)
            {
                // TODO: should we really check this now, after improving steps' logic?
                // Rho should lie between internal and external spheres' radiuses (the lower boundary is needed because of the gradient's properties)
                Sources[i].Rho = Math.Max(Math.Min(Sources[i].Rho, BiggestRho), SmallestRho);

                // Phi should lie within [0; 2*Pi) half-open interval
                Sources[i].Phi = Sources[i].Phi % (2 * Math.PI);
                if (Sources[i].Phi < 0)
                {
                    Sources[i].Phi += 2 * Math.PI;
                }


                // TODO: should we really check this now, after improving steps' logic?
                // Theta should lie within [0; Pi] segment, but, unlike phi, does not form circular trajectory, we should account for that
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

        //-----------------------------------------------------------
        // Gradient computation specifics, possibly to become private
        //-----------------------------------------------------------

        // Component of the loss function's gradient, an integral, 1/3
        double GradComponentRho(int sourceNumber)
        {
            return IntegralOverSurface(LocalGradComponentRho, sourceNumber);
        }

        // Component of the loss function's gradient, an integral, 2/3
        double GradComponentPhi(int sourceNumber)
        {
            return IntegralOverSurface(LocalGradComponentPhi, sourceNumber);
        }

        // Component of the loss function's gradient, an integral, 3/3
        double GradComponentTheta(int sourceNumber)
        {
            return IntegralOverSurface(LocalGradComponentTheta, sourceNumber);
        }

        // Component of the loss function's gradient, to be integrated, 1/3
        double LocalGradComponentRho(double phi, double theta, int sourceNumber)
        {
            return CommonDerivativeComponent(phi, theta) * RhoDerivativeComponent(phi, theta, sourceNumber);
        }

        // Component of the loss function's gradient, to be integrated, 2/3
        double LocalGradComponentPhi(double phi, double theta, int sourceNumber)
        {
            return CommonDerivativeComponent(phi, theta) * PhiDerivativeComponent(phi, theta, sourceNumber);
        }

        // Component of the loss function's gradient, to be integrated, 3/3
        double LocalGradComponentTheta(double phi, double theta, int sourceNumber)
        {
            return CommonDerivativeComponent(phi, theta) * ThetaDerivativeComponent(phi, theta, sourceNumber);
        }

        // Component of component of the loss function's gradient, 1/4
        double CommonDerivativeComponent(double phi, double theta)
        {
            double result = 0.0;

            for (int i = 0; i < SourceAmount; ++i)
            {
                result += (Math.Pow(Radius, 2) - Math.Pow(Sources[i].Rho, 2)) / Math.Pow(Sources[i].SquareDistanceFrom(Radius, phi, theta), 1.5);
            }
            result /= 4 * Math.PI * Radius;
            result += GroundTruthNormalDerivative(phi, theta);
            //result *= Math.Sin(theta);  // COMMENTED OUT: probably a mistake
            return result;
        }

        // Component of component of the loss function's gradient, 2/4
        double RhoDerivativeComponent(double phi, double theta, int sourceNumber)
        {
            // Shortcuts
            PointSource source_i = Sources[sourceNumber];
            double rho_i = Sources[sourceNumber].Rho;
            double phi_i = Sources[sourceNumber].Phi;
            double theta_i = Sources[sourceNumber].Theta;

            // Computation
            double result = Math.Cos(phi - phi_i) * Math.Sin(theta) * Math.Sin(theta_i) + Math.Cos(theta) * Math.Cos(theta_i);
            result = rho_i - Radius * result;
            result *= Math.Pow(rho_i, 2) - Math.Pow(Radius, 2);
            result *= 3 / (2 * Math.PI * Math.Pow(source_i.SquareDistanceFrom(Radius, phi, theta), 2.5));
            result += -rho_i / (Math.PI * Math.Pow(source_i.SquareDistanceFrom(Radius, phi, theta), 1.5));
            //result *= Radius;  // COMMENTED OUT: probably a mistake
            result /= Radius;
            return result;
        }

        // Component of component of the loss function's gradient, 3/4
        double PhiDerivativeComponent(double phi, double theta, int sourceNumber)
        {
            // Shortcuts
            PointSource source_i = Sources[sourceNumber];
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
        double ThetaDerivativeComponent(double phi, double theta, int sourceNumber)
        {
            // Shortcuts
            PointSource source_i = Sources[sourceNumber];
            double rho_i = Sources[sourceNumber].Rho;
            double phi_i = Sources[sourceNumber].Phi;
            double theta_i = Sources[sourceNumber].Theta;

            // Computation
            double result = (Math.Pow(Radius, 2) - Math.Pow(rho_i, 2)) * rho_i;
            result *= Math.Cos(phi - phi_i) * Math.Sin(theta) * Math.Cos(theta_i) - Math.Cos(theta) * Math.Sin(theta_i);
            result /= Math.Pow(source_i.SquareDistanceFrom(Radius, phi, theta), 2.5);
            result *= 3 / (2 * Math.PI);  // COMMENTED OUT R^2: probably a mistake

            return result;
        }

        //---------------------------------------------------------
        // Integral computation methods, possibly to become private
        //---------------------------------------------------------

        // General method for integrals' computation, sum of integrals over all areas
        public double IntegralOverSurface(Func<double, double, int, double> func, int sourceNumber = -1)
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
        public double IntegralOverRectangularArea(Func<double, double, int, double> func, Tuple<double, double> aziRange, Tuple<double, double> polRange, int sourceNumber = -1)
        {
            // How many points should the grid consist of
            int aziCount = Convert.ToInt32(Math.Floor((aziRange.Item2 - aziRange.Item1) / AzimuthalStep));
            int polCount = Convert.ToInt32(Math.Floor((polRange.Item2 - polRange.Item1) / PolarStep));

            double result = 0.0;
            for (int i = 0; i < aziCount; ++i)
            {
                for (int j = 0; j < polCount; ++j)
                {
                    result += AzimuthalStep * Math.Pow(Radius, 2) * (Math.Cos(polRange.Item1 + j * PolarStep) - Math.Cos(polRange.Item1 + (j + 1) * PolarStep))
                        * func(aziRange.Item1 + (i + 0.5) * AzimuthalStep, polRange.Item1 + (j + 0.5) * PolarStep, sourceNumber);
                }
            }

            return result;
        }
    }
}

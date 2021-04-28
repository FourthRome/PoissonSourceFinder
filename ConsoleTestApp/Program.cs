namespace ConsoleTestApp
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using Computation;
    using Contracts;

    public class Program
    {
        public static async Task Main()
        {
            //--------------------------------------
            // Set up part of the sphere's surface S
            //--------------------------------------
            //SphericalSurface surface = new (radius);

            //// Full sphere
            //surface.AddAzimuthalRange(0.0, 2 * Math.PI);
            //surface.AddPolarRange(0, Math.PI);

            //// Hemisphere x > 0
            //surface.AddAzimuthalRange(0, Math.PI / 2);
            //surface.AddAzimuthalRange(3 * Math.PI / 2, 2 * Math.PI);
            //surface.AddPolarRange(0, Math.PI);

            //// Hemisphere x < 0
            //surface.AddAzimuthalRange(Math.PI / 2, 3 * Math.PI / 2);
            //surface.AddPolarRange(0, Math.PI);

            //// Hemisphere y > 0
            //surface.AddAzimuthalRange(0, Math.PI);
            //surface.AddPolarRange(0, Math.PI);

            //// Hemisphere y < 0
            //surface.AddAzimuthalRange(Math.PI, 2 * Math.PI);
            //surface.AddPolarRange(0, Math.PI);

            //// Hemisphere z > 0
            //surface.AddAzimuthalRange(0, 2 * Math.PI);
            //surface.AddPolarRange(0, Math.PI / 2);

            //// Hemisphere z < 0
            //surface.AddAzimuthalRange(0, 2 * Math.PI);
            //surface.AddPolarRange(Math.PI / 2, Math.PI);

            //// Eighth of a sphere x > 0, y > 0, z > 0
            //surface.AddAzimuthalRange(0, Math.PI / 2);
            //surface.AddPolarRange(0, Math.PI / 2);

            //// Eighth of a sphere x < 0, y < 0, z < 0
            //surface.AddAzimuthalRange(Math.PI, 3 * Math.PI / 2);
            //surface.AddPolarRange(Math.PI / 2, Math.PI);

            // TODO: now that's a little better...
            SourceGroup twoGroup = new (
                new Point[]
                {
                    (0.3, 0.4, 0.6),
                    (-0.7, -0.2, 0.1),
                });

            SourceGroup threeGroup = new (
                new Point[]
                {
                    (0.4, 0.8, 0.1),
                    (-0.2, -0.5, -0.5),
                    (-0.7, -0.3, -0.4),
                });

            SphericalSurface fullSphere = new (
                1.0,
                new () { (0.0, 2 * Math.PI), },
                new () { (0.0, Math.PI), });

            SphericalSurface xPositiveSphere = new (
                1.0,
                new ()
                {
                    (0.0, Math.PI / 2),
                    (3 * Math.PI / 2, 2 * Math.PI),
                },
                new () { (0.0, Math.PI), });

            SphericalSurface zPositiveSphere = new (
                1.0,
                new () { (0.0, 2 * Math.PI), },
                new () { (0.0, Math.PI / 2), });

            List<Experiment> experiments = new ();

            experiments.Add(new (
                new ()
                {
                    { "GroundTruthSourceGroup", twoGroup },
                    { "Surface", fullSphere },
                    { "MoveNormStoppingCondition", 1e-4 },
                    { "ExperimentLabel", "Mistake-1" },
                }));

            experiments.Add(new (
                new ()
                {
                    { "GroundTruthSourceGroup", twoGroup },
                    { "Surface", zPositiveSphere },
                    { "MoveNormStoppingCondition", 1e-4 },
                    { "ExperimentLabel", "Mistake-2" },
                }));

            experiments.Add(new (
                new ()
                {
                    { "GroundTruthSourceGroup", threeGroup },
                    { "Surface", fullSphere },
                    { "MoveNormStoppingCondition", 1e-7 },
                    { "ExperimentLabel", "Mistake-3" },
                }));

            // TODO: make this sequential execution logic clearer
            foreach (var exp in experiments)
            {
                await exp.Run();
            }

            Console.ReadLine();
        }
    }
}

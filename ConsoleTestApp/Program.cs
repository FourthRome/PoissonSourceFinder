namespace ConsoleTestApp
{
    using System;
    using System.Collections.Generic;
    using System.IO;
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

            // TODO: God forgive me...
            Experiment experiment = new (
                new ()
                {
                    {
                        "GroundTruthSourceGroup",
                        new SourceGroup(
                            new Point[]
                            {
                                (0.2, 0.4, 0.8),
                                (0.3, -0.7, -0.4),
                            })
                    },
                },
                new ()
                {
                    {
                        "Surface",
                        new SphericalSurface(
                            1.0,
                            new ()
                            {
                                (0.0, 2 * Math.PI),
                            },
                            new ()
                            {
                                (0.0, Math.PI),
                            })
                    },
                },
                new ()
                {
                    {
                        "MoveNormStoppingCondition",
                        1e-5
                    },
                },
                new ()
                {
                    {
                        "ExperimentLabel",
                        "TestRunForNewFormat"
                    },
                });

            await experiment.Run();

            Console.ReadLine();
        }
    }
}

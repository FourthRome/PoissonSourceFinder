namespace ConsoleTestApp
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Computation;

    public class Program
    {
        //----------------------
        // Public static methods
        //----------------------
        public static async Task Main()
        {
            //---------------------------------
            // SourceGroups for the experiments
            //---------------------------------
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

            //-----------------------
            // Setting up experiments
            //-----------------------
            // Settings for the whole series
            List<Experiment> experiments = new ();
            int experimentNumber = 1;
            string seriesLabel = "NewAPI";

            // Settings for specific experiments
            experiments.Add(new (
                new ()
                {
                    { "GroundTruthSourceGroup", twoGroup },
                    { "Surface", UnitSurfaces["full"] },
                    { "MoveNormStoppingCondition", 1e-4 },
                    { "ExperimentLabel", $"{seriesLabel}-{experimentNumber++}" },
                }));

            experiments.Add(new (
                new ()
                {
                    { "GroundTruthSourceGroup", twoGroup },
                    { "Surface", UnitSurfaces["zPositive"] },
                    { "MoveNormStoppingCondition", 1e-4 },
                    { "ExperimentLabel", $"{seriesLabel}-{experimentNumber++}" },
                }));

            experiments.Add(new (
                new ()
                {
                    { "GroundTruthSourceGroup", threeGroup },
                    { "Surface", UnitSurfaces["full"] },
                    { "MoveNormStoppingCondition", 1e-7 },
                    { "ExperimentLabel", $"{seriesLabel}-{experimentNumber++}" },
                }));

            // TODO: make this sequential execution logic clearer
            foreach (var exp in experiments)
            {
                await exp.Run();
            }

            Console.ReadLine();
        }

        //----------------------
        // Private static fields
        //----------------------
        // TODO: find a better way to cache this information for common usage patterns
        private static readonly double UnitRadius = 1.0;

        private static readonly Dictionary<string, SphericalSurface> UnitSurfaces = new ()
        {
            {
                "full",
                new SphericalSurface(
                    UnitRadius,
                    new () { (0.0, 2 * Math.PI) },
                    new () { (0.0, Math.PI) })
            },
            {
                "xPositive",
                new SphericalSurface(
                    UnitRadius,
                    new () { (0.0, Math.PI / 2), (3 * Math.PI / 2, 2 * Math.PI) },
                    new () { (0.0, Math.PI) })
            },
            {
                "xNegative",
                new SphericalSurface(
                    UnitRadius,
                    new () { (Math.PI / 2, 3 * Math.PI / 2) },
                    new () { (0.0, Math.PI) })
            },
            {
                "yPositive",
                new SphericalSurface(
                    UnitRadius,
                    new () { (0.0, Math.PI) },
                    new () { (0.0, Math.PI) })
            },
            {
                "yNegative",
                new SphericalSurface(
                    UnitRadius,
                    new () { (Math.PI, 2 * Math.PI) },
                    new () { (0.0, Math.PI) })
            },
            {
                "zPositive",
                new SphericalSurface(
                    UnitRadius,
                    new () { (0.0, 2 * Math.PI) },
                    new () { (0.0, Math.PI / 2) })
            },
            {
                "zNegative",
                new SphericalSurface(
                    UnitRadius,
                    new () { (0.0, 2 * Math.PI) },
                    new () { (Math.PI / 2, Math.PI) })
            },
        };
    }
}

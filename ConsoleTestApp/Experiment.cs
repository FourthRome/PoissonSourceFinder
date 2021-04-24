namespace ConsoleTestApp
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading.Tasks;

    using Computation;
    using Contracts;

    public class Experiment
    {
        //------------------
        // Public properties
        //------------------
        public Dictionary<string, double> DoubleKeys { get; protected set; }

        public Dictionary<string, SourceGroup> SourceGroupKeys { get; protected set; }

        public Dictionary<string, SphericalSurface> SphericalSurfaceKeys { get; protected set; }

        public Dictionary<string, string> StringKeys { get; protected set; }

        public SphericalGrid Grid { get; protected set; }

        public GroundTruth GroundTruth { get; protected set; }

        public Model Model { get; protected set; }

        public SourceGroup InitialSourceGroup { get; protected set; }

        public double InitialScore { get; protected set; }

        //-------------
        // Constructors
        //-------------
        public Experiment(
                Dictionary<string, SourceGroup> sourceGroupKeys,
                Dictionary<string, SphericalSurface> sphericalSurfaceKeys = null,
                Dictionary<string, double> doubleKeys = null,
                Dictionary<string, string> stringKeys = null)
        {
            // TODO: is this really the best interface possible? I'm pretty much sure there's a cleaner solution
            // to this "too many parameters" problem

            // Keys' groups with some defaults should be initialized with defaults first
            InitializeKeysWithDefaults();

            // Keys' initialization from constructor's parameters
            DoubleKeys.MergeInPlace(doubleKeys); // For this syntax see GeneralExtensions.cs
            StringKeys.MergeInPlace(stringKeys);
            SphericalSurfaceKeys.MergeInPlace(sphericalSurfaceKeys);
            SourceGroupKeys.MergeInPlace(sourceGroupKeys);

            // Check that we're all set
            CheckKeysValidity();
            Prepare();
        }

        //---------------
        // Public methods
        //---------------
        public async Task Run()
        {
            Console.WriteLine($"Initial model score: {InitialScore}");
            Console.WriteLine($"Initial sources' coordinates:");
            Console.WriteLine(InitialSourceGroup);

            // Start inference process
            Model.SearchForSources();

            // Print the results
            Console.WriteLine($"Final results:");
            Console.WriteLine();

            Console.WriteLine($"Model's target value: {Model.Score}");
            Console.WriteLine();

            Console.WriteLine($"Sources' real coordinates:");
            Console.WriteLine(SourceGroupKeys["GroundTruthSourceGroup"]);
            Console.WriteLine();

            Console.WriteLine($"Sources' calculated coordinates:");
            Console.WriteLine(Model.Group);

            // Generate output files
            await WriteResultsTxt();
            await WriteResultsCsv();
        }

        //----------------------
        // Public static methods
        //----------------------
        public static void ModelEventCallback(object sender, ModelEventArgs<Point> args)
        {
            Console.WriteLine(args.Message);
            if (args.Group != null)
            {
                foreach (var source in args.Group)
                {
                    Console.WriteLine(source);
                }
            }

            Console.WriteLine();
        }

        //------------------
        // Protected methods
        //------------------
        protected void InitializeKeysWithDefaults()
        {
            DoubleKeys = new Dictionary<string, double>()
            {
                { "Radius", 1.0 },
                { "AzimuthalStep", 1e-2 },
                { "PolarStep", 1e-2 },
                { "SmallestRho", 0.0 },
                { "BiggestRho", 1.0 - 1e-2 },
                { "ScoreValueStoppingCondition", 0.0 },
                { "ScoreImprovementStoppingCondition", 0.0 },
                { "MoveNormStoppingCondition", 0.0 },
                { "Delta", 0.0 },
            };

            StringKeys = new Dictionary<string, string>()
            {
                { "ExperimentLabel", "no-label" },
                { "ResultsPath", "../../../../Results" },
            };

            SphericalSurfaceKeys = new Dictionary<string, SphericalSurface>()
            {
                { "Surface", new SphericalSurface(DoubleKeys["Radius"]) },
            };
            SphericalSurfaceKeys["Surface"].AddAzimuthalRange(0.0, 2 * Math.PI);
            SphericalSurfaceKeys["Surface"].AddPolarRange(0.0, Math.PI);

            SourceGroupKeys = new Dictionary<string, SourceGroup>()
            {
                { "InitialSourceGroup", null },
            };
        }

        protected void CheckKeysValidity()
        {
            // TODO: Here's the most basic check, it should be expanded for various input
            if (!SourceGroupKeys.ContainsKey("GroundTruthSourceGroup"))
            {
                throw new ArgumentException("Experiment should be provided with a 'GroundTruthSourceGroup' key in 'sourceGroupKeys'");
            }
        }

        protected void Prepare()
        {
            Grid = new RectangularSphericalGrid(SphericalSurfaceKeys["Surface"], DoubleKeys["AzimuthalStep"], DoubleKeys["PolarStep"]);
            GroundTruth = new (SourceGroupKeys["GroundTruthSourceGroup"], Grid, DoubleKeys["Delta"]);
            Model = new (
                Grid,
                GroundTruth.CachedNormalDerivative,
                DoubleKeys["SmallestRho"],
                DoubleKeys["BiggestRho"],
                DoubleKeys["ScoreValueStoppingCondition"],
                DoubleKeys["ScoreImprovementStoppingCondition"],
                DoubleKeys["MoveNormStoppingCondition"],
                SourceGroupKeys["GroundTruthSourceGroup"].SourceAmount,
                SourceGroupKeys["InitialSourceGroup"]);
            InitialSourceGroup = Model.Group;
            InitialScore = Model.Score;
            Model.ModelEvent += ModelEventCallback;
        }

        protected async Task WriteResultsTxt()
        {
            // Make aliases for some keys (so that code is more readable)
            string experimentLabel = StringKeys["ExperimentLabel"];
            string path = StringKeys["ResultsPath"];
            SourceGroup groundTruthSourceGroup = SourceGroupKeys["GroundTruthSourceGroup"];

            // Open file
            string filename = DateTime.Now.ToString("yyyy-MM-dd__HH-mm-ss") + "-" + experimentLabel + ".txt";
            using StreamWriter file = new (path + "/" + filename);

            // Write contents
            await file.WriteLineAsync("Real sources' cartesian coordinates:");
            await file.WriteLineAsync(groundTruthSourceGroup.ToStringCartesian());

            await file.WriteLineAsync("Initial sources' cartesian coordinates:");
            await file.WriteLineAsync(InitialSourceGroup.ToStringCartesian());
            await file.WriteLineAsync($"Initial score: {InitialScore}");

            await file.WriteLineAsync($"Calculated sourses' cartesian coordinates:");
            await file.WriteLineAsync(Model.Group.ToStringCartesian());

            await file.WriteLineAsync($"Final score: {Model.Score}");
            await file.WriteLineAsync($"Number of iterations: {Model.IterationsNumber}");
        }

        protected async Task WriteResultsCsv()
        {
            // Make aliases for some keys (so that code is more readable)
            string experimentLabel = StringKeys["ExperimentLabel"];
            string path = StringKeys["ResultsPath"];
            SourceGroup groundTruthSourceGroup = SourceGroupKeys["GroundTruthSourceGroup"];

            // Open file
            string filename = DateTime.Now.ToString("yyyy-mm-dd_hh-mm-ss") + "-" + experimentLabel + ".csv";
            using StreamWriter file = new (path + "/" + filename);

            // Write contents
            await file.WriteLineAsync("x,y,z,cat,label,");
            foreach (var (idx, source) in groundTruthSourceGroup.Sources.Enumerate(start: 1))
            {
                await file.WriteLineAsync($"{source.ToStringCartesianCsv()}A,A{idx},");
            }

            foreach (var (idx, source) in InitialSourceGroup.Sources.Enumerate(start: 1))
            {
                await file.WriteLineAsync($"{source.ToStringCartesianCsv()}B,B{idx},");
            }

            foreach (var (idx, source) in Model.Group.Sources.Enumerate(start: 1))
            {
                await file.WriteLineAsync($"{source.ToStringCartesianCsv()}C,C{idx},");
            }
        }
    }
}

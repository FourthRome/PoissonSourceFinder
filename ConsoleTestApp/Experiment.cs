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
        public Dictionary<string, object> Properties { get; set; }

        public SphericalGrid Grid { get; protected set; }

        public GroundTruth GroundTruth { get; protected set; }

        public Model Model { get; protected set; }

        public SourceGroup InitialSourceGroup { get; protected set; }

        public double InitialScore { get; protected set; }

        //-------------
        // Constructors
        //-------------
        public Experiment(Dictionary<string, object> properties)
        {
            // TODO: now that's a lot better, but can it be even clearer? I mean API for providing properties of experiments
            SetDefaultProperties(); // Properties with defaults should be initialized before being (possibly) owerwritten
            Properties.MergeInPlace(properties); // For this syntax see GeneralExtensions.cs
            CheckPropertiesValidity();
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
            Console.WriteLine((SourceGroup)Properties["GroundTruthSourceGroup"]);
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
        protected void SetDefaultProperties()
        {
            Properties = new Dictionary<string, object>()
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
                { "ExperimentLabel", "no-label" },
                { "ResultsPath", "../../../../Results" },
                { "InitialSourceGroup", null },
            };

            Properties.Add(
                "Surface",
                new SphericalSurface(
                    (double)Properties["Radius"],
                    new () { (0.0, 2 * Math.PI), },
                    new () { (0.0, Math.PI), }));
        }

        protected void CheckPropertiesValidity()
        {
            // TODO: Here's the most basic check, it should be expanded for various input
            if (!Properties.ContainsKey("GroundTruthSourceGroup"))
            {
                throw new ArgumentException("Experiment should be provided with a 'GroundTruthSourceGroup' key in 'properties' constructor argument");
            }
        }

        protected void Prepare()
        {
            Grid = new RectangularSphericalGrid(
                    (SphericalSurface)Properties["Surface"],
                    (double)Properties["AzimuthalStep"],
                    (double)Properties["PolarStep"]);

            GroundTruth = new (
                    (SourceGroup)Properties["GroundTruthSourceGroup"],
                    Grid,
                    (double)Properties["Delta"]);

            Model = new (
                Grid,
                GroundTruth.CachedNormalDerivative,
                (double)Properties["SmallestRho"],
                (double)Properties["BiggestRho"],
                (double)Properties["ScoreValueStoppingCondition"],
                (double)Properties["ScoreImprovementStoppingCondition"],
                (double)Properties["MoveNormStoppingCondition"],
                ((SourceGroup)Properties["GroundTruthSourceGroup"]).SourceAmount,
                (SourceGroup)Properties["InitialSourceGroup"]);

            InitialSourceGroup = Model.Group;
            InitialScore = Model.Score;
            Model.ModelEvent += ModelEventCallback;
        }

        protected async Task WriteResultsTxt()
        {
            // Make aliases for some keys (so that code is more readable)
            string experimentLabel = (string)Properties["ExperimentLabel"];
            string path = (string)Properties["ResultsPath"];
            SourceGroup groundTruthSourceGroup = (SourceGroup)Properties["GroundTruthSourceGroup"];

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
            string experimentLabel = (string)Properties["ExperimentLabel"];
            string path = (string)Properties["ResultsPath"];
            SourceGroup groundTruthSourceGroup = (SourceGroup)Properties["GroundTruthSourceGroup"];

            // Open file
            string filename = DateTime.Now.ToString("yyyy-MM-dd__HH-mm-ss") + "-" + experimentLabel + ".csv";
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

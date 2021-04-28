namespace ConsoleTestApp
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using System.Threading.Tasks;

    using Computation;
    using Contracts;

    public class Experiment : IDisposable
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

        public bool Finished { get; protected set; }

        //-------------
        // Constructors
        //-------------
        public Experiment(Dictionary<string, object> properties)
        {
            // TODO: now that's a lot better, but can it be even clearer? I mean API for providing properties of experiments
            Finished = false;
            SetDefaultProperties(); // Properties with defaults should be initialized before being (possibly) owerwritten
            Properties.MergeInPlace(properties); // For this syntax see GeneralExtensions.cs
            CheckPropertiesValidity();
            Prepare().Wait(); // TODO: smells like bad async code, read more about how to do this properly
        }

        public void Dispose()
        {
            // TODO: understand IDisposable (now I clearly don't)
            if (LogCsvWriter != null)
            {
                LogCsvWriter.Dispose();
            }

            if (LogTxtWriter != null)
            {
                LogTxtWriter.Dispose();
            }
        }

        //---------------
        // Public methods
        //---------------
        public async Task Run()
        {
            if (Finished)
            {
                throw new InvalidOperationException("Same Experiment object cannot be .Run() twice due to internal functionality.");
            }

            // TODO: ok, this looks like a TERRIBLY smelly code. Please educate yourself on async patterns.
            await Output($"Initial model score: {InitialScore}");
            await Output($"Initial sources' coordinates:");
            await Output(InitialSourceGroup.ToString());

            // Start inference process
            Model.SearchForSources();

            // Print the results
            await Output($"Final results:");
            await Output();

            await Output($"Model's target value: {Model.Score}");
            await Output();

            await Output($"Sources' real coordinates:");
            await Output(((SourceGroup)Properties["GroundTruthSourceGroup"]).ToString());
            await Output();
            await LogCsvWriter.WriteAsync(((SourceGroup)Properties["GroundTruthSourceGroup"]).ToStringCartesianCsv(category: "A"));

            await Output($"Sources' calculated coordinates:");
            await Output(Model.Group.ToString());
            await LogCsvWriter.WriteAsync(InitialSourceGroup.ToStringCartesianCsv(category: "B"));
            await LogCsvWriter.WriteAsync(Model.Group.ToStringCartesianCsv(category: "С"));

            // Generate output files
            await WriteResultsTxt();
            await WriteResultsCsv();

            // Disposal of resources
            await LogCsvWriter.DisposeAsync();
            await LogTxtWriter.DisposeAsync();
            Finished = true;
        }

        public async void ModelEventCallback(object sender, ModelEventArgs<Point> args)
        {
            Console.WriteLine(args.Message);
            await LogTxtWriter.WriteLineAsync(args.Message);

            if (args.Group != null)
            {
                Console.WriteLine(args.Group);
                await LogTxtWriter.WriteLineAsync(args.Group.ToString());

                foreach (Point source in args.Group)
                {
                    await LogCsvWriter.WriteLineAsync(source.ToStringCartesianCsv() + "C,,");
                }
            }

            Console.WriteLine();
            await LogTxtWriter.WriteLineAsync();
        }

        //---------------------
        // Protected properties
        //---------------------
        protected StreamWriter LogTxtWriter { get; set; }

        protected StreamWriter LogCsvWriter { get; set; }

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
                { "LogsPath", "../../../../Logs" },
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

        protected async Task Prepare()
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

            // Setting up loggers
            string experimentLabel = (string)Properties["ExperimentLabel"];
            string logPath = (string)Properties["LogsPath"];
            string filename = DateTime.Now.ToString("yyyy-MM-dd__HH-mm-ss") + "-LOG-" + experimentLabel;
            LogTxtWriter = new(logPath + "/" + filename + ".txt");
            LogCsvWriter = new(logPath + "/" + filename + ".csv");

            // Initial write to csv file TODO: put this somewhere else
            await LogCsvWriter.WriteLineAsync("x,y,z,cat,label,");
            await LogCsvWriter.WriteAsync(InitialSourceGroup.ToStringCartesianCsv(category: "C", noLabels: true));

            // Subscribing to model events
            Model.ModelEvent += ModelEventCallback;
        }

        protected async Task Output(string message = "")
        {
            Console.WriteLine(message);
            await LogTxtWriter.WriteLineAsync(message);
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
            StringBuilder contents = new ();

            contents.AppendLine("Real sources' cartesian coordinates:");
            contents.AppendLine(groundTruthSourceGroup.ToStringCartesian());

            contents.AppendLine("Initial sources' cartesian coordinates:");
            contents.AppendLine(InitialSourceGroup.ToStringCartesian());
            contents.AppendLine($"Initial score: {InitialScore}");

            contents.AppendLine($"Calculated sourses' cartesian coordinates:");
            contents.AppendLine(Model.Group.ToStringCartesian());

            contents.AppendLine($"Final score: {Model.Score}");
            contents.AppendLine($"Number of iterations: {Model.IterationsNumber}");

            await file.WriteLineAsync(contents.ToString());
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
            await file.WriteAsync(groundTruthSourceGroup.ToStringCartesianCsv(category: "A"));
            await file.WriteAsync(InitialSourceGroup.ToStringCartesianCsv(category: "B"));
            await file.WriteAsync(Model.Group.ToStringCartesianCsv(category: "C"));
        }
    }
}

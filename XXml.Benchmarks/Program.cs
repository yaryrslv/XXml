using System.Diagnostics;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Exporters;
using BenchmarkDotNet.Exporters.Csv;
using BenchmarkDotNet.Loggers;
using BenchmarkDotNet.Running;
using BenchmarkDotNet.Validators;
using XXml.XmlEntities;

namespace XXml.Benchmarks
{
    abstract class Program
    {
        public static void Main()
        {
            var config = new ManualConfig()
                .WithOptions(ConfigOptions.DisableOptimizationsValidator)
                .AddValidator(JitOptimizationsValidator.DontFailOnError)
                .AddLogger(ConsoleLogger.Default)
                .AddColumnProvider(DefaultColumnProviders.Instance)
                .AddExporter(RPlotExporter.Default, CsvExporter.Default);
            
            BenchmarkRunner.Run<ParserFileBenchmark>(config);
        }
    }

    [MemoryDiagnoser]
    [MarkdownExporterAttribute.GitHub]
    [RyuJitX64Job]
    public class ParserFileBenchmark
    {
        private const string BenchmarkFile = "TicketsInsert.xml";
        private const string TestsDataFolder = "BenchmarksData";
        private string? FilePath { get; set; } = Path.Combine(Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory)?
            .FullName ?? string.Empty, TestsDataFolder, BenchmarkFile);

        [Benchmark(Baseline = true, Description = "XXml")]
        public void XmlParser_GetAllNodesByTagName()
        {
            using var xml = XmlParser.ParseFile(FilePath);
            var list = xml.GetAllNodes(XmlNodeType.ElementNode).Where(x => x.Name == "Rec");
            foreach (var item in list) {
                foreach(var attribute in item.Attributes) {
                    Debug.WriteLine(attribute.Name + ": " + attribute.Value);
                }
            }
            xml.Dispose();
        }
        
        [Benchmark(Description = "System.Xml.XmlReader")]
        public void SystemXmlReader_XmlParser_GetAllNodesByTagName()
        {
            using var reader = System.Xml.XmlReader.Create(FilePath);
            while (reader.Read())
            {
                if (reader.NodeType != System.Xml.XmlNodeType.Element || reader.Name != "Rec") continue;
                for (var i = 0; i < reader.AttributeCount; i++)
                {
                    reader.MoveToAttribute(i);
                    Debug.WriteLine(reader.Name + ": " + reader.Value);
                }
            }
        }
    }
}
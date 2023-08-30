using Xunit.Abstractions;
using XXml.XmlEntities;

namespace XXml.Tests
{
    public class DebugTest
    {
        private const string DebugFile = "TicketsInsert.xml";
        private const string TestsDataFolder = "TestsData";
        private string FilePath { get; set; } = Path.Combine(Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory)?
            .FullName ?? string.Empty, TestsDataFolder, DebugFile);
        
        private readonly ITestOutputHelper _output;

        public DebugTest(ITestOutputHelper output)
        {
            _output = output;
        }
        
        [Fact]
        public void XXml_XmlParser_File()
        {
            var xml = XmlParser.ParseFile(FilePath);
            var list = xml.GetAllNodes(XmlNodeType.ElementNode).Where(x => x.Name == "Rec");
            foreach (var item in list)
            {
                foreach (var attribute in item.Attributes)
                {
                    _output.WriteLine(attribute.Name + ": " + attribute.Value);
                }
                _output.WriteLine("========================================");
            }
            Assert.NotNull(list);
            Assert.Equal(96786, list.Count());
        }
    }
}
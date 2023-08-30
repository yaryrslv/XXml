using Xunit.Abstractions;
using XXml.XmlEntities;

namespace XXml.Tests;

public class SmokeTests
{
    private const string DebugFile = "TicketsInsert.xml";
    private const string TestsDataFolder = "TestsData";
    private string? FilePath { get; set; } = Path.Combine(Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory)?
        .FullName ?? string.Empty, TestsDataFolder, DebugFile);
        
    private readonly ITestOutputHelper _output;

    public SmokeTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public void XXml_XmlParser_GetChildren()
    {
        
        var xml = XmlParser.ParseFile(FilePath);
        var firstNode = xml.GetAllNodes(XmlNodeType.ElementNode).First();
        var children = firstNode.GetChildren(XmlNodeType.ElementNode);
        foreach (var item in children)
        {
            foreach (var attribute in item.Attributes)
            {
                _output.WriteLine(attribute.Name + ": " + attribute.Value);
            }
            _output.WriteLine(new string('=', 8));
        }
        Assert.Equal(96786, children.Count());
    }
    
    [Fact]
    public void XXml_XmlParser_GetLocation()
    {
        var xml = XmlParser.ParseFile(FilePath);
        var firstNode = xml.GetAllNodes(XmlNodeType.ElementNode).First();
        var location = xml.GetLocation(firstNode);
        _output.WriteLine("Range: " + location.Range + " | Start: " + location.Start + " | End: " + location.End);
    }
}
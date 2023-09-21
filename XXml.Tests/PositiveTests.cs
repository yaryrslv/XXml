using Xunit.Abstractions;
using XXml.XmlEntities;

namespace XXml.Tests;

public class PositiveTests
{
    private const string DebugFile = "TicketsInsert.xml";
    private const string TestsDataFolder = "TestsData";
    private string? FilePath { get; set; } = Path.Combine(Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory)?
        .FullName ?? string.Empty, TestsDataFolder, DebugFile);
        
    private readonly ITestOutputHelper _output;

    public PositiveTests(ITestOutputHelper output)
    {
        _output = output;
    }
    
    [Fact]
    public void XmlParser_GetAllNodesByTagName()
    {
        var valueString = "";
        using var xml = XmlParser.ParseFile(FilePath);
        var list = xml.GetAllNodes(XmlNodeType.ElementNode).Where(x => x.Name == "Rec");
        foreach (var item in list) {
            foreach(var attribute in item.Attributes) {
                if (attribute.Name == "Action" && attribute.Value == "X")
                {
                    valueString = attribute.Name + ": " + attribute.Value;
                    break;
                }
            }
        }
        xml.Dispose();
        _output.WriteLine(valueString);
        Assert.NotNull(valueString);
        Assert.NotEmpty(valueString);
    }
}
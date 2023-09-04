using XXml.XmlEntities;

const string benchmarkFile = "TicketsInsert.xml";
const string testsDataFolder = "ExamplesData";
string filePath = Path.Combine(Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory)?
    .FullName ?? string.Empty, testsDataFolder, benchmarkFile);

Console.ReadKey();


//Пример с использованием XXml
var testDataXXml = "";
using var xml = XmlParser.ParseFile(filePath);
var list = xml.GetAllNodes(XmlNodeType.ElementNode).Where(x => x.Name == "Rec");
foreach (var item in list) {
    foreach(var attribute in item.Attributes)
    {
        testDataXXml = attribute.Name.ToString() + attribute.Value.ToString();
    }
}
xml.Dispose();
Console.ReadKey();

//Пример с использованием System.Xml.XmlReader
var testDataSystemXmlXmlReader = "";
using var reader = System.Xml.XmlReader.Create(filePath);
while (reader.Read())
{
    if (reader.NodeType != System.Xml.XmlNodeType.Element || reader.Name != "Rec") continue;
    for (var i = 0; i < reader.AttributeCount; i++)
    {
        reader.MoveToAttribute(i);
        testDataSystemXmlXmlReader = reader.Name.ToString() + reader.Value.ToString();
    }
}
Console.ReadKey();
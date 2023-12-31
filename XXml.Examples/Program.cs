﻿using System.Diagnostics;
using XXml.XmlEntities;

const string benchmarkFile = "TicketsInsert.xml";
const string testsDataFolder = "ExamplesData";
string filePath = Path.Combine(Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory)?
    .FullName ?? string.Empty, testsDataFolder, benchmarkFile);

Console.ReadKey();


//Пример с использованием XXml
Console.WriteLine(typeof(XmlParser).Assembly.FullName);
var valueString = "";
using var xml = XmlParser.ParseFile(filePath);
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

if (valueString != string.Empty)
{
    Console.WriteLine(valueString);
}
xml.Dispose();
Console.ReadKey();

//Пример с использованием System.Xml.XmlReader
Console.WriteLine(typeof(System.Xml.XmlReader).Assembly.FullName);
var valueStringSystem = "";
using var reader = System.Xml.XmlReader.Create(filePath);
while (reader.Read())
{
    if (reader.NodeType != System.Xml.XmlNodeType.Element || reader.Name != "Rec") continue;
    for (var i = 0; i < reader.AttributeCount; i++)
    {
        reader.MoveToAttribute(i);
        if (reader.Name == "Action" && reader.Value == "X")
        {
            valueStringSystem = reader.Name + ": " + reader.Value;
        }
    }
}
if (valueStringSystem != string.Empty)
{
    Console.WriteLine(valueStringSystem);
}
Console.WriteLine("END");
Console.ReadKey();
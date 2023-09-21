# XXml

## Benchmarks
```
BenchmarkDotNet v0.13.8, Windows 10 (10.0.19045.3324/22H2/2022Update)
11th Gen Intel Core i5-11400 2.60GHz, 1 CPU, 12 logical and 6 physical cores
.NET SDK 6.0.407
  [Host]    : .NET 6.0.21 (6.0.2123.36311), X64 RyuJIT AVX2 DEBUG
  RyuJitX64 : .NET 6.0.21 (6.0.2123.36311), X64 RyuJIT AVX2

Job=RyuJitX64  Jit=RyuJit  Platform=X64  
```
<details>
  <summary>150 mb file size (100000 items)</summary>

| Method               | Mean     | Error   | StdDev   | Ratio | RatioSD | Gen0      | Allocated native memory | Native memory leak | Allocated   | Alloc Ratio |
|--------------------- |---------:|--------:|---------:|------:|--------:|----------:|------------------------:|-------------------:|------------:|------------:|
| XXml                 | 382.2 ms | 7.31 ms | 18.87 ms |  1.00 |    0.00 |         - |              507,294 KB |                  - |     1.02 KB |        1.00 |
| System.Xml.XmlReader | 421.6 ms | 6.24 ms |  5.83 ms |  1.10 |    0.05 | 4000.0000 |                    0 KB |                  - | 29161.63 KB |   28,493.81 |

</details>
<details>
  <summary>800 mb file size (5000000 items)</summary>
  
| Method               | Mean     | Error   | StdDev   | Ratio | RatioSD | Gen0      | Allocated native memory | Native memory leak | Allocated  | Alloc Ratio |
|--------------------- |---------:|--------:|---------:|------:|--------:|----------:|------------------------:|-------------------:|-----------:|------------:|
| XXml                 | 404.4 ms | 8.79 ms | 25.37 ms |  1.00 |    0.00 |         - |              507,294 KB |                  - |    1.02 KB |        1.00 |
| System.Xml.XmlReader | 433.6 ms | 8.62 ms | 12.91 ms |  1.08 |    0.08 | 4000.0000 |                    0 KB |                  - | 29149.8 KB |   28,482.25 |

</details>

## Usage

```csharp
# Get XML from file
var xml = XmlParser.ParseFile(FilePath);

# Get all nodes by type
xml.GetAllNodes(XmlNodeType.ElementNode)
```

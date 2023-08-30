# XXml
```

BenchmarkDotNet v0.13.7, Windows 10 (10.0.19045.3208/22H2/2022Update)
11th Gen Intel Core i5-11400 2.60GHz, 1 CPU, 12 logical and 6 physical cores
.NET SDK 6.0.407
  [Host]    : .NET 6.0.15 (6.0.1523.11507), X64 RyuJIT AVX2 DEBUG
  RyuJitX64 : .NET 6.0.15 (6.0.1523.11507), X64 RyuJIT AVX2

Job=RyuJitX64  Jit=RyuJit  Platform=X64  

```
|               Method |     Mean |   Error |   StdDev | Ratio | RatioSD |      Gen0 |  Allocated | Alloc Ratio |
|--------------------- |---------:|--------:|---------:|------:|--------:|----------:|-----------:|------------:|
|                 XXml | 315.0 ms | 6.25 ms | 13.19 ms |  1.00 |    0.00 |         - |      944 B |        1.00 |
| System.Xml.XmlReader | 403.4 ms | 1.65 ms |  1.38 ms |  1.25 |    0.04 | 4000.0000 | 28020928 B |   29,683.19 |


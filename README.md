# XXml

## Annotation
Simple and universal library for large XML files parsing with acceptable speed  

Supported platforms:  

- Unity >= 18
- .NET Framework 4.8
- .NET Core 3.2
- .NET 5
- .NET 6

Supported OS:
- Windows
- Linux

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
  <summary>Test data item example</summary>

<?xml version="1.0" encoding="UTF-8"?>
    <TICKET xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance"
        xsi:noNamespaceSchemaLocation="MessagesFlow.xsd">

    <Rec Action="I" TransactionDate="2023-07-10T10:30:00" OperationDate="2023-07-10T10:00:00"
         Operation="Operation 1" TicketId="123456789012" TicketKindId="987654321098"
         TicketKindName="Ticket Kind 1" TicketTypeId="246813579024" TicketTypeName="Ticket Type 1"
         CardId="135792468013" Par="Par 1" BINCard="123456" DbsProgrammId="357913456082"
         DbsProgrammName="Discount Program 1" Payment="1000" Price="10.50" DiscountedPrice="9.50"
         Aggregation="246813579035" RRN="987654321" UTRNO="123456789" Amount="10.50"
         BankDate="2023-07-10T00:00:00" DateOfFinancialPayment="2023-07-10" ClearingDate="2023-07-11"
         SettlementDate="2023-07-12" RouteId="135792468035" RouteNumber="123456789" RouteName="Route 1"
         RouteIdentifier="RouteIdentifier 1" TerminalLogicalNumber="987654321" TerminalSerialNumber="135792468"
         BankTerminalId="BankTerminal 1" FirmId="246813579035" MerchantId="357913456024"
         VehicleRegistrationNumber="Vehicle 1" ConductorCard="987654321" ConductorName="Conductor 1"
         PrepaidTicketId="357913456024" OfficialCarrierId="135792468035" OfficialCarrierName="Carrier 1"
         OfficialCarrierInn="987654321" OfficialFsTax="135792468" OfficialFsInn="246813579"
         TransportCarrierId="135792468035" TransportCarrierName="Carrier 2" TransportCarrierInn="987654321"
         TransportFsTax="135792468" TransportFsInn="246813579" MunicipalityId="246813579035"
         MunicipalityName="Municipality 1" OpeningShift="2023-07-10T08:00:00" ClosingShift="2023-07-10T16:00:00"
         FlightNumber="123.45" VehicleNumber="Vehicle Number 1" FinancialTicketDate="2023-07-10T10:00:00"
         FiscalDataId="135792468035" FiscalDataFD="Fiscal Data 1" />
    </TICKET>
    
</details>

<details>
  <summary>150 mb file size (100000 items)</summary>

| Method               | Mean     | Error   | StdDev   | Ratio | RatioSD | Gen0      | Allocated native memory | Native memory leak | Allocated   | Alloc Ratio |
|--------------------- |---------:|--------:|---------:|------:|--------:|----------:|------------------------:|-------------------:|------------:|------------:|
| XXml                 | 382.2 ms | 7.31 ms | 18.87 ms |  1.00 |    0.00 |         - |              507,294 KB |                  - |     1.02 KB |        1.00 |
| System.Xml.XmlReader | 421.6 ms | 6.24 ms |  5.83 ms |  1.10 |    0.05 | 4000.0000 |                    0 KB |                  - | 29161.63 KB |   28,493.81 |

</details>

<details>
  <summary>800 mb file size (500000 items)</summary>
  
| Method               | Mean     | Error   | StdDev   | Ratio | RatioSD | Gen0      | Allocated native memory | Native memory leak | Allocated  | Alloc Ratio |
|--------------------- |---------:|--------:|---------:|------:|--------:|----------:|------------------------:|-------------------:|-----------:|------------:|
| XXml                 | 404.4 ms | 8.79 ms | 25.37 ms |  1.00 |    0.00 |         - |              507,294 KB |                  - |    1.02 KB |        1.00 |
| System.Xml.XmlReader | 433.6 ms | 8.62 ms | 12.91 ms |  1.08 |    0.08 | 4000.0000 |                    0 KB |                  - | 29149.8 KB |   28,482.25 |

</details>

<details>
  <summary>1580 mb file size (1000000 items)</summary>
  
| Method               | Mean     | Error   | StdDev   | Ratio | RatioSD | Gen0      | Allocated native memory | Native memory leak | Allocated  | Alloc Ratio |
|--------------------- |---------:|--------:|---------:|------:|--------:|----------:|------------------------:|-------------------:|-----------:|------------:|
| XXml                 | 384.9 ms | 7.69 ms | 15.70 ms |  1.00 |    0.00 |         - |              507,294 KB |                  - |    1.02 KB |        1.00 |
| System.Xml.XmlReader | 424.1 ms | 8.39 ms |  7.85 ms |  1.10 |    0.04 | 4000.0000 |                    0 KB |                  - | 29149.8 KB |   28,482.25 |

</details>

<details>
  <summary>2150 mb file size (2000000 items)</summary>
  
| Method               | Mean     | Error   | StdDev   | Ratio | RatioSD | Gen0      | Allocated native memory | Native memory leak | Allocated   | Alloc Ratio |
|--------------------- |---------:|--------:|---------:|------:|--------:|----------:|------------------------:|-------------------:|------------:|------------:|
| XXml                 | 389.2 ms | 7.71 ms | 20.19 ms |  1.00 |    0.00 |         - |              507,294 KB |                  - |     1.02 KB |        1.00 |
| System.Xml.XmlReader | 428.4 ms | 8.43 ms | 12.87 ms |  1.09 |    0.07 | 4000.0000 |                    0 KB |                  - | 29150.01 KB |   28,482.45 |

</details>

## Usage

```csharp
# Get XML from file
var xml = XmlParser.ParseFile(FilePath);

# Get all nodes by type
xml.GetAllNodes(XmlNodeType.ElementNode)
```

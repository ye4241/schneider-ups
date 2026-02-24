using SchneiderUPS.Models;
using SchneiderUPS.UpsLink;

// Default COM3; override via first command-line argument, e.g. dotnet run -- COM4
var portName = args.Length > 0 ? args[0] : "COM3";

Console.WriteLine("SchneiderUPS UpsLink Demo");
Console.WriteLine("Port: {0}, 2400 8N1", portName);
Console.WriteLine();

var ports = UpsLinkSerialPort.GetPortNames();
if (ports.Length == 0)
{
    Console.WriteLine("No serial ports detected.");
    return 1;
}
Console.WriteLine("Available ports: {0}", string.Join(", ", ports));
Console.WriteLine();

using var client = new UpsLinkClient(portName);

try
{
    client.Open();

    Console.WriteLine("Matching protocol (Y -> SM, Ctrl_A -> model)...");
    var matched = client.MatchProtocol();
    Console.WriteLine("  Match: {0}", matched ? "OK" : "Failed");
    if (matched)
        Console.WriteLine("  Protocol/Model ID: {0}", client.ProtocolId);
    else
    {
        Console.WriteLine("  Querying model directly (Ctrl_A)...");
        var pid = client.GetProtocolId();
        Console.WriteLine("  returnData: {0}", pid);
    }

    Console.WriteLine();

    Console.WriteLine("Query work mode (D2)...");
    var mode = client.GetWorkMode();
    Console.WriteLine("  Work mode: {0}", mode);
    Console.WriteLine();

    Console.WriteLine("Query full work state (QueryWorkInfo)...");
    var info = client.QueryWorkInfo();
    PrintWorkInfo(info);
}
catch (Exception ex)
{
    Console.WriteLine("Error: {0}", ex.Message);
    return 1;
}

return 0;

static void PrintWorkInfo(WorkInfo info)
{
    Console.WriteLine("  ── WorkInfo ──");
    Console.WriteLine("  Time: {0:yyyy-MM-dd HH:mm:ss}", info.CurrentTime);
    Console.WriteLine("  Model: {0}", info.ProtocolId);
    Console.WriteLine("  Mode: {0}", info.WorkMode);
    Console.WriteLine("  Input V: {0} V, Input F: {1} Hz", info.InputVoltage, info.InputFrequency);
    Console.WriteLine("  Output V: {0} V, Output F: {1} Hz", info.OutputVoltage, info.OutputFrequency);
    Console.WriteLine("  Output I: {0} A, Load: {1} %, Apparent: {2}", info.OutputCurrent, info.LoadPercent, info.ApparentLoadPower);
    Console.WriteLine("  Temp: {0} °C", info.Temperature);
    Console.WriteLine("  Battery V: {0} V, Capacity: {1} %, Remain: {2} min", info.BatteryVoltage, info.BatteryCapacity, info.RemainTimeMinutes);
    Console.WriteLine("  Warn: {0}", info.WarnStatus);
    if (!string.IsNullOrEmpty(info.FaultKind))
        Console.WriteLine("  Fault: {0}", info.FaultKind);
    Console.WriteLine("  ─────────────");
}

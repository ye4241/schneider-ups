using SchneiderUPS.Models;

namespace SchneiderUPS.UpsLink;

/// <summary>
/// UpsLink protocol client: protocol detection and status query (maps to UpsLinkUSBCOMProcessor.queryWorkInfo etc.).
/// </summary>
public sealed class UpsLinkClient : IDisposable
{
    private readonly UpsLinkSerialPort _transport;
    private string _protocolId = "";

    public string ProtocolId => _protocolId;
    public string PortName => _transport.PortName;

    public UpsLinkClient(string portName, int baudRate = 2400)
    {
        _transport = new UpsLinkSerialPort(portName, baudRate);
    }

    public void Open() => _transport.Open();
    public void Close() => _transport.Close();
    public void Dispose() => _transport.Dispose();

    /// <summary>
    /// Match protocol: send Y expect "SM", then Ctrl_A to get model (e.g. SPM3K); match succeeds if reply starts with "SP".
    /// </summary>
    public bool MatchProtocol()
    {
        for (var i = 0; i < 3; i++)
        {
            try
            {
                var comm = ExecuteQuick(UpsLinkCommand.Y, 50);
                if (string.Equals(comm, "SM", StringComparison.OrdinalIgnoreCase))
                    break;
            }
            catch { /* ignore */ }
        }

        for (var i = 0; i < 5; i++)
        {
            try
            {
                var dataStr = _transport.ExecuteCommandTry3(UpsLinkCommand.Ctrl_A, 100);
                if (!string.Equals(dataStr, "NA", StringComparison.OrdinalIgnoreCase) &&
                    dataStr.StartsWith("SP", StringComparison.OrdinalIgnoreCase))
                {
                    _protocolId = dataStr;
                    return true;
                }
            }
            catch (IOException) { }
        }
        return false;
    }

    /// <summary>
    /// Single quick command (short timeout), used for handshake Y → "SM".
    /// </summary>
    private string ExecuteQuick(byte[] command, int timeoutMs)
    {
        return _transport.ExecuteCommand(command, timeoutMs);
    }

    /// <summary>
    /// Query protocol/model ID (Ctrl_A), e.g. returnData:SPM3K in logs.
    /// </summary>
    public string GetProtocolId()
    {
        var id = _transport.ExecuteCommandTry3(UpsLinkCommand.Ctrl_A, 100);
        _protocolId = id;
        return id;
    }

    /// <summary>
    /// Query work mode (D2): P/S/Y/L/B/T/F/E/C/D.
    /// </summary>
    public string GetWorkMode()
    {
        var mode = _transport.ExecuteCommandTry3(UpsLinkCommand.D2, 100);
        return MapWorkMode(mode);
    }

    private static string MapWorkMode(string code)
    {
        return code switch
        {
            "P" => "Power on mode",
            "S" => "Standby mode",
            "Y" => "Bypass mode",
            "L" => "Line mode",
            "B" => "Battery mode",
            "T" => "Battery test mode",
            "F" => "Fault mode",
            "E" => "ECO mode",
            "C" => "Converter mode",
            "D" => "Shutdown mode",
            _ => code
        };
    }

    /// <summary>
    /// Full work state query (maps to queryWorkInfo).
    /// </summary>
    public WorkInfo QueryWorkInfo()
    {
        var info = new WorkInfo { CurrentTime = DateTime.Now, ProtocolId = _protocolId };

        var pid = _transport.ExecuteCommandTry3(UpsLinkCommand.Ctrl_A, 100);
        if (!string.Equals(pid, _protocolId, StringComparison.OrdinalIgnoreCase))
        {
            info.WorkMode = "Unknown (protocol mismatch)";
            return info;
        }

        info.ProtocolId = pid;

        var mode = _transport.ExecuteCommandTry3(UpsLinkCommand.D2, 100);
        info.WorkMode = MapWorkMode(mode);

        try
        {
            info.InputVoltage = _transport.ExecuteCommandTry3(UpsLinkCommand.L, 100);
            info.InputFrequency = _transport.ExecuteCommandTry3(UpsLinkCommand.D3, 100);
            info.OutputVoltage = _transport.ExecuteCommandTry3(UpsLinkCommand.O, 100);
            info.OutputFrequency = _transport.ExecuteCommandTry3(UpsLinkCommand.F, 100);
            info.OutputCurrent = _transport.ExecuteCommandTry3(UpsLinkCommand.fs, 100);
            info.LoadPercent = _transport.ExecuteCommandTry3(UpsLinkCommand.P, 100);
            info.ApparentLoadPower = _transport.ExecuteCommandTry3(UpsLinkCommand.bs, 100);
            info.Temperature = _transport.ExecuteCommandTry3(UpsLinkCommand.C, 100);
            info.WarnStatus = _transport.ExecuteCommandTry3(UpsLinkCommand.D9, 100);
            info.BatteryCapacity = _transport.ExecuteCommandTry3(UpsLinkCommand.f, 100);
            info.BatteryVoltage = _transport.ExecuteCommandTry3(UpsLinkCommand.B, 100);
            info.RemainTimeMinutes = _transport.ExecuteCommandTry3(UpsLinkCommand.j, 100);
        }
        catch (IOException ex)
        {
            info.FaultKind = $"Read error: {ex.Message}";
        }

        if (string.Equals(info.WorkMode, "Fault mode", StringComparison.OrdinalIgnoreCase))
        {
            try
            {
                info.FaultKind = _transport.ExecuteCommandTry3(UpsLinkCommand.D8, 100);
            }
            catch { info.FaultKind = "?"; }
        }

        return info;
    }
}

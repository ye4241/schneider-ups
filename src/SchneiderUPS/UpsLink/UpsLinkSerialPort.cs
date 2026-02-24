using System.IO.Ports;
using System.Text;

namespace SchneiderUPS.UpsLink;

/// <summary>
/// UpsLink serial transport: 2400 8N1; send command bytes, read until \r\n.
/// </summary>
public sealed class UpsLinkSerialPort : IDisposable
{
    private readonly SerialPort _port;
    private const int DefaultBaudRate = 2400;
    private const int DefaultReadTimeoutMs = 3000;
    private const int DefaultWriteTimeoutMs = 1000;

    public string PortName => _port.PortName;

    public UpsLinkSerialPort(string portName, int baudRate = DefaultBaudRate)
    {
        _port = new SerialPort(portName, baudRate, Parity.None, 8, StopBits.One)
        {
            ReadTimeout = DefaultReadTimeoutMs,
            WriteTimeout = DefaultWriteTimeoutMs,
            Encoding = Encoding.ASCII
        };
    }

    public void Open()
    {
        if (_port.IsOpen) return;
        _port.Open();
        ClearBuffer();
    }

    public void Close()
    {
        if (_port.IsOpen)
            _port.Close();
    }

    public void Dispose() => Close();

    /// <summary>
    /// Clear the receive buffer.
    /// </summary>
    public void ClearBuffer()
    {
        try
        {
            while (_port.BytesToRead > 0)
                _port.ReadByte();
        }
        catch { /* ignore */ }
    }

    /// <summary>
    /// Send command bytes (no trailing \r), read until \r\n, return the string in between.
    /// If the reply contains ':', only the part before the colon is returned (same as Java).
    /// </summary>
    public string ExecuteCommand(byte[] command, int timeoutMs = 100)
    {
        if (command is null || command.Length == 0)
            throw new ArgumentNullException(nameof(command));
        ClearBuffer();

        for (var i = 0; i < command.Length; i++)
        {
            _port.Write(command, i, 1);
            _port.BaseStream.Flush();
            Thread.Sleep(timeoutMs);
        }

        var endTime = DateTime.UtcNow.AddMilliseconds(DefaultReadTimeoutMs);
        var sb = new StringBuilder();
        var sawCr = false;
        // Skip first-char echo: ! $ % + ? = * # & | (same as Java)
        var firstCmd = true;
        var skipFirst = new HashSet<int> { 0x21, 0x24, 0x25, 0x2B, 0x3F, 0x3D, 0x2A, 0x23, 0x26, 0x7C };

        while (DateTime.UtcNow < endTime)
        {
            if (_port.BytesToRead == 0)
            {
                Thread.Sleep(10);
                continue;
            }

            var ch = _port.ReadByte();
            if (ch < 0) break;

            if (firstCmd && skipFirst.Contains(ch))
                continue;
            firstCmd = false;

            if (ch == 0x0D)
            {
                sawCr = true;
                continue;
            }
            if (ch == 0x0A && sawCr)
                break;
            sb.Append((char)ch);
        }

        var result = sb.ToString();
        var colon = result.IndexOf(':');
        if (colon > 0)
            result = result[..colon];
        return result.Trim();
    }

    /// <summary>
    /// Execute command, retry up to 3 times on failure.
    /// </summary>
    public string ExecuteCommandTry3(byte[] command, int timeoutMs = 100)
    {
        string last = "BACKEXCEPTION";
        for (var i = 0; i < 3; i++)
        {
            try
            {
                last = ExecuteCommand(command, timeoutMs);
                if (!string.IsNullOrEmpty(last))
                    return last;
            }
            catch (Exception)
            {
                last = "BACKEXCEPTION";
            }
        }
        if (last == "BACKEXCEPTION")
            throw new IOException("BACKEXCEPTION");
        return last;
    }

    /// <summary>
    /// List available serial ports on the system.
    /// </summary>
    public static string[] GetPortNames() => SerialPort.GetPortNames();
}

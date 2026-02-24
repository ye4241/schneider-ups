using System.IO.Ports;
using NModbus;
using NModbus.Serial;
using SchneiderUPS.Models;

namespace SchneiderUPS.Modbus;

/// <summary>
/// Modbus UPS client (optional). Uses NModbus over serial RTU to read holding registers etc.
/// Maps to decompiled ModbusProcessor; register addresses must be configured per device docs.
/// </summary>
public sealed class ModbusUpsClient : IDisposable
{
    private readonly SerialPort _port;
    private readonly IModbusMaster _master;
    private readonly byte _slaveId;

    public ModbusUpsClient(string portName, int baudRate = 9600, byte slaveId = 1)
    {
        _port = new SerialPort(portName, baudRate, Parity.None, 8, StopBits.One)
        {
            ReadTimeout = 2000,
            WriteTimeout = 1000
        };
        _slaveId = slaveId;
        var adapter = new SerialPortAdapter(_port);
        var factory = new ModbusFactory();
        _master = factory.CreateRtuMaster(adapter);
    }

    public void Open()
    {
        if (!_port.IsOpen)
            _port.Open();
    }

    public void Close()
    {
        if (_port.IsOpen)
            _port.Close();
    }

    public void Dispose() => Close();

    /// <summary>
    /// Read holding registers (function 0x03). Start address and count per device documentation.
    /// </summary>
    public ushort[] ReadHoldingRegisters(ushort startAddress, ushort count)
    {
        return _master.ReadHoldingRegisters(_slaveId, startAddress, count);
    }

    /// <summary>
    /// Write single register (function 0x06) for control (e.g. shutdown). Address/value per ModbusProcessor excuteCommand(register, value).
    /// </summary>
    public void WriteSingleRegister(ushort address, ushort value)
    {
        _master.WriteSingleRegister(_slaveId, address, value);
    }
}

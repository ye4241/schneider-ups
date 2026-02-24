# SchneiderUPS .NET

A .NET library and sample for communicating with Schneider/Voltronic-style UPS devices over serial (UpsLink protocol) or Modbus RTU.

## Requirements

- [.NET 10.0 SDK](https://dot.net)

## Solution structure

| Project              | Description                                      |
|----------------------|--------------------------------------------------|
| **SchneiderUPS**     | Class library: UpsLink client, Modbus client, models |
| **SchneiderUPS.Sample** | Console app that demonstrates `UpsLinkClient` usage |

## Library overview

- **UpsLink** – Serial protocol (2400 8N1): protocol detection (Y → SM, Ctrl+A → model ID), work mode, and full status query (`WorkInfo`). Maps to the decompiled Java `UpsLinkUSBCOMProcessor` / `SerialPortHandler` behavior.
- **Modbus** – Optional Modbus RTU client via [NModbus](https://github.com/NModbus/NModbus) for devices that expose Modbus registers.
- **Models** – `WorkInfo` and related DTOs for status (voltage, frequency, load, battery, faults, etc.).

## Build

```bash
cd csharp
dotnet build SchneiderUPS.slnx
```

## Run the sample

Default port is `COM3`; pass the port name as the first argument to override:

```bash
dotnet run --project SchneiderUPS.Sample
# Or specify port:
dotnet run --project SchneiderUPS.Sample -- COM4
```

The sample will list available serial ports, open the given port, match the UpsLink protocol, query work mode, and print full `WorkInfo` (input/output voltage and frequency, load, temperature, battery voltage/capacity/remaining time, warnings, faults).

## Serial settings

- **UpsLink**: 2400 baud, 8 data bits, no parity, 1 stop bit (2400 8N1). Commands are sent as raw bytes (no trailing `\r`); device replies end with `\r\n`.
- **Modbus**: Configurable (default 9600 8N1); register addresses must be set per device documentation.

## License

See repository root.

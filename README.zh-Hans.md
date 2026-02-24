# SchneiderUPS .NET

基于 .NET 的库与示例，用于通过串口（UpsLink 协议）或 Modbus RTU 与施耐德/Voltronic 系 UPS 设备通信。

## 环境要求

- [.NET 10.0 SDK](https://dot.net)

## 解决方案结构

| 项目                | 说明                                               |
|---------------------|----------------------------------------------------|
| **SchneiderUPS**    | 类库：UpsLink 客户端、Modbus 客户端、数据模型     |
| **SchneiderUPS.Sample** | 控制台示例，演示 `UpsLinkClient` 的用法        |

## 库概览

- **UpsLink** – 串口协议（2400 8N1）：协议识别（Y→SM、Ctrl+A→机型）、工作模式及完整状态查询（`WorkInfo`），行为对应反编译后的 Java `UpsLinkUSBCOMProcessor` / `SerialPortHandler`。
- **Modbus** – 通过 [NModbus](https://github.com/NModbus/NModbus) 的 Modbus RTU 客户端，供支持 Modbus 寄存器的设备使用。
- **Models** – `WorkInfo` 等状态 DTO（电压、频率、负载、电池、故障等）。

## 编译

```bash
cd csharp
dotnet build SchneiderUPS.slnx
```

## 运行示例

默认使用 `COM3`；可通过第一个命令行参数指定串口：

```bash
dotnet run --project SchneiderUPS.Sample
# 或指定串口：
dotnet run --project SchneiderUPS.Sample -- COM4
```

示例会列出可用串口、打开指定串口、进行 UpsLink 协议匹配、查询工作模式并打印完整 `WorkInfo`（输入/输出电压与频率、负载、温度、电池电压/容量/剩余时间、告警、故障等）。

## 串口配置

- **UpsLink**：2400 波特率，8 数据位，无校验，1 停止位（2400 8N1）。命令以原始字节发送（不带 `\r`）；设备回复以 `\r\n` 结尾。
- **Modbus**：可配置（默认 9600 8N1）；寄存器地址需按设备文档设置。

## 许可证

见仓库根目录。

namespace SchneiderUPS.Models;

/// <summary>
/// UPS work state (maps to decompiled WorkInfo / queryWorkInfo).
/// </summary>
public class WorkInfo
{
    public DateTime CurrentTime { get; set; }
    /// <summary>Protocol/model ID, e.g. SPM3K.</summary>
    public string ProtocolId { get; set; } = "";
    /// <summary>Work mode: Power on / Standby / Bypass / Line / Battery / Fault / ECO / Shutdown etc.</summary>
    public string WorkMode { get; set; } = "";
    public string InputVoltage { get; set; } = "";
    public string InputFrequency { get; set; } = "";
    public string OutputVoltage { get; set; } = "";
    public string OutputFrequency { get; set; } = "";
    public string OutputCurrent { get; set; } = "";
    public string LoadPercent { get; set; } = "";
    public string ApparentLoadPower { get; set; } = "";
    public string Temperature { get; set; } = "";
    public string BatteryVoltage { get; set; } = "";
    public string BatteryCapacity { get; set; } = "";
    public string RemainTimeMinutes { get; set; } = "";
    public string FaultKind { get; set; } = "";
    public string WarnStatus { get; set; } = "";
}

namespace SchneiderUPS.UpsLink;

/// <summary>
/// UpsLink binary protocol command bytes (maps to decompiled UpsLinkCommand.java).
/// Send bytes only, no trailing \r; device replies end with \r\n.
/// </summary>
public static class UpsLinkCommand
{
    // Protocol/model query → returns "SPM3K" etc.
    public static readonly byte[] Ctrl_A = [0x01];
    // Handshake → expect "SM"
    public static readonly byte[] Y = [0x59];
    // Work mode → P/S/Y/L/B/T/F/E/C/D
    public static readonly byte[] D2 = [0x9F, 0xD2];
    public static readonly byte[] D1 = [0x9F, 0xD1];
    public static readonly byte[] D3 = [0x9F, 0xD3];
    public static readonly byte[] D4 = [0x9F, 0xD4];
    public static readonly byte[] D5 = [0x9F, 0xD5];
    public static readonly byte[] D6 = [0x9F, 0xD6];
    public static readonly byte[] D7 = [0x9F, 0xD7];
    public static readonly byte[] D8 = [0x9F, 0xD8];
    public static readonly byte[] D9 = [0x9F, 0xD9];

    // Single-byte queries
    public static readonly byte[] L = [0x4C];   // Input voltage
    public static readonly byte[] O = [0x4F];   // Output voltage
    public static readonly byte[] F = [0x46];   // Output frequency
    public static readonly byte[] P = [0x50];   // Load percent
    public static readonly byte[] B = [0x42];   // Battery voltage
    public static readonly byte[] C = [0x43];   // Temperature
    public static readonly byte[] f = [0x66];   // Battery capacity
    public static readonly byte[] j = [0x6A];   // Remain time
    public static readonly byte[] fs = [0x2F];  // Output current
    public static readonly byte[] bs = [0x5C];  // Apparent power

    public static readonly byte[] S = [0x53];   // Shutdown related
    public static readonly byte[] K = [0x4B];   // Buzzer/cancel
    public static readonly byte[] E2 = [0x9F, 0xE2];
    public static readonly byte[] gt = [0x3E];  // Battery group etc.
    public static readonly byte[] c = [0x63];   // Serial number
    public static readonly byte[] Ctrl_Z = [0x1A];
}

using System.Runtime.InteropServices;

namespace SellSn.Client.OpenVPN.Models;

[StructLayout(LayoutKind.Sequential)]
internal struct JobobjectExtendedLimitInformation
{
    public JobobjectBasicLimitInformation BasicLimitInformation;
    private readonly IoCounters IoInfo;
    private readonly nuint ProcessMemoryLimit;
    private readonly nuint JobMemoryLimit;
    private readonly nuint PeakProcessMemoryUsed;
    private readonly nuint PeakJobMemoryUsed;
}
using System.Runtime.InteropServices;

namespace SellSn.Client.OpenVPN.Models;

[StructLayout(LayoutKind.Sequential)]
internal struct JobobjectBasicLimitInformation
{
    private readonly long PerProcessUserTimeLimit;
    private readonly long PerJobUserTimeLimit;
    internal Jobobjectlimit LimitFlags;
    private readonly nuint MinimumWorkingSetSize;
    private readonly nuint MaximumWorkingSetSize;
    private readonly uint ActiveProcessLimit;
    private readonly long Affinity;
    private readonly uint PriorityClass;
    private readonly uint SchedulingClass;
}
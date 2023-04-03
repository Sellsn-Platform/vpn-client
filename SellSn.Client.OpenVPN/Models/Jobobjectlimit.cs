using System;

namespace SellSn.Client.OpenVPN.Models;

[Flags]
internal enum Jobobjectlimit : uint
{
    JobObjectLimitKillOnJobClose = 0x2000
}
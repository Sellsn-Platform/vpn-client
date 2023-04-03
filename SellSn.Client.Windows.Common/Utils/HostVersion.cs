﻿using System.Linq;
using System.Management;
using System.Runtime.Versioning;

namespace SellSn.Client.Windows.Common.Utils;

[SupportedOSPlatform("windows")]
public static class HostVersion
{
    public static string GetOsVersion()
    {
        var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_OperatingSystem");
        var info = searcher.Get().Cast<ManagementObject>().FirstOrDefault();
        var version = info?.Properties["Version"].Value.ToString();

        return $"win64 {version}";
    }
}
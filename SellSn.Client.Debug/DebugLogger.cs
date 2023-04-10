using System;
using System.IO;

namespace SellSn.Client.Debug;

public static class DebugLogger
{
    public static readonly string DebugLogLocation = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "SellSN", "debug.log");

    public static void Write(string source, string log)
    {
        if (!Directory.Exists(Path.GetDirectoryName(DebugLogLocation)))
            Directory.CreateDirectory(Path.GetDirectoryName(DebugLogLocation)!);

        File.AppendAllText(DebugLogLocation, $"[{source} at {DateTime.Now}]: {log}\r\n");
    }
}
using System.ComponentModel;

namespace SellSn.Client.Windows.Models;

/// <summary>
///     Contains all the possible theme combinations
/// </summary>
internal enum BackgroundMode
{
    [Description("Dark mode")] Dark,
    [Description("Light mode")] Light
}
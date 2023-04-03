using System.ComponentModel;
using SellSn.Client.Windows.Converters;

namespace SellSn.Client.Windows.Models;

[TypeConverter(typeof(EnumDescriptionTypeConverter))]
internal enum ConnectionState
{
    [Description("Disconnected")] Disconnected,
    [Description("Disconnecting...")] Disconnecting,
    [Description("Connecting...")] Connecting,
    [Description("Connected!")] Connected
}
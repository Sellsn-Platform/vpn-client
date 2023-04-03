using System;
using System.ComponentModel;
using SellSn.Client.Windows.Common.Models;

namespace SellSn.Client.Windows.Services.Models;

public sealed class ServerCache
{
    public BindingList<DisplayVpnServer> Servers { get; set; }
    public DateTime LastCache { get; set; }
}
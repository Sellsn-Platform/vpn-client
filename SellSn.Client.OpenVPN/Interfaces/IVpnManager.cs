﻿using System;
using System.Threading;
using System.Threading.Tasks;
using SellSn.Client.OpenVPN.EventArgs;
using SellSn.Client.OpenVPN.Utils;

namespace SellSn.Client.OpenVPN.Interfaces;

public interface IVpnManager
{
    delegate void Connected(object sender, ConnectedEventArgs e);

    delegate void ErrorReceived(object sender, ErrorEventArgs e);

    delegate void OutputReceived(object sender, OutputReceivedEventArgs e);

    bool IsConnected { get; }
    bool IsConnecting { get; }
    bool IsDisposed { get; }
    string ConfigurationPath { get; }
    TapManager TapManager { get; init; }

    event OutputReceived OnOutputReceived;
    event ErrorReceived OnErrorReceived;
    event Connected OnConnected;
    Task DisconnectAsync(bool graceful = true, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Connects to the server provided in the configuration file
    /// </summary>
    /// <param name="configurationPath">Path to the OpenVPN configuration file</param>
    /// <param name="cancellationToken"></param>
    /// <exception cref="InvalidOperationException">
    ///     Thrown when attempting to connect whilst connected or connecting or if the
    ///     configuration file doesn't exist
    /// </exception>
    Task ConnectAsync(string configurationPath, CancellationToken cancellationToken = default);

    ValueTask DisposeAsync();
}
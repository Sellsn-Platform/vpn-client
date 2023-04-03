﻿using System;
using System.IO;
using System.Linq;
using System.Security.Authentication;
using System.Threading;
using System.Threading.Tasks;
using LightVPN.Client.Debug;
using SellSn.Client.OpenVPN.Exceptions;
using SellSn.Client.OpenVPN.Interfaces;
using SellSn.Client.Windows.Common;
using SellSn.Client.Windows.Configuration.Interfaces;
using SellSn.Client.Windows.Configuration.Models;
using SellSn.Client.Windows.Services.Interfaces;

namespace SellSn.Client.Windows.Services;

/// <inheritdoc />
/// <summary>
///     Essentially a Windows only wrapper for the OpenVPN manager class
/// </summary>
public sealed class OpenVpnService : IOpenVpnService
{
    /// <inheritdoc />
    /// <summary>
    ///     Locates a OpenVPN configuration file in the cache and tells the OpenVPN manager to connect to it.
    /// </summary>
    /// <param name="location">Server location</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <param name="id">Server ID</param>
    /// <exception cref="T:System.InvalidOperationException">
    ///     Thrown when attempting to connect whilst connected or connecting
    ///     or if the configuration file doesn't exist
    /// </exception>
    /// <exception cref="AuthenticationException">Thrown when the authentication process fails</exception>
    /// <exception cref="FileLoadException">Thrown when OpenVPN rejects a configuration file for whatever reason</exception>
    /// <exception cref="UnknownErrorException">Thrown when OpenVPN spits out Unknown error into stdout</exception>
    /// <exception cref="TimeoutException">Thrown when the connection to a VPN server times out</exception>
    /// <returns></returns>
    public async Task ConnectAsync(string id, string location, CancellationToken cancellationToken = default)
    {
        var vpnManager = Globals.Container.GetInstance<IVpnManager>();

        var files = Directory.GetFiles(Globals.AppOpenVpnCachePath);

        if (files.Length == 0 || !files.Any(x => x.Contains(id)))
        {
            DebugLogger.Write("lvpn-client-services-ovpnwrapper",
                "oh shit!?! looks like the system cannot locate the config, what now!??! that's right sir, we throw a phat exception");
            throw new InvalidOperationException(
                "Cannot fetch the server configuration, you may need to refresh your cache, you can do so in settings.");
        }

        var configFileName = files.FirstOrDefault(x => x.Contains(id));
        if (string.IsNullOrWhiteSpace(configFileName))
        {
            DebugLogger.Write("lvpn-client-services-ovpnwrapper",
                "looks like this system has done some sorcery and the filename found is null or whitespaces, wtf!?!?!?!");

            return;
        }

        var manager = Globals.Container.GetInstance<IConfigurationManager<AppConfiguration>>();
        var currentConfig = await manager.ReadAsync(cancellationToken);

        currentConfig.LastServer = new AppLastServer
        {
            Location = location,
            PritunlName = id
        };

        await manager.WriteAsync(currentConfig, cancellationToken);

        await vpnManager.ConnectAsync(configFileName, cancellationToken);
    }
}
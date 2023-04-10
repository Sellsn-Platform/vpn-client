﻿using System;
using System.Diagnostics;
using System.IO;
using System.Management;
using System.Runtime.Versioning;
using System.Threading;
using System.Threading.Tasks;
using SellSn.Client.Debug;
using SellSn.Client.OpenVPN.Models;
using SellSn.Client.OpenVPN.Resources;
using SellSn.Client.Windows.Common;
using ErrorEventArgs = SellSn.Client.OpenVPN.EventArgs.ErrorEventArgs;

namespace SellSn.Client.OpenVPN.Utils;

/// <summary>
///     Manages the TAP adapter and it's drivers, this class is only functional on Windows
/// </summary>
[SupportedOSPlatform("windows")]
public sealed class TapManager
{
    public delegate void ErrorReceived(object sender, ErrorEventArgs e);

    public delegate void Success(object sender);

    private readonly OpenVpnConfiguration _configuration;
    private Process _tapCtlProcess;

    internal TapManager(OpenVpnConfiguration configuration)
    {
        _configuration = configuration;

        ConstructProcess();
    }

    public event Success OnSuccess;
    public event ErrorReceived OnErrorReceived;

    /// <summary>
    ///     Starts the TAP process and starts redirecting stdout
    /// </summary>
    private void StartProcess()
    {
        _tapCtlProcess.Start();

        _tapCtlProcess.BeginErrorReadLine();
        _tapCtlProcess.BeginOutputReadLine();
    }

    /// <summary>
    ///     Configures the process for TAP driver installation
    /// </summary>
    private void ConfigureTapDriverInstallation()
    {
        _tapCtlProcess.StartInfo.WorkingDirectory = Globals.OpenVpnDriversPath;
        _tapCtlProcess.StartInfo.FileName = Path.Combine(Globals.OpenVpnDriversPath, "tapinstall.exe");
    }
    
    /// <summary>
    ///     Configures the process for TAP adapter installation
    /// </summary>
    private void ConfigureTapAdapterInstallation()
    {
        _tapCtlProcess.StartInfo.WorkingDirectory = Globals.OpenVpnPath;
        _tapCtlProcess.StartInfo.FileName = Path.Combine(Globals.OpenVpnPath, "tapctl.exe");
    }

    /// <summary>
    ///     Constructs the default process object
    /// </summary>
    /// <exception cref="ArgumentNullException">Thrown when the OpenVpnPath is null</exception>
    private void ConstructProcess()
    {
        _tapCtlProcess = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                CreateNoWindow = true,
                FileName = _configuration.TapCtlPath,
                Verb = "runas",
                WindowStyle = ProcessWindowStyle.Hidden,
                WorkingDirectory = Path.GetDirectoryName(_configuration.OpenVpnPath) ??
                                   throw new ArgumentNullException(nameof(_configuration)),
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false
            }
        };

        _tapCtlProcess.OutputDataReceived += TapCtlProcessOnOutputDataReceived;
        _tapCtlProcess.ErrorDataReceived += TapCtlProcessOnErrorDataReceived;
    }

    /// <summary>
    ///     Installs the TAP adapter driver asynchronously
    /// </summary>
    /// <param name="cancellationToken">The cancellation token</param>
    public async Task InstallTapDriverAsync(CancellationToken cancellationToken = default)
    {
        ConfigureTapDriverInstallation();

        _tapCtlProcess.StartInfo.Arguments = "install OemVista.inf tap0901";

        StartProcess();
        await BeginTerminatingProcessAsync(cancellationToken);
    }

    /// <summary>
    ///     Removes the TAP adapter driver asynchronously
    /// </summary>
    /// <param name="cancellationToken"></param>
    public async Task RemoveTapDriverAsync(CancellationToken cancellationToken = default)
    {
        ConfigureTapDriverInstallation();

        _tapCtlProcess.StartInfo.Arguments = "remove tap0901";

        StartProcess();
        await BeginTerminatingProcessAsync(cancellationToken);
    }

    /// <summary>
    ///     Checks if the TAP driver has been installed on the system
    /// </summary>
    /// <returns>True if it has been, false otherwise</returns>
    public static bool IsDriverInstalled()
    {
        var found = false;

        var query = new SelectQuery("SELECT * FROM Win32_PnPSignedDriver");

        using var searcher = new ManagementObjectSearcher(query);

        foreach (var item in searcher.Get())
        {
            if (found) break;

            var name = item["Description"]?.ToString();
            found = name == "TAP-Windows Adapter V9";
        }

        return found;
    }

    /// <summary>
    ///     Begins to wait for the process to exit and then cancels stdout redirection
    /// </summary>
    /// <param name="cancellationToken">The cancellation token</param>
    private async Task BeginTerminatingProcessAsync(CancellationToken cancellationToken = default)
    {
        await _tapCtlProcess.WaitForExitAsync(cancellationToken);

        _tapCtlProcess.CancelOutputRead();
        _tapCtlProcess.CancelErrorRead();
    }

    /// <summary>
    ///     Installs a TAP adapter on the system with the specified TAP adapter name
    /// </summary>
    /// <param name="cancellationToken">The cancellation token</param>
    public async Task InstallTapAdapterAsync(CancellationToken cancellationToken = default)
    {
        ConfigureTapAdapterInstallation();
        _tapCtlProcess.StartInfo.Arguments = $"create";

        StartProcess();
        await BeginTerminatingProcessAsync(cancellationToken);
    }

    /// <summary>
    ///     Removes a TAP adapter from the system with the specified TAP adapter name
    /// </summary>
    /// <param name="cancellationToken">The cancellation token</param>
    public async Task RemoveTapAdapterAsync(CancellationToken cancellationToken = default)
    {
        ConfigureTapAdapterInstallation();
        _tapCtlProcess.StartInfo.Arguments = $"delete {_configuration.TapAdapterName}";

        StartProcess();
        await BeginTerminatingProcessAsync(cancellationToken);
    }

    /// <summary>
    ///     Checks if the TAP adapter specified is existent on the system
    /// </summary>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>True if it is, false otherwise</returns>
    public async Task<bool> IsAdapterExistentAsync(CancellationToken cancellationToken = default)
    {
        ConfigureTapAdapterInstallation();
        var found = false;

        _tapCtlProcess.StartInfo.Arguments = "list";

        _tapCtlProcess.OutputDataReceived -= TapCtlProcessOnOutputDataReceived;

        _tapCtlProcess.OutputDataReceived += (_, args) =>
        {
            if (args.Data?.Contains(_configuration.TapAdapterName) == true && !found) found = true;
        };

        _tapCtlProcess.OutputDataReceived += TapCtlProcessOnOutputDataReceived;

        StartProcess();

        await BeginTerminatingProcessAsync(cancellationToken);

        return found;
    }

    /// <summary>
    ///     Fired when error data is received from the tapctl process
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    /// <exception cref="InvalidOperationException">Thrown when TAP creation has failed</exception>
    private void TapCtlProcessOnErrorDataReceived(object sender, DataReceivedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(e.Data)) return;

        DebugLogger.Write("svpn-client-ovpn-tapman", $"create_vpn_adap failed: {e.Data}");

        _tapCtlProcess.Kill();

        OnErrorReceived?.Invoke(this,
            new ErrorEventArgs(
                new InvalidOperationException(
                    "Failed to create VPN adapter. Your OpenVPN drivers are most likely corrupt.")));
    }

    /// <summary>
    ///     Fired when normal output data is received from the tapctl process, any errors are handled in this method
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    /// <exception cref="InvalidOperationException">Thrown when something goes wrong</exception>
    private void TapCtlProcessOnOutputDataReceived(object sender, DataReceivedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(e.Data)) return;

        switch (e.Data)
        {
            case var str
                when str.Contains(string.Format(StringTable.OVPN_TAP_ALREADY_EXISTS,
                    _configuration.TapAdapterName)):
                OnErrorReceived?.Invoke(this,
                    new ErrorEventArgs(
                        new InvalidOperationException("TAP adapter already exists!")));
                break;
            case var str
                when str.Contains(string.Format(StringTable.OVPN_TAP_NO_EXISTS,
                    _configuration.TapAdapterName)):
                OnErrorReceived?.Invoke(this,
                    new ErrorEventArgs(
                        new InvalidOperationException("TAP adapter doesn't exist!")));
                break;
            case var guid when Guid.TryParse(guid, out var _):
                DebugLogger.Write("svpn-client-ovpn-tapman", "tapctl exited, tap created it seems.");
                OnSuccess?.Invoke(this);
                break;
        }
    }
}
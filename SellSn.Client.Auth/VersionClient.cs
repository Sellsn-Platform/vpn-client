using System.Diagnostics;
using SellSn.Client.Auth.Interfaces;

namespace SellSn.Client.Auth;

public class VersionClient : IVersionClient
{
    private readonly IApiClient _apiClient;

    public VersionClient(IApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    /// <summary>
    ///     Checks and installs any available client updates
    /// </summary>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>True if a client update was detected, downloaded and executed, false otherwise.</returns>
    public async Task<bool> CheckAndInstallUpdatesAsync(CancellationToken cancellationToken = default)
    {
        // Check that we are actually up-to-date
        var valid = await _apiClient.CheckVersionAsync(cancellationToken);
        if (valid) return false;
        
        // Get the installer bytes from the API
        var installerBytes = await _apiClient.GetWindowsInstallerAsync(cancellationToken);

        // Get a temp location and write the binary data to the location
        var installerLocation = Path.GetTempFileName() + ".exe";
        await File.WriteAllBytesAsync(installerLocation, installerBytes, cancellationToken);

        // Execute the file
        Process.Start(installerLocation);

        // Return true state
        return true;

    }
}
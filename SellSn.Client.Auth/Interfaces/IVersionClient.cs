namespace SellSn.Client.Auth.Interfaces;

public interface IVersionClient
{
    /// <summary>
    ///     Checks and installs any available client updates
    /// </summary>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>True if a client update was detected, downloaded and executed, false otherwise.</returns>
    Task<bool> CheckAndInstallUpdatesAsync(CancellationToken cancellationToken = default);
}
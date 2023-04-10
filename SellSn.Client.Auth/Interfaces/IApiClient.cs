using System.Net;
using SellSn.Client.Windows.Common.Models;

namespace SellSn.Client.Auth.Interfaces;

public interface IApiClient
{
    Task<Profile?> GetProfileAsync(CancellationToken cancellationToken = default);
    Task<string?> GetOAuthUrlAsync(CancellationToken cancellationToken = default);
    string GetCachedSessionCookie();
    void SetCachedSessionCookie(Cookie cookie);
    Task<List<DisplayVpnServer>> GetServersAsync(CancellationToken cancellationToken = default);
    Task<byte[]> GetOpenVpnArchiveAsync(CancellationToken cancellationToken = default);
    Task<byte[]> GetDriverArchiveAsync(CancellationToken cancellationToken = default);
    Task<byte[]> GetConfigArchiveAsync(CancellationToken cancellationToken = default);
    Task<byte[]> GetWindowsInstallerAsync(CancellationToken cancellationToken = default);
    Task<bool> CheckVersionAsync(CancellationToken cancellationToken = default);
}
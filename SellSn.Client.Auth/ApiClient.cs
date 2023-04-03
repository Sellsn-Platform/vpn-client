using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using SellSn.Client.Auth.Interfaces;
using SellSn.Client.Auth.Models;
using SellSn.Client.Cryptography;
using SellSn.Client.Windows.Common;
using SellSn.Client.Windows.Common.Models;

namespace SellSn.Client.Auth;

public class ApiClient : IApiClient
{
    private readonly CookieContainer _cookieContainer = new();
    public const string WebUrl = "https://141.95.19.80/";
    private readonly HttpClient _client;

    public ApiClient()
    {
        _client = new(new HttpClientHandler
        {
            CookieContainer = _cookieContainer,
            UseCookies = true,
            ServerCertificateCustomValidationCallback = (_, _, _, _) => true 
        });

        _client.BaseAddress = new Uri(WebUrl + "api/");
        _client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("SellSN-VPN", "1.0.0"));

        GetCachedSessionCookie();
    }

    public async Task<List<DisplayVpnServer>> GetServersAsync(CancellationToken cancellationToken = default)
    {
        var res = await _client.GetFromJsonAsync<List<VpnServer>>("servers");
        return res?.Select(x => new DisplayVpnServer
        {
            Country = x.Country,
            Flag = x.CountryCode,
            Id = x.Id,
            ServerName = x.Name,
            Status = "Online"
        }).ToList() ?? new List<DisplayVpnServer>();
    }

    public async Task<Profile?> GetProfileAsync(CancellationToken cancellationToken = default)
    {
        return await _client.GetFromJsonAsync<Profile>("profile", cancellationToken: cancellationToken);
    }

    public async Task<string?> GetOAuthUrlAsync(CancellationToken cancellationToken = default)
    {
        var res = await _client.GetFromJsonAsync<OAuthUrlResponse>("oauth", cancellationToken: cancellationToken);
        return res?.Url;
    }

    public async Task<byte[]> GetOpenVpnArchiveAsync(CancellationToken cancellationToken = default)
    {
        var res = await _client.GetByteArrayAsync("download?platform=ovpn", cancellationToken);
        return res;
    }
    
    public async Task<byte[]> GetDriverArchiveAsync(CancellationToken cancellationToken = default)
    {
        var res = await _client.GetByteArrayAsync("download?platform=drivers", cancellationToken);
        return res;
    }
    
    public async Task<byte[]> GetConfigArchiveAsync(CancellationToken cancellationToken = default)
    {
        var res = await _client.GetByteArrayAsync("download?platform=conf", cancellationToken);
        return res;
    }

    public string GetCachedSessionCookie()
    {
        try
        {
            var cipherText = File.ReadAllBytes(Globals.AuthDataPath);
            var cookieHeader = Aes.Decrypt(cipherText);
        
            _cookieContainer.SetCookies(new Uri(WebUrl), cookieHeader);
        
            return cookieHeader;
        }
        catch (Exception)
        {
            // ignored.
            return null;
        }
    }

    public void SetCachedSessionCookie(Cookie cookie)
    {
        var cookies = new CookieCollection { cookie };
        _cookieContainer.Add(cookies);
        
        var cipherText = Aes.Encrypt(_cookieContainer.GetCookieHeader(new Uri(WebUrl)));
        File.WriteAllBytes(Globals.AuthDataPath, cipherText);
    }
}
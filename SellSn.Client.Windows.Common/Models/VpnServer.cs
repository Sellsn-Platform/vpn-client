namespace SellSn.Client.Windows.Common.Models;

public class VpnServer
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Country { get; set; }
    public string CountryCode { get; set; }

    public static implicit operator DisplayVpnServer(VpnServer server) => new()
    {
        ServerName = server.Name,
        Status = "Online",
        Country = server.Country,
        Flag = server.CountryCode,
        Id = server.Id
    };
}
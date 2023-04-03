using System;
using System.Linq;
using System.Windows;
using System.Windows.Forms;
using Microsoft.Web.WebView2.Core;
using SellSn.Client.Auth;
using SellSn.Client.Auth.Interfaces;
using SellSn.Client.Windows.ViewModels;
using MessageBox = System.Windows.MessageBox;

namespace SellSn.Client.Windows.Windows;

public partial class OAuthWindow : Window
{
    private readonly IApiClient _apiClient;
    
    public OAuthWindow(IApiClient apiClient)
    {
        DataContext = new WindowViewModel(this, false);
        
        _apiClient = apiClient;
        InitializeComponent();
        WebView.NavigationCompleted += WebViewOnNavigationCompleted;
        Loaded += OnLoaded;
    }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        var url = await _apiClient.GetOAuthUrlAsync();
        WebView.Source = new Uri(uriString: url ?? throw new InvalidOperationException());
    }

    private async void WebViewOnNavigationCompleted(object? sender, CoreWebView2NavigationCompletedEventArgs e)
    {
        if (e.IsSuccess && WebView.Source.ToString().Contains(ApiClient.WebUrl))
        {
            var cookies = await WebView.CoreWebView2.CookieManager.GetCookiesAsync(ApiClient.WebUrl);
            var cookie = cookies.FirstOrDefault(x => x.Name == "session");
            if (cookie is null)
            {
                return;
            }

            var netCookie = cookie.ToSystemNetCookie();
            _apiClient.SetCachedSessionCookie(netCookie);
            Close();
        }
    }
}
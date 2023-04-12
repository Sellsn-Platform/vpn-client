using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Shapes;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.Wpf;
using SellSn.Client.Auth;
using SellSn.Client.Auth.Interfaces;
using SellSn.Client.Windows.Common;
using SellSn.Client.Windows.ViewModels;
using MessageBox = System.Windows.MessageBox;

namespace SellSn.Client.Windows.Windows;

public partial class OAuthWindow : Window
{
    private readonly IApiClient _apiClient;
    public static readonly CoreWebView2CreationProperties Properties = new()
    {
        UserDataFolder = Globals.UserDataLocation,
        IsInPrivateModeEnabled = true
    };
    
    public OAuthWindow(IApiClient apiClient)
    {
        Directory.Delete(Globals.UserDataLocation, true);
        DataContext = new WindowViewModel(this, false);

        _apiClient = apiClient;
        
        InitializeComponent();
        
        WebView.ContentLoading += ContentLoading;
        WebView.NavigationCompleted += WebViewOnNavigationCompleted;
    }

    private void ContentLoading(object? sender, CoreWebView2ContentLoadingEventArgs e)
    {
        LoadingIndicator.Visibility = Visibility.Visible;
        WebView.Visibility = Visibility.Collapsed;
    }

    private async void WebViewOnNavigationCompleted(object? sender, CoreWebView2NavigationCompletedEventArgs e)
    {
        if (e.IsSuccess)
        {
            LoadingIndicator.Visibility = Visibility.Collapsed;
            WebView.Visibility = Visibility.Visible;
        }
        
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
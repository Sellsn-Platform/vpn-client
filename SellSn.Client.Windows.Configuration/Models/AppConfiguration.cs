using System;
using System.ComponentModel;
using System.Reflection;

namespace SellSn.Client.Windows.Configuration.Models;

public sealed class AppConfiguration : INotifyPropertyChanged
{
    private bool _isAutoConnectEnabled;

    private bool _isDarkModeEnabled;

    private bool _isDiscordRpcEnabled = true;

    private bool _isKillSwitchEnabled;

    private AppLastServer _lastServer;

    private AppSizeSaving _sizeSaving;

    private ThemeColor _theme;

    public AppLastServer LastServer
    {
        get => _lastServer;
        set
        {
            _lastServer = value;
            OnPropertyChanged(nameof(LastServer));
        }
    }

    public ThemeColor Theme
    {
        get => _theme;
        set
        {
            _theme = value;
            OnPropertyChanged(nameof(Theme));
        }
    }

    public bool IsAutoConnectEnabled
    {
        get => _isAutoConnectEnabled;
        set
        {
            _isAutoConnectEnabled = value;
            OnPropertyChanged(nameof(IsAutoConnectEnabled));
        }
    }

    public bool IsDiscordRpcEnabled
    {
        get => _isDiscordRpcEnabled;
        set
        {
            _isDiscordRpcEnabled = value;
            OnPropertyChanged(nameof(IsDiscordRpcEnabled));
        }
    }

    public bool IsKillSwitchEnabled
    {
        get => _isKillSwitchEnabled;
        set
        {
            _isKillSwitchEnabled = value;
            OnPropertyChanged(nameof(IsKillSwitchEnabled));
        }
    }

    public bool IsDarkModeEnabled
    {
        get => _isDarkModeEnabled;
        set
        {
            _isDarkModeEnabled = value;
            OnPropertyChanged(nameof(IsDarkModeEnabled));
        }
    }

    public AppSizeSaving SizeSaving
    {
        get => _sizeSaving;
        set
        {
            _sizeSaving = value;
            OnPropertyChanged(nameof(SizeSaving));
        }
    }

    public bool IsFirstRun { get; set; } = true;

    public Version ConfigVersion { get; set; } = Assembly.GetEntryAssembly()?.GetName().Version;
    public event PropertyChangedEventHandler PropertyChanged;

    private void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
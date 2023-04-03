using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using MaterialDesignThemes.Wpf;
using SellSn.Client.Auth.Interfaces;
using SellSn.Client.Discord.Interfaces;
using SellSn.Client.OpenVPN.EventArgs;
using SellSn.Client.OpenVPN.Interfaces;
using SellSn.Client.Windows.Common;
using SellSn.Client.Windows.Common.Models;
using SellSn.Client.Windows.Configuration.Interfaces;
using SellSn.Client.Windows.Configuration.Models;
using SellSn.Client.Windows.Models;
using SellSn.Client.Windows.Services.Interfaces;
using SellSn.Client.Windows.Utils;
using SellSn.Client.Windows.Views;

namespace SellSn.Client.Windows.ViewModels;

internal sealed class MainViewModel : WindowViewModel
{
    private ConnectionState _connectionState;

    private string _lastServer;

    private DisplayVpnServer _selectedVpnServer;

    private BindingList<DisplayVpnServer> _vpnServers;

    public MainViewModel()
    {
        Globals.MainViewModel = this;
    }

    public MainViewModel(Window window) : base(window)
    {
        Globals.MainViewModel = this;
    }

    public ConnectionState ConnectionState
    {
        get
        {
            if (Globals.Container.GetInstance<IVpnManager>().IsConnected)
                _connectionState = ConnectionState.Connected;
            else if (_connectionState != ConnectionState.Connecting)
                _connectionState = ConnectionState.Disconnected;

            return _connectionState;
        }
        set
        {
            _connectionState = value;
            OnPropertyChanged();
        }
    }

    public string LastServer
    {
        get => _lastServer;
        set
        {
            _lastServer = value;
            OnPropertyChanged();
        }
    }

    public DisplayVpnServer SelectedVpnServer
    {
        get => _selectedVpnServer;
        set
        {
            _selectedVpnServer = value;
            OnPropertyChanged();
        }
    }

    public BindingList<DisplayVpnServer> VpnServers
    {
        get => _vpnServers;
        set
        {
            _vpnServers = value;
            OnPropertyChanged();
        }
    }

    public ICommand ToggleConnectCommand
    {
        get
        {
            return new UICommand
            {
                CommandAction = async _ =>
                {
                    try
                    {
                        if (ConnectionState is ConnectionState.Connecting or ConnectionState.Disconnecting) return;

                        var vpnService = Globals.Container.GetInstance<IOpenVpnService>();
                        var vpnManagerService = Globals.Container.GetInstance<IVpnManager>();

                        if (ConnectionState == ConnectionState.Connected)
                        {
                            ConnectionState = ConnectionState.Disconnecting;

                            await vpnManagerService.DisconnectAsync();

                            if (Globals.TrayViewModel is TrayViewModel trayViewModel)
                                trayViewModel.ConnectionState = ConnectionState.Disconnected;

                            ConnectionState = ConnectionState.Disconnected;

                            return;
                        }

                        var config = Globals.Container.GetInstance<IConfigurationManager<AppConfiguration>>()
                            .Read();

                        if (string.IsNullOrWhiteSpace(config.LastServer?.Location) ||
                            string.IsNullOrWhiteSpace(config.LastServer.PritunlName)) return;

                        ConnectionState = ConnectionState.Connecting;

                        if (Globals.TrayViewModel is TrayViewModel trayViewModel1)
                            trayViewModel1.ConnectionState = ConnectionState.Connecting;

                        if (Globals.Container.GetInstance<IConfigurationManager<AppConfiguration>>().Read()
                            .IsDiscordRpcEnabled)
                            Globals.Container.GetInstance<IDiscordRp>()
                                .UpdateState($"Connecting to {config.LastServer?.Location}...");

                        await vpnService.ConnectAsync(config.LastServer.PritunlName, config.LastServer.Location);
                    }
                    catch (InvalidOperationException e)
                    {
                        ConnectionState = ConnectionState.Disconnected;

                        await DialogManager.ShowDialogAsync(
                            PackIconKind.ErrorOutline, "Something went wrong...",
                            e.Message);
                    }
                }
            };
        }
    }

    public ICommand GridConnectCommand
    {
        get
        {
            return new UICommand
            {
                CommandAction = async args =>
                {
                    try
                    {
                        if (ConnectionState is ConnectionState.Connecting or ConnectionState.Disconnecting or
                            ConnectionState.Connected) return;

                        var vpnService = Globals.Container.GetInstance<IOpenVpnService>();

                        if (args is not DisplayVpnServer server) return;

                        LastServer = server.ServerName;

                        ConnectionState = ConnectionState.Connecting;

                        if (Globals.TrayViewModel is TrayViewModel trayViewModel)
                            trayViewModel.ConnectionState = ConnectionState.Connecting;

                        if (Globals.Container.GetInstance<IConfigurationManager<AppConfiguration>>().Read()
                            .IsDiscordRpcEnabled)
                            Globals.Container.GetInstance<IDiscordRp>()
                                .UpdateState($"Connecting to {server.ServerName}...");

                        await vpnService.ConnectAsync(server.Id, server.ServerName);
                    }
                    catch (InvalidOperationException e)
                    {
                        ConnectionState = ConnectionState.Disconnected;

                        await DialogManager.ShowDialogAsync(
                            PackIconKind.ErrorOutline, "Something went wrong...",
                            e.Message);
                    }
                }
            };
        }
    }

    public ICommand LoadSettingsCommand
    {
        get
        {
            return new UICommand
            {
                CommandAction = _ =>
                {
                    IsPlayingAnimation = true;
                    var mainWindow = (MainWindow)Application.Current.MainWindow;
                    mainWindow?.LoadView(new SettingsView());
                },
                CanExecuteFunc = () => !IsPlayingAnimation
            };
        }
    }

    public ICommand LoadCommand
    {
        get
        {
            return new UICommand
            {
                CommandAction = async _ =>
                {
                    try
                    {
                        IsPlayingAnimation = false;

                        VpnServers ??= new BindingList<DisplayVpnServer>();
                        SelectedVpnServer = null;

                        var config = Globals.Container.GetInstance<IConfigurationManager<AppConfiguration>>()
                            .Read();

                        LastServer = config.LastServer is null ? "N/A" : config.LastServer.Location;

                        var vpnService = Globals.Container.GetInstance<IVpnManager>();
                        vpnService.OnConnected += OnConnected;
                        vpnService.OnErrorReceived += OnErrorReceived;

                        var cacheService = Globals.Container.GetInstance<ICacheService>();

                        var cachedVpnServers = await cacheService.GetCachedApiServerResponseAsync();
                        if (cachedVpnServers is not null)
                        {
                            VpnServers = cachedVpnServers;
                            return;
                        }

                        var apiClient = Globals.Container.GetInstance<IApiClient>();
                        var servers = await apiClient.GetServersAsync();

                        VpnServers.Clear();

                        foreach (var item in servers.OrderByDescending(x => x.Country).ThenBy(x => x.ServerName))
                            VpnServers.Add(new DisplayVpnServer
                            {
                                ServerName = item.ServerName,
                                Id = item.Id,
                                Country = item.Country,
                                Status = "Check",
                                Flag =
                                    $"pack://application:,,,/Resources/Flags/{item.Country.Replace(' ', '-')}.png"
                            });

                        await cacheService.CacheApiServerResponseAsync(VpnServers);
                    }
                    catch (Exception e)
                    {
                        await DialogManager.ShowDialogAsync(PackIconKind.ErrorOutline, "Something went wrong...",
                            e.Message);
                    }
                }
            };
        }
    }

    private async void OnErrorReceived(object sender, ErrorEventArgs e)
    {
        if (Globals.Container.GetInstance<IConfigurationManager<AppConfiguration>>().Read()
            .IsDiscordRpcEnabled)
            Globals.Container.GetInstance<IDiscordRp>()
                .UpdateState("Disconnected");

        ConnectionState = ConnectionState.Disconnected;

        if (Globals.TrayViewModel is TrayViewModel trayViewModel)
            trayViewModel.ConnectionState = ConnectionState.Disconnected;

        // Since the DialogHost requires STAThread, we must invoke this method using the dispatcher.
        await Application.Current.Dispatcher.InvokeAsync(async () => await DialogManager.ShowDialogAsync(
            PackIconKind.ErrorOutline, "Something went wrong...",
            e.Exception.Message));
    }

    private void OnConnected(object sender, ConnectedEventArgs e)
    {
        if (Globals.Container.GetInstance<IConfigurationManager<AppConfiguration>>().Read()
            .IsDiscordRpcEnabled)
            Globals.Container.GetInstance<IDiscordRp>()
                .UpdateState("Connected!");

        if (Globals.TrayViewModel is TrayViewModel trayViewModel)
            trayViewModel.ConnectionState = ConnectionState.Connected;

        ConnectionState = ConnectionState.Connected;
    }
}
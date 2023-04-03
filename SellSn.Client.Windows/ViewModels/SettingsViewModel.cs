using System;
using System.Diagnostics;
using System.Reflection;
using System.Windows;
using System.Windows.Input;
using LightVPN.Client.Debug;
using MaterialDesignThemes.Wpf;
using SellSn.Client.Discord.Interfaces;
using SellSn.Client.OpenVPN.Interfaces;
using SellSn.Client.OpenVPN.Utils;
using SellSn.Client.Windows.Common;
using SellSn.Client.Windows.Common.Utils;
using SellSn.Client.Windows.Configuration.Interfaces;
using SellSn.Client.Windows.Configuration.Models;
using SellSn.Client.Windows.Models;
using SellSn.Client.Windows.Services.Interfaces;
using SellSn.Client.Windows.Utils;
using SellSn.Client.Windows.Views;

namespace SellSn.Client.Windows.ViewModels;

internal sealed class SettingsViewModel : BaseViewModel
{
    private AppConfiguration _appConfiguration;

    private bool _isAppLogProcessOpen;

    private bool _isLogProcessOpen;

    private bool _isRefreshingCache;
    
    private bool _isReinstallingDriver;

    private bool _isReinstallingAdapter;

    private bool _isRunningOnStartup = StartupHelper.IsRunningOnStartup();

    public SettingsViewModel()
    {
        Globals.Container.GetInstance<IVpnManager>().TapManager.OnErrorReceived += async (_, args) =>
            await Application.Current.Dispatcher.InvokeAsync(async () => await DialogManager.ShowDialogAsync(
                PackIconKind.ErrorOutline, "Something went wrong...",
                args.Exception.Message));

        Globals.Container.GetInstance<IVpnManager>().TapManager.OnSuccess += async _ =>
            await Application.Current.Dispatcher.InvokeAsync(async () => await DialogManager.ShowDialogAsync(
                PackIconKind.CheckCircleOutline, "Success!",
                "The VPN adapter has been reinstalled."));
    }

    public AppConfiguration AppConfiguration
    {
        get => _appConfiguration;
        set
        {
            _appConfiguration = value;
            OnPropertyChanged();
        }
    }

    public bool IsLogProcessOpen
    {
        get => _isLogProcessOpen;
        set
        {
            _isLogProcessOpen = value;
            OnPropertyChanged();
        }
    }

    public bool IsAppLogProcessOpen
    {
        get => _isAppLogProcessOpen;
        set
        {
            _isAppLogProcessOpen = value;
            OnPropertyChanged();
        }
    }

    public bool IsReinstallingAdapter
    {
        get => _isReinstallingAdapter;
        set
        {
            _isReinstallingAdapter = value;
            OnPropertyChanged();
        }
    }
    
    public bool IsReinstallingDriver
    {
        get => _isReinstallingDriver;
        set
        {
            _isReinstallingDriver = value;
            OnPropertyChanged();
        }
    }

    public bool IsRunningOnStartup
    {
        get => _isRunningOnStartup;
        set
        {
            _isRunningOnStartup = value;
            OnPropertyChanged();
        }
    }

    public bool IsRefreshingCache
    {
        get => _isRefreshingCache;
        set
        {
            _isRefreshingCache = value;
            OnPropertyChanged();
        }
    }

    public string VersionString { get; set; } =
        $"{(Globals.IsBeta ? "beta" : "stable")} svpn-{Assembly.GetEntryAssembly()?.GetName().Version} {HostVersion.GetOsVersion()}";

    public ICommand HandleConfigChanges
    {
        get
        {
            return new UICommand
            {
                CommandAction = _ =>
                {
                    var manager = Globals.Container.GetInstance<IConfigurationManager<AppConfiguration>>();
                    manager.Write(AppConfiguration);

                    var discordClient = Globals.Container.GetInstance<IDiscordRp>();
                    if (AppConfiguration.IsDiscordRpcEnabled)
                        discordClient.Initialize();
                    else
                        discordClient.Deinitialise();

                    if (IsRunningOnStartup)
                        StartupHelper.EnableRunOnStartup(Process.GetCurrentProcess().MainModule?.FileName);
                    else
                        StartupHelper.DisableRunOnStartup();

                    ThemeManager.SwitchTheme(ThemeColor.Default,
                        AppConfiguration.IsDarkModeEnabled ? BackgroundMode.Dark : BackgroundMode.Light);
                }
            };
        }
    }

    public ICommand LoadCommand
    {
        get
        {
            return new UICommand
            {
                CommandAction = _ =>
                {
                    var manager = Globals.Container.GetInstance<IConfigurationManager<AppConfiguration>>();
                    AppConfiguration = manager.Read();

                    if (AppConfiguration is not null) return;

                    IsRunningOnStartup = StartupHelper.IsRunningOnStartup();

                    AppConfiguration = new AppConfiguration();
                    manager.Write(AppConfiguration);
                }
            };
        }
    }

    public ICommand RefreshCacheCommand
    {
        get
        {
            return new UICommand
            {
                CommandAction = async _ =>
                {
                    try
                    {
                        IsRefreshingCache = true;
                    
                        await Globals.Container.GetInstance<ICacheService>().CacheServersAsync(true);
                    
                        await DialogManager.ShowDialogAsync(PackIconKind.CheckCircleOutline, "Success!",
                            "Cache has been cleared.");
                    }
                    catch (Exception e)
                    {
                        await DialogManager.ShowDialogAsync(PackIconKind.ErrorOutline, "Something went wrong...",
                            e.Message);
                    }
                    finally
                    {
                        IsRefreshingCache = false;
                    }
                }
            };
        }
    }

    public ICommand BackCommand
    {
        get
        {
            return new UICommand
            {
                CommandAction = _ =>
                {
                    IsPlayingAnimation = true;
                    var mainWindow = (MainWindow)Application.Current.MainWindow;
                    mainWindow?.LoadView(new MainView());
                },
                CanExecuteFunc = () => !IsPlayingAnimation
            };
        }
    }

    public ICommand VpnLogsCommand
    {
        get
        {
            return new UICommand
            {
                CommandAction = async _ =>
                {
                    var notepadProcess = new Process
                    {
                        StartInfo = new ProcessStartInfo
                        {
                            FileName = "notepad.exe",
                            UseShellExecute = true,
                            Arguments = Globals.OpenVpnLogPath
                        }
                    };

                    notepadProcess.Start();
                    IsLogProcessOpen = true;

                    await notepadProcess.WaitForExitAsync();

                    IsLogProcessOpen = false;
                    notepadProcess.Dispose();
                },
                CanExecuteFunc = () => !IsLogProcessOpen
            };
        }
    }

    public ICommand AppLogsCommand
    {
        get
        {
            return new UICommand
            {
                CommandAction = async _ =>
                {
                    var notepadProcess = new Process
                    {
                        StartInfo = new ProcessStartInfo
                        {
                            FileName = "notepad.exe",
                            UseShellExecute = true,
                            Arguments = DebugLogger.DebugLogLocation
                        }
                    };

                    notepadProcess.Start();
                    IsAppLogProcessOpen = true;

                    await notepadProcess.WaitForExitAsync();

                    IsAppLogProcessOpen = false;
                    notepadProcess.Dispose();
                },
                CanExecuteFunc = () => !IsAppLogProcessOpen
            };
        }
    }

    public ICommand ReinstallTapAdapterCommand
    {
        get
        {
            return new UICommand
            {
                CommandAction = async _ =>
                {
                    try
                    {
                        IsReinstallingAdapter = true;

                        var vpnManager = Globals.Container.GetInstance<IVpnManager>();
                        if (await vpnManager.TapManager.IsAdapterExistentAsync())
                            await vpnManager.TapManager.RemoveTapAdapterAsync();

                        await vpnManager.TapManager.InstallTapAdapterAsync();
                    }
                    catch (InvalidOperationException e)
                    {
                        await DialogManager.ShowDialogAsync(PackIconKind.ErrorOutline, "Something went wrong...",
                            e.Message);
                    }
                    finally
                    {
                        IsReinstallingAdapter = false;
                    }
                }
            };
        }
    }
    
    public ICommand ReinstallTapDriverCommand
    {
        get
        {
            return new UICommand
            {
                CommandAction = async _ =>
                {
                    try
                    {
                        IsReinstallingDriver = true;

                        var vpnManager = Globals.Container.GetInstance<IVpnManager>();

                        await Globals.Container.GetInstance<ICacheService>().CacheOpenVpnDriversAsync();

                        if (TapManager.IsDriverInstalled())
                            await vpnManager.TapManager.RemoveTapDriverAsync();
                        
                        await vpnManager.TapManager.InstallTapDriverAsync();
                    }
                    catch (InvalidOperationException e)
                    {
                        await DialogManager.ShowDialogAsync(PackIconKind.ErrorOutline, "Something went wrong...",
                            e.Message);
                    }
                    finally
                    {
                        IsReinstallingDriver = false;
                    }
                }
            };
        }
    }
}
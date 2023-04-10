using System.Windows.Controls;
using System.Windows.Input;
using SellSn.Client.Auth.Interfaces;
using SellSn.Client.OpenVPN.Interfaces;
using SellSn.Client.OpenVPN.Utils;
using SellSn.Client.Windows.Common;
using SellSn.Client.Windows.Services.Interfaces;
using SellSn.Client.Windows.Utils;
using SellSn.Client.Windows.Windows;

namespace SellSn.Client.Windows.ViewModels;

internal sealed class LoginViewModel : WindowViewModel
{
    private bool _isAuthenticating;

    private bool _isIndeterminate = true;

    private int _progressInt = -1;

    private string _statusText = "SIGN IN";


    public LoginViewModel() : base(null, false)
    {
    }

    public bool IsAuthenticating
    {
        get => _isAuthenticating;
        set
        {
            _isAuthenticating = value;
            OnPropertyChanged();
        }
    }

    public int ProgressInt
    {
        get => _progressInt;

        set
        {
            _progressInt = value;
            OnPropertyChanged();
        }
    }

    public string StatusText
    {
        get => _statusText;

        set
        {
            _statusText = value.ToUpper();
            OnPropertyChanged();
        }
    }

    public bool IsIndeterminate
    {
        get => _isIndeterminate;

        set
        {
            _isIndeterminate = value;
            OnPropertyChanged();
        }
    }

    [NotNull]
    public ICommand LoginCommand
    {
        get
        {
            return new UICommand
            {
                CommandAction = async _ =>
                {
                    try
                    {
                        IsAuthenticating = true;
                        
                        StatusText = "Awaiting completion";
                        var window = new OAuthWindow(Globals.Container.GetInstance<IApiClient>());
                        window.ShowDialog();

                        StatusText = "Signing in";
                        var user = await Globals.Container.GetInstance<IApiClient>().GetProfileAsync();
                        Globals.Profile = user;
                        
                        StatusText = "Checking for updates";
                        await Globals.Container.GetInstance<IVersionClient>().CheckAndInstallUpdatesAsync();

                        // Cache OVPN binaries
                        StatusText = "Downloading files";
                        await Globals.Container.GetInstance<ICacheService>().CacheOpenVpnBinariesAsync();

                        // Cache OVPN configs
                        await Globals.Container.GetInstance<ICacheService>().CacheServersAsync();
                        
                        // Install drivers if not installed
                        if (!TapManager.IsDriverInstalled())
                        {
                            StatusText = "Installing drivers";
                            await Globals.Container.GetInstance<IVpnManager>().TapManager.InstallTapDriverAsync();
                        }

                        var loginWindow = (LoginWindow)Globals.LoginWindow;

                        var mainWindow = new MainWindow();
                        mainWindow.Show();

                        loginWindow.Close();
                    }
                    // catch (UpdateRequiredException e)
                    // {
                    //     await DialogManager.ShowDialogAsync(PackIconKind.Update, "An update is required!",
                    //         e.Message);
                    // }
                    finally
                    {
                        IsAuthenticating = false;
                    }
                },
                CanExecuteFunc = () => !IsAuthenticating
            };
        }
    }
}
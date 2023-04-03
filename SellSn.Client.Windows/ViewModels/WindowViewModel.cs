using System.Windows;
using System.Windows.Input;
using LightVPN.Client.Debug;
using SellSn.Client.Windows.Utils;

namespace SellSn.Client.Windows.ViewModels;

internal class WindowViewModel : BaseViewModel
{
    private readonly bool _canMaximize;

    private WindowState _windowState;
    private readonly Window? _window;

    public WindowViewModel(Window? window = null, bool canMaximize = true)
    {
        _window = window ?? Application.Current.MainWindow;
        _canMaximize = canMaximize;
        if (Application.Current.MainWindow != null)
            Application.Current.MainWindow.StateChanged +=
                (_, _) => WindowState = Application.Current.MainWindow.WindowState;
    }

    public WindowViewModel()
    {
        _canMaximize = true;
        if (Application.Current.MainWindow != null)
            Application.Current.MainWindow.StateChanged +=
                (_, _) => WindowState = Application.Current.MainWindow.WindowState;
    }

    public WindowState WindowState
    {
        get => _windowState;
        set
        {
            _windowState = value;
            OnPropertyChanged();
        }
    }

    public ICommand MinimizeCommand
    {
        get
        {
            return new UICommand
            {
                CommandAction = _ =>
                {
                    if (_window == null) return;

                    WindowState = _window.WindowState;
                    _window.WindowState = WindowState.Minimized;
                }
            };
        }
    }

    public ICommand ToggleMaxCommand
    {
        get
        {
            return new UICommand
            {
                CommandAction = _ =>
                {
                    if (_window == null) return;

                    DebugLogger.Write("lvpn-client-win-chrome-mvvm",
                        "running this monstrosity of a conditional to determine how to toggle max");

                    WindowState = _window.WindowState;
                    _window.WindowState =
                        _window is { WindowState: WindowState.Maximized }
                            ? WindowState.Normal
                            : WindowState.Maximized;
                },
                CanExecuteFunc = () => _canMaximize
            };
        }
    }

    public ICommand CloseCommand
    {
        get
        {
            return new UICommand
            {
                CommandAction = _ => _window?.Close()
            };
        }
    }
}
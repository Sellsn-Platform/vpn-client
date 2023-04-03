using System.Windows;
using SellSn.Client.Windows.Common;

namespace SellSn.Client.Windows;

/// <inheritdoc cref="System.Windows.Window" />
internal sealed partial class LoginWindow : Window
{
    public LoginWindow()
    {
        InitializeComponent();

        Globals.LoginWindow = this;

        Application.Current.MainWindow = this;
    }
}
using System.Windows;
using System.Windows.Input;
using SellSn.Client.Windows.Common;
using SellSn.Client.Windows.Configuration.Interfaces;
using SellSn.Client.Windows.Configuration.Models;
using SellSn.Client.Windows.Utils;
using SellSn.Client.Windows.Views;

namespace SellSn.Client.Windows.ViewModels;

internal sealed class FirstRunViewModel : BaseViewModel
{
    public ICommand ForwardCommand
    {
        get
        {
            return new UICommand
            {
                CommandAction = _ =>
                {
                    var manager = Globals.Container.GetInstance<IConfigurationManager<AppConfiguration>>();

                    var config = manager.Read();

                    config.IsFirstRun = false;

                    manager.Write(config);

                    var mainWindow = (MainWindow)Application.Current.MainWindow;
                    mainWindow?.LoadView(new MainView());
                }
            };
        }
    }
}
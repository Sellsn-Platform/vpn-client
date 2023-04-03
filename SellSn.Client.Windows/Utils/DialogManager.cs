using System.Threading.Tasks;
using MaterialDesignThemes.Wpf;
using SellSn.Client.Windows.Dialogs;
using SellSn.Client.Windows.ViewModels;

namespace SellSn.Client.Windows.Utils;

internal static class DialogManager
{
    internal static async Task ShowDialogAsync(PackIconKind iconKind, string title, string message)
    {
        var view = new Dialog
        {
            DataContext = new DialogViewModel(iconKind, title, message)
        };

        await DialogHost.Show(view, "RootDialog");
    }
}
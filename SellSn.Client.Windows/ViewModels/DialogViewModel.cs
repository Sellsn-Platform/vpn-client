using MaterialDesignThemes.Wpf;

namespace SellSn.Client.Windows.ViewModels;

internal sealed class DialogViewModel : BaseViewModel
{
    public DialogViewModel(PackIconKind icon, string title, string message)
    {
        Message = message;
        Title = title;
        IconKind = icon;
    }

    public string Message { get; }

    public string Title { get; }

    public PackIconKind IconKind { get; }
}
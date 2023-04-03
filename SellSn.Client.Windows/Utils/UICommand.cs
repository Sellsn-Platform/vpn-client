using System;
using System.Windows.Input;

namespace SellSn.Client.Windows.Utils;

/// <inheritdoc />
internal sealed class UICommand : ICommand
{
    internal Action<object> CommandAction { get; init; }
    internal Func<bool> CanExecuteFunc { get; init; }

    public void Execute(object args)
    {
        CommandAction(args);
    }

    public bool CanExecute(object args)
    {
        return CanExecuteFunc?.Invoke() != false;
    }

    public event EventHandler CanExecuteChanged
    {
        add => CommandManager.RequerySuggested += value;
        remove => CommandManager.RequerySuggested -= value;
    }
}
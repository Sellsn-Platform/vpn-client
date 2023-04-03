using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;

namespace SellSn.Client.Windows.ViewModels;

/// <inheritdoc cref="System.ComponentModel.INotifyPropertyChanged" />
internal abstract class BaseViewModel : INotifyPropertyChanged, IDisposable
{
    protected readonly CancellationTokenSource CancellationTokenSource = new();

    private bool _isPlayingAnimation;

    public bool IsPlayingAnimation
    {
        get => _isPlayingAnimation;
        set
        {
            _isPlayingAnimation = value;
            OnPropertyChanged();
        }
    }

    public void Dispose()
    {
        IsPlayingAnimation = false;
        CancellationTokenSource.Cancel();
    }

    public event PropertyChangedEventHandler PropertyChanged;

    [NotifyPropertyChangedInvocator]
    protected void OnPropertyChanged([CallerMemberName] [CanBeNull] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
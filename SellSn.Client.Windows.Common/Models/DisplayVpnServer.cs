using System.ComponentModel;

namespace SellSn.Client.Windows.Common.Models;

public sealed class DisplayVpnServer : INotifyPropertyChanged
{
    public string Country
    {
        get => _country;

        set
        {
            _country = value;
            OnPropertyChanged(nameof(Country));
        }
    }

    public string Flag
    {
        get => _flag;

        set
        {
            _flag = value;
            OnPropertyChanged(nameof(Flag));
        }
    }

    public string Id
    {
        get => _id;

        init
        {
            _id = value;
            OnPropertyChanged(nameof(Id));
        }
    }

    public string ServerName
    {
        get => _displayName;

        init
        {
            _displayName = value;
            OnPropertyChanged(nameof(ServerName));
        }
    }

    public string Status
    {
        get => _status;

        set
        {
            _status = value;
            OnPropertyChanged(nameof(Status));
        }
    }

    private string _country { set; get; }

    private string _displayName { set; get; }

    private string _flag { set; get; }

    private string _id { set; get; }

    private string _status { set; get; }

    public event PropertyChangedEventHandler PropertyChanged;

    private void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
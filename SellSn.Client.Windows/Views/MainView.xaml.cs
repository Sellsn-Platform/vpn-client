using System.Windows.Controls;

namespace SellSn.Client.Windows.Views;

/// <inheritdoc cref="System.Windows.Controls.Page" />
/// <summary>
///     Interaction logic for MainView.xaml
/// </summary>
internal sealed partial class MainView : Page
{
    public MainView()
    {
        InitializeComponent();
    }

    private void DataGrid_BeginningEdit(object sender, DataGridBeginningEditEventArgs e)
    {
        e.Cancel = true;
    }
}
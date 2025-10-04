using System.Windows;

namespace RD.Views;

public partial class DownloadDetailsWindow : Window
{
    public DownloadDetailsWindow()
    {
        InitializeComponent();
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }

    protected override void OnClosed(EventArgs e)
    {
        base.OnClosed(e);
        
        // Cleanup the ViewModel when window is closed
        if (DataContext is ViewModels.DownloadDetailsViewModel viewModel)
        {
            viewModel.Cleanup();
        }
    }
}
using System.Windows;

namespace RD.Views;

public partial class AddDownloadWindow : Window
{
    public AddDownloadWindow()
    {
        InitializeComponent();
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}
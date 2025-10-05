using System.Windows;

namespace RD.Views;

public partial class AddDownloadWindow : Window
{
    public static AddDownloadWindow? Current;
    public AddDownloadWindow()
    {
        InitializeComponent();
        Current = this;
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}
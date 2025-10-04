using System.Windows;
using RD.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace RD;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = App.ServiceProvider.GetRequiredService<MainViewModel>();
    }

    protected override void OnClosed(EventArgs e)
    {
        base.OnClosed(e);
        
        // Cleanup the ViewModel when window is closed
        if (DataContext is MainViewModel viewModel)
        {
            viewModel.Cleanup();
        }
    }
}
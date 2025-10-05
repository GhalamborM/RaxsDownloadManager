using Microsoft.Extensions.DependencyInjection;
using RD.Controls;
using RD.ViewModels;
using System.Windows;

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
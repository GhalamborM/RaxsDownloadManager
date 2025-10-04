using Microsoft.Extensions.DependencyInjection;
using RD.Controls;
using RD.Examples;
using RD.ViewModels;
using System.Windows;
using static RD.Examples.CustomMessageBoxExample;

namespace RD;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = App.ServiceProvider.GetRequiredService<MainViewModel>();
        Loaded += MainWindow_Loaded;
    }

    private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        return;
        await Task.Delay(5000);
        CustomMessageBox.Show("This is a simple information message.");
        CustomMessageBoxExample.ShowExamples();

        CustomMessageBoxExample.ShowWithOwner(this);
        
        DownloadManagerExamples.ConfirmDeleteDownload();


        
        var result = DownloadManagerExamples.HandleDownloadError();


        

        DownloadManagerExamples.ShowDownloadComplete("DDDDDDDDDDDDD");

        

        DownloadManagerExamples.ConfirmOverwrite("DDDDDDDDDDDDD");
        

        DownloadManagerExamples.ShowQuotaExceeded();

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
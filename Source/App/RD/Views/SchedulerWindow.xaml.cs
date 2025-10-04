using System.Windows;
using RD.ViewModels;

namespace RD.Views;

public partial class SchedulerWindow : Window
{
    public SchedulerWindow()
    {
        InitializeComponent();
    }

    protected override void OnClosed(EventArgs e)
    {
        // Cleanup the view model when the window is closed
        if (DataContext is SchedulerViewModel viewModel)
        {
            viewModel.Cleanup();
        }
        base.OnClosed(e);
    }
}
using Microsoft.Extensions.DependencyInjection;
using RD.Controls;
using RD.Models;
using RD.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace RD;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = App.ServiceProvider.GetRequiredService<MainViewModel>();
    }

    private void TreeViewItem_Selected(object sender, RoutedEventArgs e)
    {
        if (sender is TreeViewItem item && item.DataContext is CategoryTreeNode node)
        {
            if (DataContext is MainViewModel viewModel)
            {
                viewModel.SelectedCategoryNode = node;
            }
            e.Handled = true;
        }
    }

    private void ToggleCategoryPanel_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is MainViewModel viewModel)
        {
            viewModel.IsCategoryPanelCollapsed = !viewModel.IsCategoryPanelCollapsed;
        }
    }

    protected override void OnClosed(EventArgs e)
    {
        base.OnClosed(e);
        
        if (DataContext is MainViewModel viewModel)
        {
            viewModel.Cleanup();
        }
    }
}
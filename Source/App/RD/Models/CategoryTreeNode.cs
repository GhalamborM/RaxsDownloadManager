using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

namespace RD.Models;

public partial class CategoryTreeNode : ObservableObject
{
    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    private string _categoryName = string.Empty;

    [ObservableProperty]
    private CategoryFilterType _filterType;

    [ObservableProperty]
    private bool _isExpanded = true;

    [ObservableProperty]
    private bool _isSelected;

    [ObservableProperty]
    private ObservableCollection<CategoryTreeNode> _children = [];

    public bool IsCategory => !string.IsNullOrEmpty(CategoryName);
}

public enum CategoryFilterType
{
    All,
    Unfinished,
    Finished,
    Category
}

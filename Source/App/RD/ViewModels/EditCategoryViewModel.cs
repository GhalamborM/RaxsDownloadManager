using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RD.Core.Models;
using System.Collections.ObjectModel;

namespace RD.ViewModels;

public partial class EditCategoryViewModel : ObservableObject
{
    [ObservableProperty]
    private FileCategory _category;

    [ObservableProperty]
    private ObservableCollection<string> _extensions = new();

    [ObservableProperty]
    private string? _selectedExtension;

    [ObservableProperty]
    private string _newExtension = string.Empty;

    [ObservableProperty]
    private string _categoryName = string.Empty;

    [ObservableProperty]
    private string _folderName = string.Empty;

    [ObservableProperty]
    private bool _isEnabled;

    [ObservableProperty]
    private bool _isDefaultCategory;

    public event Action? SaveRequested;
    public event Action? CancelRequested;

    public EditCategoryViewModel()
    {
        _category = new FileCategory();
    }

    public void LoadCategory(FileCategory category)
    {
        Category = category;
        CategoryName = category.Name;
        FolderName = category.FolderName;
        IsEnabled = category.IsEnabled;
        IsDefaultCategory = category.IsDefault;
        
        Extensions.Clear();
        foreach (var ext in category.Extensions)
        {
            Extensions.Add(ext);
        }
    }

    [RelayCommand]
    private void AddExtension()
    {
        if (string.IsNullOrWhiteSpace(NewExtension))
            return;

        var extension = NewExtension.Trim().TrimStart('.').ToLowerInvariant();
        
        if (string.IsNullOrWhiteSpace(extension))
            return;

        if (!Extensions.Contains(extension, StringComparer.OrdinalIgnoreCase))
        {
            Extensions.Add(extension);
            NewExtension = string.Empty;
        }
    }

    [RelayCommand]
    private void RemoveExtension()
    {
        if (SelectedExtension != null)
        {
            Extensions.Remove(SelectedExtension);
        }
    }

    [RelayCommand]
    private void Save()
    {
        Category.Name = CategoryName;
        Category.FolderName = FolderName;
        Category.IsEnabled = IsEnabled;
        Category.Extensions.Clear();
        Category.Extensions.AddRange(Extensions);

        SaveRequested?.Invoke();
    }

    [RelayCommand]
    private void Cancel()
    {
        CancelRequested?.Invoke();
    }
}

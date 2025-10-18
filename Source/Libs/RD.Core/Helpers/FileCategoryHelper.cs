using RD.Core.Models;

namespace RD.Core.Helpers;

public static class FileCategoryHelper
{
    public static FileCategory? GetCategoryForFile(string fileName, List<FileCategory> categories)
    {
        if (string.IsNullOrWhiteSpace(fileName) || categories == null || categories.Count == 0)
            return null;

        var extension = Path.GetExtension(fileName)?.TrimStart('.').ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(extension))
            return null;

        foreach (var category in categories.Where(c => c.IsEnabled && c.Name != FileCategoryDefaults.General))
        {
            if (category.Extensions.Any(ext => ext.Equals(extension, StringComparison.OrdinalIgnoreCase)))
            {
                return category;
            }
        }

        return categories.FirstOrDefault(c => c.Name == FileCategoryDefaults.General);
    }

    public static string GetCategorizedDownloadPath(string baseDownloadDirectory, string fileName, 
        List<FileCategory> categories, bool useCategorization)
    {
        if (!useCategorization || string.IsNullOrWhiteSpace(baseDownloadDirectory) || string.IsNullOrWhiteSpace(fileName))
        {
            return Path.Combine(baseDownloadDirectory, fileName);
        }

        var category = GetCategoryForFile(fileName, categories);
        
        if (category == null || string.IsNullOrWhiteSpace(category.FolderName))
        {
            return Path.Combine(baseDownloadDirectory, fileName);
        }

        return Path.Combine(baseDownloadDirectory, category.FolderName, fileName);
    }

    public static void EnsureCategoryFolderExists(string downloadPath)
    {
        var directory = Path.GetDirectoryName(downloadPath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }
    }

    public static void ValidateCategories(List<FileCategory> categories)
    {
        if (categories == null) return;

        foreach (var category in categories)
        {
            if (category.Name == FileCategoryDefaults.General)
            {
                category.FolderName = string.Empty;
            }
            else if (!string.IsNullOrWhiteSpace(category.FolderName))
            {
                var invalidChars = Path.GetInvalidFileNameChars();
                category.FolderName = string.Join("", category.FolderName.Split(invalidChars));
            }
        }
    }
}

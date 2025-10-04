using RD.Core.Models;
using System.Text.Json;
using System.IO;

namespace RD.Services;
public class DataPersistenceService : IDataPersistenceService
{
    private readonly string _dataDirectory;
    private readonly string _downloadsFile;
    private readonly string _optionsFile;
    private readonly JsonSerializerOptions _jsonWriteOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };
    private readonly JsonSerializerOptions _jsonReadOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };
    public DataPersistenceService()
    {
        _dataDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "RD", "WPF");
        _downloadsFile = Path.Combine(_dataDirectory, "downloads.json");
        _optionsFile = Path.Combine(_dataDirectory, "options.json");

        Directory.CreateDirectory(_dataDirectory);
    }

    public async Task SaveDownloadsAsync(IEnumerable<DownloadItem> downloads)
    {
        try
        {
            var json = JsonSerializer.Serialize(downloads, _jsonWriteOptions);
            await File.WriteAllTextAsync(_downloadsFile, json);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to save downloads: {ex.Message}");
        }
    }

    public async Task<List<DownloadItem>> LoadDownloadsAsync()
    {
        try
        {
            if (!File.Exists(_downloadsFile))
                return [];

            var json = await File.ReadAllTextAsync(_downloadsFile);
            return JsonSerializer.Deserialize<List<DownloadItem>>(json, _jsonReadOptions) ?? [];
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to load downloads: {ex.Message}");
            return [];
        }
    }

    public async Task SaveOptionsAsync(DownloadManagerOptions options)
    {
        try
        {
            var json = JsonSerializer.Serialize(options, _jsonWriteOptions);
            await File.WriteAllTextAsync(_optionsFile, json);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to save options: {ex.Message}");
        }
    }

    public async Task<DownloadManagerOptions> LoadOptionsAsync()
    {
        try
        {
            if (!File.Exists(_optionsFile))
                return new DownloadManagerOptions();

            var json = await File.ReadAllTextAsync(_optionsFile);
            return JsonSerializer.Deserialize<DownloadManagerOptions>(json, _jsonReadOptions) ?? new DownloadManagerOptions();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to load options: {ex.Message}");
            return new DownloadManagerOptions();
        }
    }
}
using RD.Core.Models;

namespace RD.Services;

public interface IDataPersistenceService
{
    Task SaveDownloadsAsync(IEnumerable<DownloadItem> downloads);
    Task<List<DownloadItem>> LoadDownloadsAsync();
    Task SaveOptionsAsync(DownloadManagerOptions options);
    Task<DownloadManagerOptions> LoadOptionsAsync();
    Task SaveUISettingsAsync(UISettings settings);
    Task<UISettings> LoadUISettingsAsync();
}
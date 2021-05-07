using System;
using System.IO;
using System.Threading.Tasks;

namespace HttpDownloadManager.Data
{
    public class DefaultCachedFolder : ICachedFolder
    {
        public string CachedFolderPath => Path.Combine(Path.GetTempPath(), Constants.AppName);

        public Task<bool> CheckCachedFolderExistsAsync() => Task.FromResult(Directory.Exists(CachedFolderPath));
        public Task<bool> CheckFolderExistsAsync(string folderName) => Task.FromResult(Directory.Exists(Path.Combine(CachedFolderPath, folderName)));

        public async Task CreateIfCahcedFolderNotExistsAsync()
        {
            if (!await CheckCachedFolderExistsAsync())
                await Task.FromResult(Directory.CreateDirectory(CachedFolderPath));
        }

        public async Task CreateIfFolderNotExistsAsync(string folderName)
        {
            if (!await CheckFolderExistsAsync(folderName))
                await Task.FromResult(Directory.CreateDirectory(Path.Combine(CachedFolderPath, folderName)));
        }

    }
}

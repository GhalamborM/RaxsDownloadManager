using System.IO;
using System.Threading.Tasks;

namespace HttpDownloadManager.Data
{
    public interface ICachedFolder
    {
        /// <summary>
        ///     Cached
        /// </summary>
        string CachedFolderPath { get; }
        /// <summary>
        ///     Checks folder exists
        /// </summary>
        Task<bool> CheckCachedFolderExistsAsync();
        Task<bool> CheckFolderExistsAsync(string folderName);
        /// <summary>
        ///     Create cached folder if not exists
        /// </summary>
        Task CreateIfCahcedFolderNotExistsAsync();
        Task CreateIfFolderNotExistsAsync(string folderName);
    }
}

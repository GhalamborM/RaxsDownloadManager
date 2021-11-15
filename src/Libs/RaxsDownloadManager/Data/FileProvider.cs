using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace RaxsDownloadManager.Data
{
    public class FileProvider : IFileProvider
    {
        private ICachedFolder _cachedFolder;
        public ICachedFolder CachedFolder
        {
            get
            {
                if (_cachedFolder == null)
                    _cachedFolder = new DefaultCachedFolder();

                return _cachedFolder;
            }
        }

        public async Task<FileStream> GetOrCreateFileAsync(string folderName, string fileName, FileCreationMode fileCreationMode)
        {
            try
            {
                await CachedFolder.CreateIfCahcedFolderNotExistsAsync();
                await CachedFolder.CreateIfFolderNotExistsAsync(folderName);
                var fileStream = new FileStream(Path.Combine(CachedFolder.CachedFolderPath, folderName, fileName),
                    fileCreationMode == FileCreationMode.Create ? FileMode.Create : FileMode.Append,
                    FileAccess.Write, FileShare.ReadWrite);

                return fileStream;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}

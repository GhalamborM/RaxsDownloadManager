using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace HttpDownloadManager.Data
{
    public interface IFileProvider
    {
        ICachedFolder CachedFolder { get; }

        Task<FileStream> GetOrCreateFileAsync(string folderName, string name, FileCreationMode fileCreationMode);
    }
}

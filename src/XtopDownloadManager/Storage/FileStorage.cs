using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using XtopDownloadManager.Helpers;
namespace XtopDownloadManager.Storage
{
    public class FileStorage : IFileStorage
    {
        public FileStorage(string url, string path, string refererUrl)
        {
            Name = url.GetFileName();
            Extension = System.IO.Path.GetExtension(url);
            RefererUrl = refererUrl;
            Path = path;
            Identifier = Guid.NewGuid();

        }

        public Guid Identifier { get; private set; }

        public string Name { get; set; }

        public string Path { get; private set; }

        public string Extension { get; private set; }

        public int Size { get; private set; }

        public byte[] Bytes { get; private set; }

        public byte[] DownloadedBytes { get; private set; }

        public string DownloadUrl { get; private set; }

        public string RefererUrl { get; }

        public DateTime CreatedAt { get; }

        public DateTime LastTryAt { get;  set; }
    }
}

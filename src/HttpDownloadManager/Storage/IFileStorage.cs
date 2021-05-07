using System;
using System.Collections.Generic;
using System.Text;

namespace HttpDownloadManager.Storage
{
    public interface IFileStorage
    {
        Guid Identifier { get; }

        string Name { get; set; }

        string Path { get; }

        string Extension { get; }

        int Size { get; }

        byte[] Bytes { get; }

        byte[] DownloadedBytes { get; }

        string DownloadUrl { get; }

        string RefererUrl { get; }

        DateTime CreatedAt { get; }

        DateTime LastTryAt { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Text;
using WDM.Downloaders.Storage;

namespace WDM.Downloaders
{
    public class Downloader
    {
        public List<DownloadSegment> Segments { get; } = new List<DownloadSegment>();
        public DownloadInfo DownloadInfo { get; internal set; }
        public FileStorage FileStorage { get; internal set; }

    }
}

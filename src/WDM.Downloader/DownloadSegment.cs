using System;
using System.Collections.Generic;
using System.Text;

namespace WDM.Downloaders
{
    public class DownloadSegment
    {
        public string Path { get; internal set; }
        public bool IsDownloading { get; internal set; }
        public long? StartPosition { get; internal set; }
        public long? EndPosition { get; internal set; }
        public DownloadSegmentState State { get; internal set; }
    }
}

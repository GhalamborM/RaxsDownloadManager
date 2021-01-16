using System;
using System.Collections.Generic;
using System.Text;
using System.Net;

namespace WDM.Downloader
{
    public struct DownloadInfo
    {
        public string ServerSideFileName { get; internal set; }
        public string ContentType { get; internal set; }
        public long? ContentLength { get; internal set; }
        public bool PartialContent { get; internal set; }
        public bool CanDetectPartialContent { get; internal set; }
        public HttpStatusCode StatusCode { get; internal set; }
    }
}

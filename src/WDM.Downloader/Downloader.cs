using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using WDM.Downloaders.Exceptions;
using WDM.Downloaders.Storage;

namespace WDM.Downloaders
{
    public class Downloader
    {
        public List<DownloadSegment> Segments { get; } = new List<DownloadSegment>();
        public DownloadInfo DownloadInfo { get; internal set; }
        public FileStorage FileStorage { get; internal set; }

        public async Task ConfigureAsync(Uri uri)
        {
            if(!uri.Scheme.ToLower().StartsWith("http"))
                throw new NotSupportedSchemeException($"'{uri.Scheme}' scheme is not supported");

        }
    }
}

using HttpDownloadManager.Downloads;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Security.Authentication;
using System.Text;

namespace HttpDownloadManager
{
    /// <summary>
    ///     Download configuration
    /// </summary>
    public class DownloadConfiguration
    {
        /// <summary>
        ///     Timeout
        /// </summary>
        public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(30);
        /// <summary>
        ///     Pre-download (automatically starts downloading)
        /// </summary>
        public bool PreDownload { get; set; } = false;
        /// <summary>
        ///     Max retry count when request fails
        /// </summary>
        public uint MaxRetryCountOnFails { get; set; } = uint.MaxValue;
        /// <summary>
        ///     Automatic deflate & gzip decompression
        /// </summary>
        public bool AutomaticDecompression { get; set; } = false;
        /// <summary>
        ///     Allow auto redirect
        /// </summary>
        public bool AllowAutoRedirect { get; set; } = false;
        /// <summary>
        ///     Maximum automatic redirections
        /// </summary>
        public int MaxAutomaticRedirections { get; set; } = 5;
        /// <summary>
        ///     Request message 
        /// </summary>
        public HttpRequestMessage RequestMessage { get; set; }
        /// <summary>
        ///     Cookie container
        /// </summary>
        public CookieContainer CookieContainer { get; set; }
        /// <summary>
        ///     Proxy
        /// </summary>
        public IWebProxy Proxy { get; set; }
        /// <summary>
        ///     SSL protocol, if available
        /// </summary>
        public SslProtocols SslProtocols { get; set; } = default;

    }
}

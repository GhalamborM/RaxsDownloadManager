using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HttpDownloadManager.Http
{
    /// <summary>
    ///     An tiny abstraction of HttpClient
    /// </summary>
    public interface IXttpClient : IDisposable
    {
        IReadOnlyDictionary<string, string[]> Headers { get; }

        string UserAgent { get; set; }

        string Accept { get; set; }

        Uri BaseAddress { get; set; }

        long MaxResponseContentBufferSize { get; set; }

        TimeSpan Timeout { get; set; }

        bool AutomaticDecompression { get; set; }

        bool AllowAutoRedirect { get; set; }

        int MaxAutomaticRedirections { get; set; } 

        void CancelPendingRequests();

        Task<HttpResponseMessage> SendAsync(HttpRequestMessage request);

        Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, HttpCompletionOption completionOption);

        Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, HttpCompletionOption completionOption, CancellationToken cancellationToken);
        
        Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken);
    }
}

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using XtopDownloadManager.Helpers;
namespace XtopDownloadManager.Http
{
    public sealed class XttpClient : IXttpClient
    {
        private const int MAX_CONN_PER_SERVER = 20;
        public IReadOnlyDictionary<string, string[]> Headers => throw new NotImplementedException();
        public string UserAgent { get; set; } = UserAgentHelper.CHROME_USER_AGENT; // default
        public string Accept { get; set; } = "*";
        public Uri BaseAddress { get; set; }
        public long MaxResponseContentBufferSize { get; set; } = int.MaxValue;
        public TimeSpan Timeout { get; set; } = TimeSpan.FromMinutes(1);
        public bool AutomaticDecompression { get; set; } = false;
        public bool AllowAutoRedirect { get; set; } = true;
        public int MaxAutomaticRedirections { get; set; } = 5;

        private HttpClient HttpClient;

        public XttpClient()
        {

        }

        public void CancelPendingRequests() =>
            HttpClient.CancelPendingRequests();

        public void Dispose()
        {
            HttpClient.Dispose();
            BaseAddress = null;
        }

        public Task<HttpResponseMessage> SendAsync(HttpRequestMessage request) =>
            SendAsync(request, HttpCompletionOption.ResponseHeadersRead, CancellationToken.None);

        public Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
            HttpCompletionOption completionOption) =>
            SendAsync(request, completionOption, CancellationToken.None);

        public Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
            HttpCompletionOption completionOption, CancellationToken cancellationToken) =>
            HttpClient.SendAsync(request, completionOption, cancellationToken);

        public Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, 
            CancellationToken cancellationToken) =>
            SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
    }
}

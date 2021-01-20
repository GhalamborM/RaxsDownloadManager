using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace WDM.Downloaders.Downloads
{
    internal static class HttpHelper
    {
        static public TimeSpan GetDefaultTimeout() => TimeSpan.FromSeconds(30);

        static public HttpClient GetClient(TimeSpan timeout = default, HttpClientHandler clientHandler = null) =>
            new HttpClient(clientHandler ?? GetClientHandler()) { Timeout = timeout == default ? GetDefaultTimeout() : timeout };

        static public HttpClientHandler GetClientHandler(IWebProxy proxy = null) =>
            new HttpClientHandler { Proxy = proxy, UseProxy = proxy != null };

        static public HttpRequestMessage GetRequest(Uri uri, HttpMethod method = null, bool appendRange = false)
        {
            var request = new HttpRequestMessage(method ?? HttpMethod.Get, uri);
            if (appendRange)
                request.Headers.Range = new System.Net.Http.Headers.RangeHeaderValue(0, 255);
            return request;
        }

    }
}

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace WDM.Downloaders.Downloads
{
    static class HttpHelper
    {
        static public readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(30);

        static public HttpClient GetClient(TimeSpan timeout = default) =>
            new HttpClient { Timeout = timeout == default ? DefaultTimeout : timeout };

        static public HttpClientHandler GetClientHandler(IWebProxy proxy = null) =>
            new HttpClientHandler { Proxy = proxy, UseProxy = proxy != null };

        static public HttpRequestMessage GetRequest(Uri uri, HttpMethod method = null) =>
            new HttpRequestMessage(method ?? HttpMethod.Get, uri);

    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using RaxsDownloadManager.Helpers;
namespace RaxsDownloadManager.Downloads
{
    internal static class HttpHelper
    {
        const int MAX_CONN_PER_SERVER = 20;
        static readonly public TimeSpan DefaultTimeout = TimeSpan.FromMinutes(1);

        static public HttpClient GetClient(HttpMessageHandler clientHandler = null, TimeSpan timeout = default) =>
            new HttpClient(clientHandler ?? GetClientHandler()) { Timeout = timeout == default ? DefaultTimeout : timeout }
            .AppendDefaultHeaders();

        static public HttpRequestMessage GetRequest(Uri uri, HttpMethod method = null, bool appendRange = false)
        {
            var request = new HttpRequestMessage(method ?? HttpMethod.Get, uri);
            if (appendRange)
                request.Headers.Range = new System.Net.Http.Headers.RangeHeaderValue(0, 255);
            return request;
        }

        static public HttpRequestMessage GetRequestWithRange(Uri uri, HttpMethod method = null, long? fromRange = null, long? toRange = null)
        {
            var request = new HttpRequestMessage(method ?? HttpMethod.Get, uri);
            request.Headers.Range = new System.Net.Http.Headers.RangeHeaderValue(fromRange, toRange);
            return request;
        }

        #region Client Handler
        // Ref: https://github.com/NimaAra/Easy.Common/blob/master/Easy.Common/RestClient.cs#L52
        static public HttpMessageHandler GetClientHandler(IWebProxy proxy = null)
        {
#if NETCOREAPP || NET
            return new SocketsHttpHandler
            {
                // https://github.com/dotnet/corefx/issues/26895
                // https://github.com/dotnet/corefx/issues/26331
                // https://github.com/dotnet/corefx/pull/26839
                PooledConnectionLifetime = DefaultTimeout,
                PooledConnectionIdleTimeout = DefaultTimeout,
                MaxConnectionsPerServer = MAX_CONN_PER_SERVER,
                Proxy = proxy,
                UseProxy = proxy != null
            };
#else
            return new HttpClientHandler
            {
#if NETSTANDARD2_0 || NET471 || NET472
                MaxConnectionsPerServer = MAX_CONN_PER_SERVER,
#endif
                Proxy = proxy,
                UseProxy = proxy != null
            };
#endif
        }
        // Ref: https://github.com/NimaAra/Easy.Common/blob/master/Easy.Common/RestClient.cs#L514
        static public void ConfigureServicePointManager()
        {
            ServicePointManager.DnsRefreshTimeout = (int)DefaultTimeout.TotalMilliseconds;
            ServicePointManager.DefaultConnectionLimit = MAX_CONN_PER_SERVER;
        }
        #endregion

        public static async Task<HttpRequestMessage> Clone(this HttpRequestMessage httpRequestMessage)
        {
            HttpRequestMessage request = new HttpRequestMessage(httpRequestMessage.Method, httpRequestMessage.RequestUri)
            {
                Version = httpRequestMessage.Version,
                
            };

            httpRequestMessage.Properties.ToList()
                .ForEach(props => request.Properties.Add(props));

            httpRequestMessage.Headers.ToList()
                .ForEach(header => request.Headers.TryAddWithoutValidation(header.Key, header.Value));

            return request;
        }

        //https://stackoverflow.com/questions/18000583/re-send-httprequestmessage-exception
        //// using System.Reflection;
        //// using System.Net.Http;
        //// private const string SEND_STATUS_FIELD_NAME = "_sendStatus";
        //private void ResetSendStatus(HttpRequestMessage request)
        //{
        //    TypeInfo requestType = request.GetType().GetTypeInfo();
        //    FieldInfo sendStatusField = requestType.GetField(SEND_STATUS_FIELD_NAME, BindingFlags.Instance | BindingFlags.NonPublic);
        //    if (sendStatusField != null)
        //        sendStatusField.SetValue(request, 0);
        //    else
        //        throw new Exception($"Failed to hack HttpRequestMessage, {SEND_STATUS_FIELD_NAME} doesn't exist.");
        //}
        //Mono has bool HttpRequestMessage.is_used flag
    }
}

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;

namespace HttpDownloadManager.Helpers
{
    static public class HeadersHelper
    {
        static public HttpClient AppendDefaultHeaders(this HttpClient client)
        {
            client.DefaultRequestHeaders.UserAgent.Clear();
            client.DefaultRequestHeaders.UserAgent.TryParseAdd(UserAgentHelper.CHROME_USER_AGENT);
            return client;
        }
    }
}

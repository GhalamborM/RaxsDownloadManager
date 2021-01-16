using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace WDM.Downloaders.Downloads
{
    static class DownloadChecker
    {
        /// <summary>
        ///     Get a tiny information related to a URL link
        /// </summary>
        /// <param name="url">Url</param>
        public static async Task<DownloadInfo> GetInfoAsync(string url) =>
            await GetInfoAsync(new Uri(url));

        /// <summary>
        ///     Get a tiny information related to a URI link
        /// </summary>
        /// <param name="uri">Uri</param>
        public static async Task<DownloadInfo> GetInfoAsync(Uri uri)
        {
            using (var client = new HttpClient())
            using (var request = new HttpRequestMessage(HttpMethod.Head, uri))
            using (var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead))
            // Use ResponseHeadersRead to prevent a exception related to files larger than 2GB.
            // Setting client.MaxResponseContentBufferSize to long.MaxValue will cause a exception! 2147483647 is the limit
            {
                var content = response.Content;
                if (content == null) return new DownloadInfo { StatusCode = response.StatusCode };
                DownloadInfo downloadInfo = new DownloadInfo
                {
                    ContentLength = content.Headers.ContentLength,
                    ContentType = content.Headers.ContentType.MediaType,
                    StatusCode = response.StatusCode,
                    ServerSideFileName = content.Headers.ContentDisposition?.FileName
                };
                if (downloadInfo.ContentLength.HasValue && response.IsSuccessStatusCode)
                {
                    if (downloadInfo.ContentLength.Value <= int.MaxValue)
                    {
                        downloadInfo.CanDetectPartialContent = true;
                        var statusCode = await GetStatusCodeAsync(uri).ConfigureAwait(false);
                        downloadInfo.StatusCode = statusCode;
                        downloadInfo.PartialContent = statusCode == HttpStatusCode.PartialContent;
                    }
                    else
                        downloadInfo.CanDetectPartialContent = downloadInfo.PartialContent = false;
                }
                return downloadInfo;
            }
        }
        private static async Task<HttpStatusCode> GetStatusCodeAsync(Uri uri)
        {
            using (var client = new HttpClient())
            using (var request = new HttpRequestMessage(HttpMethod.Head, uri))
            using (var response = await client.SendAsync(request)) 
                // since our file is lower than 2GB we can avoid ResponseHeadersRead part
                // now we can actually get the partial content, if possible
                return response.StatusCode;
        }
    }
}

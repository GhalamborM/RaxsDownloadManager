using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using static WDM.Downloaders.Downloads.HttpHelper;

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
            using (var client = GetClient())
            using (var request = GetRequest(uri, null, true))
            using (var response = await client.SendAsync(request))
            // Use ResponseHeadersRead to prevent a exception related to files larger than 2GB. But since I used Ranges no exception will throw.
            // Setting client.MaxResponseContentBufferSize to long.MaxValue will cause a exception! 2147483647 is the limit
            {
                var statusCode = response.StatusCode;
                var content = response.Content;
                if (content == null) return new DownloadInfo { StatusCode = statusCode };
                DownloadInfo downloadInfo = new DownloadInfo
                {
                    ContentLength = content.Headers.ContentLength,
                    ContentType = content.Headers.ContentType.MediaType,
                    StatusCode = response.StatusCode,
                    ServerSideFileName = content.Headers.ContentDisposition?.FileName,
                    PartialContent = statusCode == HttpStatusCode.PartialContent
                };
                if (response.Content.Headers.ContentRange?.HasLength ?? false)
                    downloadInfo.ContentLength = response.Content.Headers.ContentRange.Length;
                //if (downloadInfo.ContentLength.HasValue && response.IsSuccessStatusCode)
                //{
                //    if (downloadInfo.ContentLength.Value <= int.MaxValue)
                //    {
                //        downloadInfo.CanDetectPartialContent = true;
                //        //var statusCode = await GetStatusCodeAsync(uri).ConfigureAwait(false);
                //        downloadInfo.StatusCode = statusCode;
                //        downloadInfo.PartialContent = statusCode == HttpStatusCode.PartialContent;
                //    }
                //    else
                //        downloadInfo.CanDetectPartialContent = downloadInfo.PartialContent = false;
                //}
                return downloadInfo;
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace WDM.Downloader.Downloads
{
    static class DownloadChecker
    {

        public static async Task<DownloadInfo> GetInfoAsync(string url)
        {
            using (var client = new HttpClient())
            using (var request = new HttpRequestMessage(HttpMethod.Head, url))
            using (var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead))
            // Using ResponseHeadersRead to prevent a exception related to files larger than 2GB 
            // Setting client.MaxResponseContentBufferSize to long.MaxValue will cause a exception! 2147483647 is the limit
            {
                var content = response.Content;
                if (content == null) return default;
                DownloadInfo downloadInfo = new DownloadInfo
                {
                    ContentLength = content.Headers.ContentLength,
                    ContentType = content.Headers.ContentType.MediaType,
                    StatusCode = response.StatusCode,
                    ServerSideFileName = content.Headers.ContentDisposition?.FileName
                };
                if (downloadInfo.ContentLength.HasValue)
                {
                    if (downloadInfo.ContentLength.Value <= int.MaxValue)
                    {
                        downloadInfo.CanDetectPartialContent = true;
                    }
                    else
                        downloadInfo.CanDetectPartialContent = downloadInfo.PartialContent = false;
                }
                return downloadInfo;
            }
        }
    }
}

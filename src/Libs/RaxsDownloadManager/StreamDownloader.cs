using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Http;
using System.IO;
using System.Threading;
using RaxsDownloadManager.Streams;
using System.Net;
using System.Threading.Tasks;

namespace RaxsDownloadManager
{
    public class StreamDownloader
    {
        public static int SIZE => IntPtr.Size;
        private const int DefaultCopyBufferLength = 8192;
        private const int DefaultDownloadBufferLength = 65536;
        public async Task<long?> DownloadBits(HttpResponseMessage response, Stream output, CancellationToken cancellationToken)
        {
            long readed = 0;
            try
            {
                var contentLength = response.Content.Headers.ContentLength ?? -1;

                byte[] copyBuffer = new 
                    byte[contentLength == -1 || contentLength > DefaultDownloadBufferLength ? 
                    DefaultDownloadBufferLength : contentLength];

                if (output is ChunkedMemoryStream)
                {
                    //If IntPtr.Size == 4 then it's 32 bit (4 x 8). If IntPtr.Size == 8 then it's 64 bit (8 x 8)

                    if (IntPtr.Size == 4 && contentLength > int.MaxValue) // if it was 32 bit app
                        throw new Exception("The message length limit was exceeded");

                    output.SetLength(copyBuffer.Length);
                }

                using (Stream readStream = await response.Content.ReadAsStreamAsync())
                {
                    if (readStream != null)
                    {
                        int bytesRead;
                        while ((bytesRead = await readStream.ReadAsync(copyBuffer, 0, copyBuffer.Length).ConfigureAwait(false)) != 0 &&
                            !cancellationToken.IsCancellationRequested)
                        {
                            await output.WriteAsync(copyBuffer, 0, bytesRead).ConfigureAwait(false);
                            readed += bytesRead;
                        }
                    }
                }

            }
            catch { }
            return readed;
        }
    }
}

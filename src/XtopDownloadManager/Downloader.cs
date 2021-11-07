using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using XtopDownloadManager.Exceptions;
using XtopDownloadManager.Storage;
using XtopDownloadManager.Downloads;
using static XtopDownloadManager.Downloads.HttpHelper;
using System.Net.Http;
using System.IO;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Threading;
//https://github.com/Microsoft/dotnet/blob/master/releases/net471/KnownIssues/534719-Networking.ServicePoint.ConnectionLimit%20default%20behavior%20changed.md
//http://www.bizcoder.com/httpclient-it-lives-and-it-is-glorious
//https://stackoverflow.com/questions/15705092/do-httpclient-and-httpclienthandler-have-to-be-disposed-between-requests
//https://github.com/microsoft/vs-threading/blob/main/doc/index.md
namespace XtopDownloadManager
{
    public class Downloader
    {
        const int BUFFER_SIZE = 1024/*8192*//* *1024*/;
        public List<DownloadSegment> Segments { get; } = new List<DownloadSegment>();
        public DownloadInfo DownloadInfo { get; internal set; }
        public FileStorage FileStorage { get; internal set; }
        public Uri Uri { get; private set; }
        public string Path { get; private set; }
        public async Task ConfigureAsync(Uri uri, string path)
        {
            if(!uri.Scheme.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                throw new NotSupportedSchemeException($"'{uri.Scheme}' scheme is not supported");
            Uri = uri;
            if (!string.IsNullOrEmpty(path))
                Path = path;
            DownloadInfo = await DownloadChecker.GetInfoAsync(uri).ConfigureAwait(false);
        }
        public long BytesWritten { get; private set; }
        bool _canDownload = true;
        readonly public Stopwatch Stopwatch = new Stopwatch();
        public DateTime started;
        public IWebProxy Proxy { get; set; } = null;
        public async Task StartBatch(List<Uri> urls)
        {
            if (urls?.Count > 0)
            {
                Debug.WriteLine(">>>>>>>>>>>>>>>>>>>>>>>> Download Started <<<<<<<<<<<<<<<<<<<<<<<<<");
                foreach (var uri in urls)
                {
                    await ConfigureAsync(uri, null);
                    await StartDownload();
                }

                Debug.WriteLine(">>>>>>>>>>>>>>>>>>>>>>>> Download Completed <<<<<<<<<<<<<<<<<<<<<<<<<");

            }

        }
        public async Task StartDownload()
        {
            _canDownload = true;
            Stopwatch.Reset();
            using (var handler = GetClientHandler(Proxy))
            using (var client = GetClient(handler))
            using (var request = GetRequest(Uri))
            {
                //if (BytesWritten > 0)
                //    request.Headers.Range = new RangeHeaderValue(BytesWritten, DownloadInfo.ContentLength);
                // vaghti range bishtar az hajme file bashe> response.StatusCode RequestedRangeNotSatisfiable
                //System.IO.IOException: 'Unable to read data from the transport connection: An existing connection was forcibly closed by the remote host.'
                using (var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead))
                {
                    Debug.WriteLine(response.StatusCode);
                    if (response.StatusCode != HttpStatusCode.OK && 
                        response.StatusCode != HttpStatusCode.PartialContent)
                    {
                        Console.WriteLine("response.StatusCode " + response.StatusCode);
                        return;
                    }
                    using (var inputStream = await response.Content.ReadAsStreamAsync())
                    using (var fileStream = new FileStream(Path, FileMode.Append, FileAccess.Write, FileShare.ReadWrite))
                    {
                        int bytesRead;
                        Stopwatch.Start();
                        SpeedStopwatch.Start();
                        Debug.WriteLine("Percentage\t\tEstimated\t\t\tSpeed");
                        started = DateTime.Now;
                        DateTime started2 = DateTime.Now;
                        do
                        {
                            var buffer = new byte[BUFFER_SIZE];
                            bytesRead = await inputStream.ReadAsync(buffer, 0, buffer.Length).ConfigureAwait(false);
                            await fileStream.WriteAsync(buffer, 0, bytesRead);
                            BytesWritten += bytesRead;
                            double seconds = Stopwatch.Elapsed.TotalSeconds;
                            TimeSpan elapsedTime = DateTime.Now - started;
                            TimeSpan estimatedTime =
                                TimeSpan.FromSeconds(
                                    (DownloadInfo.ContentLength.Value - BytesWritten) /
                                    (BytesWritten / elapsedTime.TotalSeconds));

                            var bytesPerSecond = (BytesWritten / (DateTime.Now - started2).TotalSeconds);

                            Debug.WriteLine(GetPercentage(BytesWritten, DownloadInfo.ContentLength.Value) + "\t\t" +
                              estimatedTime.ToString() + "\t\t\t" + GetMeg(bytesPerSecond));

                            started2 = DateTime.Now;
                        }
                        while (_canDownload && bytesRead > 0);

                        await fileStream.FlushAsync();
                        Stopwatch.Stop();
                    }
                }
            }
        }
        protected async Task ReadStreamAsync(Stream stream, CancellationToken token)
        {
            while (true)
            {
                token.ThrowIfCancellationRequested();

            }
        }
        public void PauseDownload() => _canDownload = false;

        double GetKilo(double d) => Math.Floor(d / 1024);
        double GetMeg(double d) => Math.Floor(GetKilo(d) / 1024);
        public int GetPercentage(long writtenBytes, long contentLength)
        {
           
            return (int)(((double)writtenBytes / contentLength) * 100);
        }

        int _dataIndex = 0;
        const int MAX_DATA_POINTS = 5;
        double[] _dataPoints = new double[MAX_DATA_POINTS];

        readonly Stopwatch SpeedStopwatch = new Stopwatch();
        public double GetAverageSpeed(double receivedBytes, long lastWrittenBytes, DateTime date = default)
        {
            SpeedStopwatch.Stop();
            double msElapsed = Stopwatch.Elapsed.TotalMilliseconds;
            double bytesDownloaded = receivedBytes - lastWrittenBytes;
            double dataPoint = (double)bytesDownloaded / (msElapsed / 1000);
            _dataPoints[_dataIndex++ % MAX_DATA_POINTS] = dataPoint;

            double downloadSpeed = _dataPoints.Average();
            SpeedStopwatch.Restart();
            return downloadSpeed;
        }
    }
}

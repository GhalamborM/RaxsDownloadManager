using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using WDM.Downloaders.Exceptions;
using WDM.Downloaders.Storage;
using WDM.Downloaders.Downloads;
using System.Net.Http;
using System.IO;
using System.Diagnostics;
using System.Linq;
using System.Net;

namespace WDM.Downloaders
{
    public class Downloader
    {
        const int BUFFER_SIZE = 8192/* *1024*/;
        public List<DownloadSegment> Segments { get; } = new List<DownloadSegment>();
        public DownloadInfo DownloadInfo { get; internal set; }
        public FileStorage FileStorage { get; internal set; }
        public Uri Uri { get; private set; }
        public string Path { get; private set; }
        public async Task ConfigureAsync(Uri uri, string path)
        {
            if(!uri.Scheme.ToLower().StartsWith("http"))
                throw new NotSupportedSchemeException($"'{uri.Scheme}' scheme is not supported");
            Uri = uri;
            Path = path;
            DownloadInfo = await DownloadChecker.GetInfoAsync(uri);
        }
        public long BytesWritten { get; private set; }
        bool _canDownload = true;
        readonly public Stopwatch Stopwatch = new Stopwatch();
        public DateTime started;
        public IWebProxy Proxy { get; set; } = null;
        public async void StartDownload()
        {
            _canDownload = true;
            Stopwatch.Reset();
            using (var handler = new HttpClientHandler { Proxy = Proxy })
            using (var client = new HttpClient(handler))
            using (var request = new HttpRequestMessage(HttpMethod.Get, Uri))
            {
                if (BytesWritten > 0)
                    request.Headers.Range = new System.Net.Http.Headers.RangeHeaderValue(BytesWritten, DownloadInfo.ContentLength);
                // vaghti range bishtar az hajme file bashe> response.StatusCode RequestedRangeNotSatisfiable
                //System.IO.IOException: 'Unable to read data from the transport connection: An existing connection was forcibly closed by the remote host.'
                using (var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead))
                {
                    Debug.WriteLine(response.StatusCode);
                    if (response.StatusCode != System.Net.HttpStatusCode.OK && 
                        response.StatusCode != System.Net.HttpStatusCode.PartialContent)
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

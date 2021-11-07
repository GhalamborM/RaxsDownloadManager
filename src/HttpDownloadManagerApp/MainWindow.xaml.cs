using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using HttpDownloadManager;
using System.IO;
using System.Diagnostics;
using System.Windows.Threading;
using HttpDownloadManager.Helpers;
using System.Text.RegularExpressions;

namespace HttpDownloadManagerApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        DispatcherTimer Timer = new DispatcherTimer();
        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded;
            Timer.Interval = TimeSpan.FromSeconds(1);
            Timer.Tick += Timer_Tick;

            var all = Thinktecture.Helpers.MimeTypeLookup._mappings;
        //https://gist.github.com/ChristianWeyer/eea2cb567932e345cdfa
            var aa = all.Select(x => new C { Key = x.Key.ToLower(), Value = x.Value.ToLower() });
            var bb = aa.OrderBy(x=> x.Value).ToList();
            var dict = new Dictionary<string, List<string>>();

            foreach(var item in bb)
            {
                if (!dict.ContainsKey(item.Value))
                    dict.Add(item.Value, new List<string> { item.Key });
                else
                    dict[item.Value].Add(item.Key);
            }
            //var json = Newtonsoft.Json.JsonConvert.SerializeObject(dict);
            //File.WriteAllText("aaaa.json", json);
            //var list = new List<string>();
            //foreach(var item in all)
            //{
            //    list.Add(item.Value + "\t\t\t" + item.Key);
            //}

            //list.Sort();
            //foreach (var item in list)
            //    Debug.WriteLine(item);
            //8     64
            Debug.WriteLine(StreamDownloader.SIZE + "     " +(StreamDownloader.SIZE * 8));

        }
        class C
        {
            public string Key { get; set; }
            public string Value { get; set; }
        }
        class XX
        {
            public string Key { get; set; }
            public List<string> Values { get; set; } = new List<string>();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            var tick = Environment.TickCount;
            var span = TimeSpan.FromMilliseconds(tick);

            Debug.WriteLine(tick+"\t\t\t" +span.ToString());
            //if (downloader.BytesWritten < 3000) return;
            //var lastWrittenBytes = downloader.BytesWritten;
            //TimeSpan elapsedTime = DateTime.Now - downloader.started;
            //TimeSpan estimatedTime =
            //    TimeSpan.FromSeconds(
            //        (downloader.DownloadInfo.ContentLength.Value - downloader.BytesWritten) /
            //        ((double)downloader.BytesWritten / elapsedTime.TotalSeconds));

            //var bytesPerSecond = (lastWrittenBytes / elapsedTime.TotalSeconds);

            //Debug.WriteLine(downloader.GetPercentage(downloader.BytesWritten, downloader.DownloadInfo.ContentLength.Value) + "\t\t" /*+ (BytesWritten / seconds) + "       "*/ +
            //   downloader.BytesWritten + "\t\t\t" + bytesPerSecond/* + "\t\t\t" + speed*/);
        }

        Downloader downloader = new Downloader();
        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine(Convert.ToBase64String(Encoding.UTF8.GetBytes("https://video-mct1-1.xx.fbcdn.net/v/t39.12897-2/44063894_337791370307095_5837111153720819712_n.m4a?_nc_cat=101&ccb=1-5&_nc_sid=02c1ff&_nc_ohc=I4kBh9o85lsAX_az4Nw&_nc_ad=z-m&_nc_cid=0&_nc_ht=video-mct1-1.xx&oh=e7198cdcbbedfec245ba11063e1366c8&oe=61508EFC")));
            //Timer.Start();
            var all = HttpDownloadManagerApp.MimeTypeMap._mappings.Value;
           var t = HttpDownloadManagerApp.MimeTypeMap.GetExtension("application/x-zip-compressed");
            var url = "http://127.0.0.1/1.mkv";
            //url = "https://dl16.ftk.pw/user/shahab2/film/Ludo.2020.720p.BluRay.Farsi.Dubbed.Film2Movie_Asia.mkv";
            //url = "https://github.com/dotnet/samples/archive/master.zip";
            var name = "AZ " + DateTime.Now.Hour + "-" + DateTime.Now.Minute + "-" + DateTime.Now.Second + ".ts";
            url = "https://isubtitles.org/download/arrow/farsi-persian/1801961";

            url = "https://video-mct1-1.xx.fbcdn.net/v/t39.12897-2/44063894_337791370307095_5837111153720819712_n.m4a?_nc_cat=101&ccb=1-5&_nc_sid=02c1ff&_nc_ohc=I4kBh9o85lsAX_az4Nw&_nc_ad=z-m&_nc_cid=0&_nc_ht=video-mct1-1.xx&oh=e7198cdcbbedfec245ba11063e1366c8&oe=61508EFC";
            var path = Path.Combine(@"D:\Barname nevisi\ramtinak\HttpDownloadManager\src\HttpDownloadManagerApp\bin\Debug", url.GetFileName());
            await downloader.ConfigureAsync(new Uri(url), path);

        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            _ = downloader.StartDownload();
            //Timer.Start();
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            downloader.PauseDownload();
            //Timer.Stop();
        }

        private void button2_Click(object sender, RoutedEventArgs e)
        {
            //C:\Users\Ramtin\AppData\Roaming
            Debug.WriteLine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData).ToString());
            //C:\Users\Ramtin\AppData\Local
            Debug.WriteLine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData));
            //C:\Users\Ramtin\AppData\Roaming
            Debug.WriteLine(Environment.GetEnvironmentVariable("APPDATA"));
            //C:\Users\Ramtin\Downloads
            var folder = getDownloadFolderPath();
            Debug.WriteLine(folder);
            var fi = File.Create(Path.Combine(folder, "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa.txt"));

            //var fileInfo = new FileInfo("D:\\Barname nevisi\\Projects\\WindowsDownloadManger\\src\\HttpDownloadManager\\bin\\Debug\\Ludo.2020.720p.BluRay.Farsi.Dubbed.Film2Movie_Asia.mkv");
            //Debug.WriteLine(fileInfo?.Length);
            File.Create(@"D:\Barname nevisi\Projects\WindowsDownloadManger\src\HttpDownloadManager\bin\Debug\ABC\Xyz\BB\a.txt");
            var aboc = new FileStream(@"D:\Barname nevisi\Projects\WindowsDownloadManger\src\HttpDownloadManager\bin\Debug\ABC\Xyz\BB\a.txt", FileMode.Create);
        }
        public static string getHomePath()
        {
            // Not in .NET 2.0
            // System.Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            if (System.Environment.OSVersion.Platform == System.PlatformID.Unix)
                return System.Environment.GetEnvironmentVariable("HOME");

            return System.Environment.ExpandEnvironmentVariables("%HOMEDRIVE%%HOMEPATH%");
        }
        public static string getDownloadFolderPath()
        {
            //if (System.Environment.OSVersion.Platform == System.PlatformID.Unix)
            {
                string pathDownload = System.IO.Path.Combine(getHomePath(), "Downloads");
                return pathDownload;
            }

            return System.Convert.ToString(
                Microsoft.Win32.Registry.GetValue(
                     @"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\Shell Folders"
                    , "{374DE290-123F-4565-9164-39C4925E467B}"
                    , String.Empty
                )
            );
        }

        private void button3_Click(object sender, RoutedEventArgs e)
        {
            //await downloader.StartBatch(urls);
        }

    }
}

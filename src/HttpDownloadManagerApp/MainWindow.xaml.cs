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
using HttpDownloadManager.Parsers.M3U8;

namespace HttpDownloadManager
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
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
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


            var url = "http://127.0.0.1/1.mkv";
            //url = "https://dl16.ftk.pw/user/shahab2/film/Ludo.2020.720p.BluRay.Farsi.Dubbed.Film2Movie_Asia.mkv";
            //url = "https://github.com/dotnet/samples/archive/master.zip";
            var name = "AZ " + DateTime.Now.Hour + "-" + DateTime.Now.Minute + "-" + DateTime.Now.Second + ".ts";

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

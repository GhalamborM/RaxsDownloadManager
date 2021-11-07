using System;
using System.IO;

namespace XtopDownloadManager.Helpers
{
    public static class PathHelper
    {
        public static string GetLocalFolderPath() => Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        public static string GetRoamingFolderPath() => Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

        public static string GetHomeFolderPath()
        {
            if (Environment.OSVersion?.Platform == PlatformID.Unix)
                return Environment.GetEnvironmentVariable("HOME");
            else
                return Environment.ExpandEnvironmentVariables("%HOMEDRIVE%%HOMEPATH%");
        }
        public static string GetDownloadFolderPath()
        {
            return Path.Combine(GetHomeFolderPath(), "Downloads");
            // Get download folder from registry
            //return Convert.ToString(
            //    Microsoft.Win32.Registry.GetValue(
            //         @"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\Shell Folders"
            //        , "{374DE290-123F-4565-9164-39C4925E467B}"
            //        , string.Empty)
            //);
        }
    }
}
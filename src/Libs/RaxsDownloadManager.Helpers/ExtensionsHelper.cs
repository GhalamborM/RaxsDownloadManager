using System;
using System.IO;
using System.Net;

namespace RaxsDownloadManager.Helpers
{
    public static class ExtensionsHelper
    {
        public static string GetFileName(this string url) =>
            new Uri(url).GetFileName();

        public static string GetFileName(this Uri uri) =>
           Uri.UnescapeDataString(WebUtility.UrlDecode(Path.GetFileName(uri.GetLeftPart(UriPartial.Path))));
    }
}

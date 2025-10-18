using RD.Core.Models;
using System.Text.RegularExpressions;

namespace RD.Core.Helpers;

public static class Helper
{
    public static string FormatBytes(long bytes)
    {
        string[] sizes = { "B", "KB", "MB", "GB", "TB" };
        double len = bytes;
        int order = 0;
        while (len >= 1024 && order < sizes.Length - 1)
        {
            order++;
            len /= 1024;
        }
        return $"{len:0.##} {sizes[order]}";
    }

    public static string GetStatusText(DownloadStatus status) => status switch
    {
        DownloadStatus.Pending => "Pending",
        DownloadStatus.Downloading => "Downloading",
        DownloadStatus.Paused => "Paused",
        DownloadStatus.Completed => "Completed",
        DownloadStatus.Failed => "Failed",
        DownloadStatus.Cancelled => "Cancelled",
        _ => "Unknown"
    };

    public static bool IsValidUrl(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return false;

        return Uri.TryCreate(text, UriKind.Absolute, out var uri) 
            && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
    }

    public static IEnumerable<string> ExtractUrls(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return [];

        var urlPattern = @"https?://[^\s<>""\[\]{}|\\^`]+";
        var matches = Regex.Matches(text, urlPattern, RegexOptions.IgnoreCase);
        
        return matches.Select(m => m.Value).Where(IsValidUrl);
    }
}

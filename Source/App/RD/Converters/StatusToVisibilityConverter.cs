using RD.Core.Models;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace RD.Converters;

public class StatusToVisibilityConverter : IValueConverter
{
    public static readonly StatusToVisibilityConverter Instance = new();

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is DownloadItem item)
        {
            return item.Status == DownloadStatus.Completed ? Visibility.Collapsed : Visibility.Visible;
        }
        return Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is Visibility visibility)
        {
            return visibility == Visibility.Visible;
        }
        return false;
    }
}
public class StatusToEmptyConverter : IValueConverter
{
    public static readonly StatusToVisibilityConverter Instance = new();

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is DownloadItem item)
        {
            return item.Status == DownloadStatus.Completed ? "" : item.PercentageComplete.ToString();
        }
        return "";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is Visibility visibility)
        {
            return visibility == Visibility.Visible;
        }
        return false;
    }
}
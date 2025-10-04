using RD.Core.Helpers;
using System.Globalization;
using System.Windows.Data;

namespace RD.Converters;

public class BytesToStringConverter : IValueConverter
{
    public static readonly BytesToStringConverter Instance = new();

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is long bytes)
        {
            return Helper.FormatBytes(bytes);
        }
        return "0 B";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
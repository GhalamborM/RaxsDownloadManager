using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace RD.Converters;

public class CategoryPanelIconConverter : IValueConverter
{
    // (collapse)
    private const string ChevronLeftPath = "M15.41 7.41L14 6l-6 6 6 6 1.41-1.41L10.83 12z";
    
    // (expand)
    private const string ChevronRightPath = "M10 6L8.59 7.41 13.17 12l-4.58 4.59L10 18l6-6z";

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool isCollapsed)
        {
            var pathString = isCollapsed ? ChevronRightPath : ChevronLeftPath;
            return Geometry.Parse(pathString);
        }
        
        return Geometry.Parse(ChevronLeftPath);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

using System.Globalization;
using System.Windows.Data;

namespace RD.Converters;

public class CategoryPanelTooltipConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool isCollapsed)
        {
            return isCollapsed ? "Expand Categories" : "Collapse Categories";
        }
        
        return "Collapse Categories";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

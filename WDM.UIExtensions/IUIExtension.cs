using System.Windows.Controls;

namespace WDM.UIExtensions
{
    public interface IUIExtension
    {
        Control[] Controls { get; }
    }
}

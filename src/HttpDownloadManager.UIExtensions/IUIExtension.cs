using System.Windows.Controls;

namespace HttpDownloadManager.UIExtensions
{
    public interface IUIExtension
    {
        Control[] Controls { get; }
    }
}

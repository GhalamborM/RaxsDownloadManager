using RD.Services;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows;

namespace RD.Localization;

public class LocalizeExtension : MarkupExtension
{
    public string Key { get; set; }

    public LocalizeExtension()
    {
        Key = string.Empty;
    }

    public LocalizeExtension(string key)
    {
        Key = key;
    }

    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        if (string.IsNullOrEmpty(Key))
            return "[Empty Key]";

        if (serviceProvider.GetService(typeof(IProvideValueTarget)) is IProvideValueTarget target)
        {
            if (target.TargetObject?.GetType().Name == "SharedDp")
                return this;

            if (target.TargetObject is DependencyObject depObj &&
                System.ComponentModel.DesignerProperties.GetIsInDesignMode(depObj))
            {
                return $"[{Key}]";
            }
        }

        var binding = new System.Windows.Data.Binding
        {
            Source = LocalizationService.Instance,
            Path = new PropertyPath($"[{Key}]"),
            Mode = BindingMode.OneWay
        };

        return binding.ProvideValue(serviceProvider);
    }
}

namespace RD.Core.Models;

public class UISettings
{
    /// <summary>
    /// Main window settings
    /// </summary>
    public WindowSettings? MainWindow { get; set; } = new();

    /// <summary>
    /// DataGrid column width settings
    /// </summary>
    public Dictionary<string, double> DataGridColumnWidths { get; set; } = new();

    /// <summary>
    /// Whether the category panel is collapsed
    /// </summary>
    public bool IsCategoryPanelCollapsed { get; set; } = false;
}

public class WindowSettings
{
    public double Width { get; set; } = 1000;
    public double Height { get; set; } = 500;
    public double Left { get; set; } = double.NaN;
    public double Top { get; set; } = double.NaN;
    public bool IsMaximized { get; set; } = false;
}

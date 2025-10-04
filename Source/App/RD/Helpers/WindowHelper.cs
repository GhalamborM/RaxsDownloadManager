using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace RD.Helpers;
#pragma warning disable
public static class WindowHelper
{
    [DllImport("dwmapi.dll")]
    private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);

    private const int DWMWA_USE_IMMERSIVE_DARK_MODE = 20;
    private const int DWMWA_MICA_EFFECT = 1029;

    public static void EnableMicaEffect(Window window)
    {
        try
        {
            var windowHelper = new WindowInteropHelper(window);
            var hwnd = windowHelper.Handle;

            if (hwnd == IntPtr.Zero)
            {
                window.SourceInitialized += (s, e) =>
                {
                    var helper = new WindowInteropHelper(window);
                    EnableMicaForWindow(helper.Handle);
                };
            }
            else
            {
                EnableMicaForWindow(hwnd);
            }
        }
        catch
        {
        }
    }

    private static void EnableMicaForWindow(IntPtr hwnd)
    {
        try
        {
            // Enable Mica effect
            int micaEffect = 1;
            DwmSetWindowAttribute(hwnd, DWMWA_MICA_EFFECT, ref micaEffect, sizeof(int));

            // Set dark mode if needed
            int darkMode = 1;
            DwmSetWindowAttribute(hwnd, DWMWA_USE_IMMERSIVE_DARK_MODE, ref darkMode, sizeof(int));
        }
        catch
        {
        }
    }
}
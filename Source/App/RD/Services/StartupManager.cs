using Microsoft.Win32;
using System.Diagnostics;
using System.Reflection;

namespace RD.Services;

public class StartupManager
{
    private const string AppName = "RaxsDownloadManager";
    private const string RunKeyPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";

    public static bool SetStartup(bool enable)
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(RunKeyPath, true);
            if (key == null)
            {
                Debug.WriteLine("Failed to open registry key for startup configuration");
                return false;
            }

            if (enable)
            {
                var exePath = GetExecutablePath();
                if (string.IsNullOrEmpty(exePath))
                {
                    Debug.WriteLine("Failed to get executable path");
                    return false;
                }

                key.SetValue(AppName, $"\"{exePath}\"");
                Debug.WriteLine($"Added application to startup: {exePath}");
                return true;
            }
            else
            {
                key.DeleteValue(AppName, false);
                Debug.WriteLine("Removed application from startup");
                return true;
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Failed to set startup configuration: {ex.Message}");
            return false;
        }
    }

    public static bool IsStartupEnabled()
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(RunKeyPath, false);
            if (key == null)
            {
                return false;
            }

            var value = key.GetValue(AppName) as string;
            return !string.IsNullOrEmpty(value);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Failed to check startup status: {ex.Message}");
            return false;
        }
    }

    private static string GetExecutablePath()
    {
        try
        {
            // Get the path to the executable
            var assembly = Assembly.GetExecutingAssembly();
            var location = assembly.Location;

            // If it's a .dll (common in .NET Core/5+), get the process path instead
            if (location.EndsWith(".dll", StringComparison.OrdinalIgnoreCase))
            {
                var process = Process.GetCurrentProcess();
                location = process.MainModule?.FileName ?? string.Empty;
            }

            return location;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Failed to get executable path: {ex.Message}");
            return string.Empty;
        }
    }
}

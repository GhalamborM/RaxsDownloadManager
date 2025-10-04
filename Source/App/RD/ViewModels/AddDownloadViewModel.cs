using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RD.Core.Models;
using System.Windows;
using SaveFileDialog = Microsoft.Win32.SaveFileDialog;
using MessageBox = System.Windows.MessageBox;

namespace RD.ViewModels;
#pragma warning disable
public partial class AddDownloadViewModel : ObservableObject
{
    [ObservableProperty]
    private string _url = string.Empty;

    [ObservableProperty]
    private string _fileName = string.Empty;

    [ObservableProperty]
    private string _downloadPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

    [ObservableProperty]
    private int _maxSegments = 8;

    [ObservableProperty]
    private int _bufferSize = 8192;

    [ObservableProperty]
    private int _timeoutMinutes = 5;

    [ObservableProperty]
    private int _retryAttempts = 3;

    [ObservableProperty]
    private int _retryDelaySeconds = 2;

    [ObservableProperty]
    private bool _overwriteExisting = false;

    [ObservableProperty]
    private bool _enableResume = true;

    [ObservableProperty]
    private bool _useProxy = false;

    [ObservableProperty]
    private string _proxyHost = string.Empty;

    [ObservableProperty]
    private int _proxyPort = 8080;

    [ObservableProperty]
    private string _proxyUsername = string.Empty;

    [ObservableProperty]
    private string _proxyPassword = string.Empty;

    [ObservableProperty]
    private bool _useAuthentication = false;

    [ObservableProperty]
    private AuthenticationType _authenticationType = AuthenticationType.None;

    [ObservableProperty]
    private string _authUsername = string.Empty;

    [ObservableProperty]
    private string _authPassword = string.Empty;

    [ObservableProperty]
    private string _authToken = string.Empty;

    [ObservableProperty]
    private string _authApiKey = string.Empty;

    [ObservableProperty]
    private string _authHeaderName = "Authorization";

    public event Action<string, DownloadOptions>? DownloadAdded;

    public static Array AuthenticationTypes => Enum.GetValues<AuthenticationType>();

    [RelayCommand]
    private void BrowseDownloadPath()
    {
        var dialog = new SaveFileDialog
        {
            Title = "Select Download Location",
            FileName = FileName,
            CheckPathExists = true
        };

        if (dialog.ShowDialog() == true)
        {
            DownloadPath = System.IO.Path.GetDirectoryName(dialog.FileName) ?? DownloadPath;
            FileName = System.IO.Path.GetFileName(dialog.FileName);
        }
    }

    [RelayCommand]
    private void AddDownload()
    {
        if (string.IsNullOrWhiteSpace(Url))
        {
            MessageBox.Show("Please enter a valid URL.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (string.IsNullOrWhiteSpace(FileName))
        {
            MessageBox.Show("Please enter a file name.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        try
        {
            var fullPath = System.IO.Path.Combine(DownloadPath, FileName);
            var options = CreateDownloadOptions(fullPath);
            DownloadAdded?.Invoke(Url, options);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error creating download: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    private void Cancel()
    {
        
    }

    private DownloadOptions CreateDownloadOptions(string fullPath)
    {
        var options = new DownloadOptions
        {
            Url = Url,
            FilePath = fullPath,
            MaxSegments = MaxSegments,
            BufferSize = BufferSize,
            Timeout = TimeSpan.FromMinutes(TimeoutMinutes),
            RetryAttempts = RetryAttempts,
            RetryDelay = TimeSpan.FromSeconds(RetryDelaySeconds),
            OverwriteExisting = OverwriteExisting,
            EnableResume = EnableResume
        };

        if (UseProxy && !string.IsNullOrWhiteSpace(ProxyHost))
        {
            options.Proxy = new ProxyConfiguration
            {
                Host = ProxyHost,
                Port = ProxyPort,
                Username = string.IsNullOrWhiteSpace(ProxyUsername) ? null : ProxyUsername,
                Password = string.IsNullOrWhiteSpace(ProxyPassword) ? null : ProxyPassword
            };
        }

        if (UseAuthentication && AuthenticationType != AuthenticationType.None)
        {
            options.Authentication = new AuthenticationConfiguration
            {
                Type = AuthenticationType,
                Username = string.IsNullOrWhiteSpace(AuthUsername) ? null : AuthUsername,
                Password = string.IsNullOrWhiteSpace(AuthPassword) ? null : AuthPassword,
                Token = string.IsNullOrWhiteSpace(AuthToken) ? null : AuthToken,
                ApiKey = string.IsNullOrWhiteSpace(AuthApiKey) ? null : AuthApiKey,
                HeaderName = AuthHeaderName
            };
        }

        return options;
    }

    partial void OnUrlChanged(string value)
    {
        if (!string.IsNullOrWhiteSpace(value) && string.IsNullOrWhiteSpace(FileName))
        {
            try
            {
                var uri = new Uri(value);
                var suggestedFileName = System.IO.Path.GetFileName(uri.LocalPath);
                if (!string.IsNullOrWhiteSpace(suggestedFileName))
                {
                    FileName = suggestedFileName;
                }
            }
            catch
            {
            }
        }
    }
}
#pragma warning restore
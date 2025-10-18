using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using RD.Controls;
using RD.Core.Helpers;
using RD.Core.Models;
using RD.Localization;
using RD.Services;

namespace RD.ViewModels;
#pragma warning disable
public partial class AddDownloadViewModel : ObservableObject
{
    private readonly IDataPersistenceService _dataPersistenceService;

    [ObservableProperty]
    private string _url = string.Empty;

    [ObservableProperty]
    private string _fileName = string.Empty;

    [ObservableProperty]
    private string _downloadPath = string.Empty;

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

    private readonly ILocalizationService localizationService = LocalizationService.Instance;

    public AddDownloadViewModel(IDataPersistenceService dataPersistenceService)
    {
        _dataPersistenceService = dataPersistenceService;
        _ = InitializeAsync();
    }

    private async Task InitializeAsync()
    {
        try
        {
            var options = await _dataPersistenceService.LoadOptionsAsync();
            DownloadPath = options.DefaultDownloadDirectory;
        }
        catch
        {
            DownloadPath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
        }
        
        CopyFromClipboard();
    }

    [RelayCommand]
    private void BrowseDownloadPath()
    {
        var dialog = new OpenFolderDialog
        {
            Title = localizationService.GetString(MessageUtils.SelectDownloadLocation)
        };

        if (dialog.ShowDialog() == true)
        {
            DownloadPath = System.IO.Path.GetDirectoryName(dialog.FolderName) ?? DownloadPath;
        }
    }

    [RelayCommand]
    private void AddDownload()
    {
        if (string.IsNullOrWhiteSpace(Url))
        {
            CustomMessageBox.Show(localizationService.GetString(MessageUtils.InvalidUrl), localizationService.GetString(MessageUtils.ValidationError), MessageBoxType.Error);
            return;
        }

        if (string.IsNullOrWhiteSpace(FileName))
        {
            CustomMessageBox.Show(localizationService.GetString(MessageUtils.EnterFileName), localizationService.GetString(MessageUtils.ValidationError),MessageBoxType.Error);
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
            CustomMessageBox.Show($"{localizationService.GetString(MessageUtils.ErrorCreatingDownload)} {ex.Message}", localizationService.GetString(MessageUtils.Error),MessageBoxType.Error);
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

    private void CopyFromClipboard()
    {
        var clipboard = System.Windows.Clipboard.GetText();
        if (Helper.IsValidUrl(clipboard))
        {
            var url = Helper.ExtractUrls(clipboard).FirstOrDefault();
            Url = url;
            OnUrlChanged(url);
        }
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
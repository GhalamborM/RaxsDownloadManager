using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Text.Json;

namespace RD.Services;

public class LocalizationService : ILocalizationService, INotifyPropertyChanged
{
    private static LocalizationService? _instance;
    private readonly Dictionary<string, JsonElement> _translations;
    private CultureInfo _currentCulture;
    private readonly string _resourcesPath;
    private readonly JsonSerializerOptions _serializerOptions = new()
    {
        WriteIndented = true,
        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };
    public static LocalizationService Instance => _instance ??= new LocalizationService();

    public event PropertyChangedEventHandler? PropertyChanged;
    public event EventHandler? LanguageChanged;

    private LocalizationService()
    {
        _translations = [];
        _currentCulture = CultureInfo.CurrentCulture;
        _resourcesPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Languages");

        System.IO.Directory.CreateDirectory(_resourcesPath);

        LoadTranslations();
    }

    public CultureInfo CurrentCulture
    {
        get => _currentCulture;
        private set
        {
            if (_currentCulture != value)
            {
                _currentCulture = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CurrentCulture)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Item[]"));
                LanguageChanged?.Invoke(this, EventArgs.Empty);
            }
        }
    }

    public string GetString(string key)
    {
        try
        {
            var languageCode = _currentCulture.Name;

            // "en-US"
            if (_translations.TryGetValue(languageCode, out var translations))
            {
                var value = GetNestedValue(translations, key);
                if (!string.IsNullOrEmpty(value))
                    return value;
            }

            // "en"
            var languageOnly = _currentCulture.TwoLetterISOLanguageName;
            if (_translations.TryGetValue(languageOnly, out translations))
            {
                var value = GetNestedValue(translations, key);
                if (!string.IsNullOrEmpty(value))
                    return value;
            }

            // Fallback to English
            if (_translations.TryGetValue("en", out translations))
            {
                var value = GetNestedValue(translations, key);
                if (!string.IsNullOrEmpty(value))
                    return value;
            }

            // Return key in brackets if not found
            return $"[{key}]";
        }
        catch
        {
            return $"[{key}]";
        }
    }

    private string GetNestedValue(JsonElement element, string key)
    {
        try
        {
            var keyParts = key.Split('.');
            var current = element;

            foreach (var part in keyParts)
            {
                if (current.ValueKind == JsonValueKind.Object && current.TryGetProperty(part, out var property))
                {
                    current = property;
                }
                else
                {
                    return string.Empty;
                }
            }

            return current.ValueKind == JsonValueKind.String ? current.GetString() ?? string.Empty : string.Empty;
        }
        catch
        {
            return string.Empty;
        }
    }

    public void SetLanguage(string languageCode)
    {
        try
        {
            var culture = new CultureInfo(languageCode);
            CurrentCulture = culture;

            // Set the UI culture for the current thread
            Thread.CurrentThread.CurrentUICulture = culture;
            Thread.CurrentThread.CurrentCulture = culture;

            // Set the default culture for new threads
            CultureInfo.DefaultThreadCurrentCulture = culture;
            CultureInfo.DefaultThreadCurrentUICulture = culture;
        }
        catch (CultureNotFoundException)
        {
            SetLanguage("en");
        }
    }

    public void AddTranslation(string languageCode, string key, string value)
    {
        var filePath = System.IO.Path.Combine(_resourcesPath, $"{languageCode}.json");

        try
        {
            JsonElement existingTranslations = default;

            if (System.IO.File.Exists(filePath))
            {
                var existingContent = System.IO.File.ReadAllText(filePath);
                if (!string.IsNullOrWhiteSpace(existingContent))
                {
                    using var doc = JsonDocument.Parse(existingContent);
                    existingTranslations = doc.RootElement.Clone();
                }
            }

            var translationsDict = JsonElementToDictionary(existingTranslations);

            SetNestedValue(translationsDict, key, value);

            var jsonContent = JsonSerializer.Serialize(translationsDict, _serializerOptions);

            System.IO.File.WriteAllText(filePath, jsonContent);

            ReloadTranslations();
        }
        catch
        {
        }
    }

    private Dictionary<string, object> JsonElementToDictionary(JsonElement element)
    {
        var result = new Dictionary<string, object>();

        if (element.ValueKind == JsonValueKind.Object)
        {
            foreach (var property in element.EnumerateObject())
            {
                if (property.Value.ValueKind == JsonValueKind.Object)
                {
                    result[property.Name] = JsonElementToDictionary(property.Value);
                }
                else if (property.Value.ValueKind == JsonValueKind.String)
                {
                    result[property.Name] = property.Value.GetString() ?? string.Empty;
                }
                else if (property.Value.ValueKind == JsonValueKind.Number)
                {
                    result[property.Name] = property.Value.GetDouble();
                }
                else if (property.Value.ValueKind == JsonValueKind.True || property.Value.ValueKind == JsonValueKind.False)
                {
                    result[property.Name] = property.Value.GetBoolean();
                }
            }
        }

        return result;
    }

    private void SetNestedValue(Dictionary<string, object> dict, string key, string value)
    {
        var keyParts = key.Split('.');
        var current = dict;

        for (int i = 0; i < keyParts.Length - 1; i++)
        {
            var part = keyParts[i];
            if (!current.TryGetValue(part, out object? value1))
            {
                value1 = new Dictionary<string, object>();
                current[part] = value1;
            }

            if (value1 is Dictionary<string, object> nestedDict)
            {
                current = nestedDict;
            }
            else
            {
                current[part] = new Dictionary<string, object>();
                current = (Dictionary<string, object>)current[part];
            }
        }

        current[keyParts[^1]] = value;
    }

    public IEnumerable<string> GetAvailableLanguages()
    {
        return _translations.Keys;
    }

    public void ReloadTranslations()
    {
        _translations.Clear();
        LoadTranslations();

        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Item[]"));
    }

    private void LoadTranslations()
    {
        try
        {
            if (!System.IO.Directory.Exists(_resourcesPath))
            {
                Debug.WriteLine("No localization resources directory found.");
                return;
            }

            var jsonFiles = System.IO.Directory.GetFiles(_resourcesPath, "*.json");

            if (jsonFiles.Length == 0)
            {
                Debug.WriteLine("No localization resources directory found.");
                return;
            }

            foreach (var file in jsonFiles)
            {
                try
                {
                    var fileName = System.IO.Path.GetFileNameWithoutExtension(file);
                    var jsonContent = System.IO.File.ReadAllText(file);

                    if (!string.IsNullOrEmpty(jsonContent))
                    {
                        using var doc = JsonDocument.Parse(jsonContent, new JsonDocumentOptions { AllowTrailingCommas = true });
                        _translations[fileName] = doc.RootElement.Clone();
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"{ex}");
                    continue;
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"No localization resources directory found.\n{ex}");
        }

        if (!_translations.ContainsKey("en"))
        {
            Debug.WriteLine("No localization resources directory found.");
        }
    }

    public string this[string key] => GetString(key);
}

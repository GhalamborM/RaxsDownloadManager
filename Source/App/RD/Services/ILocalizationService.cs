using System.Globalization;

namespace RD.Services;

public interface ILocalizationService
{
    string GetString(string key);
    void SetLanguage(string languageCode);
    CultureInfo CurrentCulture { get; }
    event EventHandler LanguageChanged;
    void AddTranslation(string languageCode, string key, string value);
    IEnumerable<string> GetAvailableLanguages();
    void ReloadTranslations();
}

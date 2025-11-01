namespace PEAKLib.UI;

/// <summary>
/// A translation key, created with <see cref="MenuAPI.CreateLocalization(string)"/> that can be used with <see cref="ElementExtensions.SetLocalizationIndex{T}(T, TranslationKey)"/>
/// </summary>
/// <param name="Index"></param>
public record TranslationKey(string Index)
{
    /// <summary>
    /// <inheritdoc cref="ElementExtensions.AddLocalization{T}(T, string, LocalizedText.Language)"/>
    /// </summary>
    /// <param name="text"></param>
    /// <param name="language"></param>
    /// <returns></returns>
    public TranslationKey AddLocalization(string text, LocalizedText.Language language)
    {
        MenuAPI.CreateLocalizationInternal(Index, text, language);

        return this;
    }
}

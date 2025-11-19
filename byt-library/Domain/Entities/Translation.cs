namespace byt_library.Domain.Entities;

public class Translation
{
    public string Link { get; set; }
    public string Language { get; set; }

    private static List<Translation> _allTranslations = new();
    private static readonly object _lockTranslation = new();

    public static void AddTranslation(Translation translation)
    {
        if (translation == null)
            throw new TranslationIsNullException(nameof(translation), "Cannot add null translation to extent");

        lock (_lockTranslation)
        {
            if (string.IsNullOrWhiteSpace(translation.Link))
                throw new ArgumentException("Link cannot be empty");

            if (_allTranslations.Any(t => t.Id == translation.Id))
                throw new TranslationAlreadyExistsException($"Translation with ID {translation.Id} already exists in extent");

            if (_allTranslations.Any(t => t.Link.Equals(translation.Link, StringComparison.OrdinalIgnoreCase) &&
                                          t.Language.Equals(translation.Language, StringComparison.OrdinalIgnoreCase)))
                throw new InvalidOperationException($"Translation with Link '{translation.Link}' and Language '{translation.Language}' already exists in extent");

            _allTranslations.Add(translation);
        }
    }

    public static bool RemoveTranslation(string link, string language)
    {
        lock (_lockTranslation)
        {
            var translation = _allTranslations.FirstOrDefault(t => t.Link.Equals(link, StringComparison.OrdinalIgnoreCase) &&
                                                                    t.Language.Equals(language, StringComparison.OrdinalIgnoreCase));
            if (translation != null)
            {
                return _allTranslations.Remove(translation);
            }
            return false;
        }
    }

    public static Translation? GetTranslation(string link, string language)
    {
        lock (_lockTranslation)
        {
            return _allTranslations.FirstOrDefault(t => t.Link.Equals(link, StringComparison.OrdinalIgnoreCase) &&
                                                        t.Language.Equals(language, StringComparison.OrdinalIgnoreCase));
        }
    }

    public static IReadOnlyList<Translation> GetAllTranslations()
    {
        lock (_lockTranslation)
        {
            return _allTranslations.AsReadOnly();
        }
    }

    public static void ClearTranslationExtent()
    {
        lock (_lockTranslation)
        {
            _allTranslations.Clear();
        }
    }
}
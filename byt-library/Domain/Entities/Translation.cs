using byt_library.Domain.Services;

namespace byt_library.Domain.Entities;

public class Translation
{
    public string Link { get; set; }
    public string Language { get; set; }

    private static List<Translation> _allTranslations = new();
    private static readonly object _lockTranslation = new();
    private static readonly JsonPersistenceService _persistenceService = new("data");
    public static readonly List<string> _supportedLanguages = ["polish", "english", "ukrainian"];

    static Translation()
    {
        try
        {
            var loadedItems = _persistenceService.Load<Translation>();
            lock (_lockTranslation)
            {
                _allTranslations = loadedItems;
            }
        }
        catch (FileNotFoundException)
        {
            _allTranslations = new List<Translation>();
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Failed to load {typeof(Translation).Name}: {ex.Message}");
            _allTranslations = new List<Translation>();
        }
    }

    public Translation() { }

    public Translation(string link, string language)
    {
        Link = link;
        Language = language;
        AddTranslation(this);
    }

    private static void AddTranslation(Translation translation)
    {
        if (translation == null)
            throw new TranslationIsNullException(nameof(translation), "Cannot add null translation to extent");

        lock (_lockTranslation)
        {
            if (string.IsNullOrWhiteSpace(translation.Link))
                throw new LinkIsEmptyException("Link cannot be empty");

            if (string.IsNullOrWhiteSpace(translation.Language))
                throw new TranslationAlreadyExistsException("Language cannot be empty");

            if (_allTranslations.Any(t => t.Link.Equals(translation.Link, StringComparison.OrdinalIgnoreCase) &&
                                          t.Language.Equals(translation.Language, StringComparison.OrdinalIgnoreCase)))
                throw new InvalidOperationException($"Translation with Link '{translation.Link}' and Language '{translation.Language}' already exists in extent");

            _allTranslations.Add(translation);

            try
            {
                _persistenceService.Save(_allTranslations);
            }
            catch (Exception ex)
            {
                _allTranslations.Remove(translation);
                throw new InvalidOperationException("Failed to persist Translation to file", ex);
            }
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
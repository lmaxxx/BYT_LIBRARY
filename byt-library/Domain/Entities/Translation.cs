using byt_library.Domain.Services;
using byt_library.Domain.Exceptions;
using byt_library.Domain.Interfaces;
using System.Text.Json.Serialization;

namespace byt_library.Domain.Entities;

public class Translation
{
    public string Link { get; set; }
    public string Language { get; set; }

    // Owner tracking fields
    private string? _ownerId;  // ISBN for Book, PageLink for OnlineMagazine
    private IDigitalResource? _ownerCache;  // Resolved reference

    // Property for JSON serialization
    public string? OwnerId
    {
        get => _ownerId;
        set => _ownerId = value;
    }

    // Owner property with lazy resolution
    [JsonIgnore]
    public IDigitalResource? Owner
    {
        get
        {
            if (_ownerCache == null && !string.IsNullOrEmpty(_ownerId))
            {
                // Try Book first
                _ownerCache = Book.GetBookByIsbn(_ownerId) as IDigitalResource;
                // Try OnlineMagazine if not found
                if (_ownerCache == null)
                    _ownerCache = OnlineMagazine.GetOnlineMagazineByPageLink(_ownerId) as IDigitalResource;
            }
            return _ownerCache;
        }
        private set
        {
            _ownerCache = value;
            _ownerId = value switch
            {
                Book b => b.ISBN,
                OnlineMagazine om => om.PageLink,
                _ => null
            };
        }
    }

    private static List<Translation> _allTranslations = new();
    internal static readonly object _lockTranslation = new();
    private static readonly JsonPersistenceService _persistenceService = new("data");
    public static readonly List<string> _supportedLanguages = ["polish", "english", "ukrainian"];

    static Translation()
    {
        try
        {
            var loadedItems = _persistenceService.Load<Translation>();

            // STRICT VALIDATION: Ensure all translations have valid owners
            ValidateTranslationsHaveOwners(loadedItems);

            lock (_lockTranslation)
            {
                _allTranslations = loadedItems;
            }
        }
        catch (FileNotFoundException)
        {
            _allTranslations = new List<Translation>();
        }
        catch (CompositionConstraintViolationException)
        {
            // Re-throw composition violations to fail fast
            throw;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Failed to load {typeof(Translation).Name}: {ex.Message}");
            _allTranslations = new List<Translation>();
        }
    }

    // Make parameterless constructor internal (needed for JSON deserialization)
    internal Translation() { }

    // Make parameterized constructor private (prevent direct instantiation)
    private Translation(string link, string language)
    {
        if (!_supportedLanguages.Contains(language.ToLower()))
            throw new UnsupportedLanguageException($"Language '{language}' is not supported.");

        Link = link;
        Language = language;
        AddTranslation(this);
    }

    // Factory method for creating translations with owner
    internal static Translation CreateFor(IDigitalResource owner, string language, string link)
    {
        if (owner == null)
            throw new TranslationOwnerIsNullException("Translation must have an owner");

        if (!_supportedLanguages.Contains(language.ToLower()))
            throw new UnsupportedLanguageException($"Language '{language}' is not supported.");

        var translation = new Translation
        {
            Link = link,
            Language = language,
            Owner = owner  // Sets both _ownerCache and _ownerId
        };

        AddTranslation(translation);
        return translation;
    }

    // Validation method for loaded translations
    private static void ValidateTranslationsHaveOwners(List<Translation> translations)
    {
        foreach (var translation in translations)
        {
            if (string.IsNullOrEmpty(translation.OwnerId))
            {
                throw new CompositionConstraintViolationException(
                    $"Translation with Link '{translation.Link}' and Language '{translation.Language}' " +
                    "has no owner. Composition constraint violated.");
            }

            // Verify owner exists
            if (translation.Owner == null)
            {
                throw new CompositionConstraintViolationException(
                    $"Translation with Link '{translation.Link}' and Language '{translation.Language}' " +
                    $"references non-existent owner '{translation.OwnerId}'. Composition constraint violated.");
            }
        }
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
                throw new LanguageIsEmptyException("Language cannot be empty");

            // Validate owner exists (composition constraint)
            if (translation.Owner == null && !string.IsNullOrEmpty(translation.OwnerId))
                throw new TranslationOwnerIsNullException("Translation must have a valid owner");

            if (_allTranslations.Any(t => t.Link.Equals(translation.Link, StringComparison.OrdinalIgnoreCase) &&
                                          t.Language.Equals(translation.Language, StringComparison.OrdinalIgnoreCase)))
                throw new TranslationAlreadyExistsException($"Translation with Link '{translation.Link}' and Language '{translation.Language}' already exists in extent");

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

    // Bulk removal method for cascading delete
    internal static void RemoveTranslationsByOwner(IDigitalResource owner)
    {
        if (owner == null) return;

        string? ownerId = owner switch
        {
            Book b => b.ISBN,
            OnlineMagazine om => om.PageLink,
            _ => null
        };

        if (string.IsNullOrEmpty(ownerId)) return;

        lock (_lockTranslation)
        {
            var toRemove = _allTranslations
                .Where(t => t.OwnerId?.Equals(ownerId, StringComparison.OrdinalIgnoreCase) == true)
                .ToList();

            foreach (var translation in toRemove)
            {
                _allTranslations.Remove(translation);
            }

            if (toRemove.Any())
            {
                _persistenceService.Save(_allTranslations);
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
            _persistenceService.Save(_allTranslations);
        }
    }
}
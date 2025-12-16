using System.Text.Json;
using byt_library.Domain.Services;
using byt_library.Domain.Exceptions;
using byt_library.Domain.Interfaces;
using System.Text.Json.Serialization;

namespace byt_library.Domain.Entities;

public class Translation
{
    public string Link { get; set; }
    public string Language { get; set; }
    
    [JsonInclude]
    private readonly IDigitalResource _owner;
    
    private static List<Translation> _allTranslations = new();
    public static readonly object _lockTranslation = new();
    private static readonly JsonPersistenceService _persistenceService = new("data");
    
    private static readonly List<string> _supportedLanguages = ["polish", "english", "ukrainian"];
    
    public IDigitalResource GetOwner() => _owner;
    
    static Translation()
    {
        try
        {
            var loadedItems = _persistenceService.Load<Translation>();

            // Ensure all translations have valid owners
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
            Console.Error.WriteLine("Composition constraint violation detected");
            throw;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Failed to load {typeof(Translation).Name}: {ex.Message}");
            _allTranslations = new List<Translation>();
        }
    }
    
    [JsonConstructor]
    public Translation(string link, string language, IDigitalResource _owner) { 
        if (!_supportedLanguages.Contains(language.ToLower()))
            throw new UnsupportedLanguageException($"Language '{language}' is not supported.");

        if (_owner == null)
            throw new TranslationOwnerIsNullException("Translation must have an owner");

        Link = link;
        Language = language;
        this._owner = _owner;
        AddTranslation(this);
    }

    // Validation method for loaded translations
    private static void ValidateTranslationsHaveOwners(List<Translation> translations)
    {
        foreach (var translation in translations)
        {
            if (translation._owner == null)
            {
                throw new CompositionConstraintViolationException(
                    $"Translation with Link '{translation.Link}' and Language '{translation.Language}' " +
                    "has no owner. Composition constraint violated.");
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
            if (translation._owner == null)
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
    public static void RemoveTranslationsByOwner(IDigitalResource owner)
    {
        if (owner == null) return;

        lock (_lockTranslation)
        {
            foreach (var translation in _allTranslations
                         .Where(t => t._owner == owner)
                         .ToList())
            {
                _allTranslations.Remove(translation);
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
            return _allTranslations.FirstOrDefault(t => t.Link.Equals(link, StringComparison.OrdinalIgnoreCase) && t.Language.Equals(language, StringComparison.OrdinalIgnoreCase));
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
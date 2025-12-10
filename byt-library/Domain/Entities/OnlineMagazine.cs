using System.Text.Json.Serialization;
using byt_library.Domain.Interfaces;
using byt_library.Domain.Services;
using byt_library.Domain.Exceptions;

namespace byt_library.Domain.Entities;

public class OnlineMagazine : IDigitalResource
{
    public string PageLink { get; set; }
    public bool HasAudio { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public int Size { get; set; }
    public string Link { get; set; }
    [JsonInclude]
    private readonly HashSet<Translation> _translations = new();

    private static List<OnlineMagazine> _allOnlineMagazines = new();
    private static readonly object _lockOnlineMagazine = new();
    private static readonly JsonPersistenceService _persistenceService = new("data");

    static OnlineMagazine()
    {
        try
        {
            var loadedItems = _persistenceService.Load<OnlineMagazine>();
            lock (_lockOnlineMagazine)
            {
                _allOnlineMagazines = loadedItems;
            }
        }
        catch (FileNotFoundException)
        {
            _allOnlineMagazines = new List<OnlineMagazine>();
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Failed to load {typeof(OnlineMagazine).Name}: {ex.Message}");
            _allOnlineMagazines = new List<OnlineMagazine>();
        }
    }

    public OnlineMagazine()
    {
        _translations = new HashSet<Translation>();
    }

    [JsonConstructor]
    public OnlineMagazine(string pageLink, string title, string description,
                          bool hasAudio = false, int size = 0, string link = "", HashSet<Translation>? _translations = null)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new TitleIsEmptyException("Online Magazine title cannot be empty");
        
        if (string.IsNullOrWhiteSpace(description))
            throw new DescriptionIsEmptyException();
        
        PageLink = pageLink;
        Title = title;
        Description = description;
        HasAudio = hasAudio;
        Size = size;
        Link = link;
        this._translations = _translations ?? new HashSet<Translation>();
        AddOnlineMagazine(this);
    }

    private static void AddOnlineMagazine(OnlineMagazine onlineMagazine)
    {
        if (onlineMagazine == null)
            throw new OnlineMagazieIsNullException(nameof(onlineMagazine), "Cannot add null online magazine to extent");

        lock (_lockOnlineMagazine)
        {
            if (string.IsNullOrWhiteSpace(onlineMagazine.PageLink))
                throw new PageLinkIsEmptyException("PageLink cannot be empty");

            if (_allOnlineMagazines.Any(om => om.PageLink.Equals(onlineMagazine.PageLink, StringComparison.OrdinalIgnoreCase)))
                throw new OnlineMagazineAlreadyExistsException($"OnlineMagazine with PageLink {onlineMagazine.PageLink} already exists in extent");

            _allOnlineMagazines.Add(onlineMagazine);

            try
            {
                _persistenceService.Save(_allOnlineMagazines);
            }
            catch (Exception ex)
            {
                _allOnlineMagazines.Remove(onlineMagazine);
                throw new InvalidOperationException("Failed to persist OnlineMagazine to file", ex);
            }
        }
    }

    public static bool RemoveOnlineMagazine(string pageLink)
    {
        lock (Translation._lockTranslation)  // Lock order: Translation first to prevent deadlock
        {
            lock (_lockOnlineMagazine)
            {
                var onlineMagazine = _allOnlineMagazines.FirstOrDefault(om =>
                    om.PageLink.Equals(pageLink, StringComparison.OrdinalIgnoreCase));

                if (onlineMagazine != null)
                {
                    // Cascade delete: remove all translations for this magazine
                    Translation.RemoveTranslationsByOwner(onlineMagazine);
                    return _allOnlineMagazines.Remove(onlineMagazine);
                }
                return false;
            }
        }
    }

    public static OnlineMagazine? GetOnlineMagazineByPageLink(string pageLink)
    {
        lock (_lockOnlineMagazine)
        {
            return _allOnlineMagazines.FirstOrDefault(om => om.PageLink.Equals(pageLink, StringComparison.OrdinalIgnoreCase));
        }
    }

    public static IReadOnlyList<OnlineMagazine> GetAllOnlineMagazines()
    {
        lock (_lockOnlineMagazine)
        {
            return _allOnlineMagazines.AsReadOnly();
        }
    }

    public void AddTranslation(string language)
    {
        // Ensure there is no other translation with such a language already
        if (_translations.FirstOrDefault(translation =>
                translation.Language.Equals(language, StringComparison.OrdinalIgnoreCase)) != null)
        {
            throw new TranslationAlreadyExistsException(language);
        }
        // Translation automatically adds owner. Adding it to hashset ensures both objects have each other and no other can access this relationship.
        _translations.Add(new Translation($"https://some.storage/online-magazines/{PageLink}/{language}", language,
            PageLink));
    }

    public bool RemoveTranslation(string language)
    {
        // Remove translation from class extent
        Translation? translation = _translations.FirstOrDefault(translation =>
            translation.Language.Equals(language, StringComparison.OrdinalIgnoreCase));

        if (translation == null) return false;
        
        if (Translation.RemoveTranslation(translation.Link, translation.Language))
        {
            _translations.RemoveWhere(t => t.Language.Equals(language, StringComparison.OrdinalIgnoreCase));
            return true;
        }

        return false;
    }

    public static void ClearOnlineMagazineExtent()
    {
        lock (_lockOnlineMagazine)
        {
            while (_allOnlineMagazines.Count != 0) RemoveOnlineMagazine(_allOnlineMagazines.Last().PageLink);  // Properly removes all the magazines, including translations.
            _persistenceService.Save(_allOnlineMagazines);
        }
    }
}
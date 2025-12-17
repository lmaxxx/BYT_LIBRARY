using System.Text.Json.Serialization;
using byt_library.Domain.Interfaces;
using byt_library.Domain.Services;
using byt_library.Domain.Exceptions;

namespace byt_library.Domain.Entities;

public class OnlineMagazine : DigitalResource
{
    public string PageLink { get; set; }
    public bool HasAudio { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public int Size { get; set; }
    public string Link { get; set; }

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

    [JsonConstructor]
    public OnlineMagazine(Resource resource, string pageLink, string title, string description,
                          bool hasAudio = false, int size = 0, string link = "", HashSet<Translation>? _translations = null) : base(resource, size, link)
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

    public static void ClearOnlineMagazineExtent()
    {
        lock (_lockOnlineMagazine)
        {
            while (_allOnlineMagazines.Count != 0) RemoveOnlineMagazine(_allOnlineMagazines.Last().PageLink);  // Properly removes all the magazines, including translations.
            _persistenceService.Save(_allOnlineMagazines);
        }
    }
}
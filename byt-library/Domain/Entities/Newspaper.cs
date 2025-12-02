using byt_library.Domain.Enums;
using byt_library.Domain.Interfaces;
using byt_library.Domain.Services;

namespace byt_library.Domain.Entities;

public class Newspaper : IPrintedResource
{
    public string Publisher { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public CoverType CoverType { get; set; }
    public int Quantity { get; set; }

    private static List<Newspaper> _allNewspapers = new();
    private static readonly object _lockNewspaper = new();
    private static readonly JsonPersistenceService _persistenceService = new("data");

    static Newspaper()
    {
        try
        {
            var loadedItems = _persistenceService.Load<Newspaper>();
            lock (_lockNewspaper)
            {
                _allNewspapers = loadedItems;
            }
        }
        catch (FileNotFoundException)
        {
            _allNewspapers = new List<Newspaper>();
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Failed to load {typeof(Newspaper).Name}: {ex.Message}");
            _allNewspapers = new List<Newspaper>();
        }
    }

    public Newspaper() { }

    public Newspaper(string publisher, string title, string description,
                     CoverType coverType = CoverType.Soft, int quantity = 1)
    {
        Publisher = publisher;
        Title = title;
        Description = description;
        CoverType = coverType;
        Quantity = quantity;
        AddNewspaper(this);
    }

    private static void AddNewspaper(Newspaper newspaper)
    {
        if (newspaper == null)
            throw new NewspaperIsNullException(nameof(newspaper), "Cannot add null newspaper to extent");

        lock (_lockNewspaper)
        {
            if (string.IsNullOrWhiteSpace(newspaper.Title))
                throw new TitleIsEmptyException("Title cannot be empty");

            if (string.IsNullOrWhiteSpace(newspaper.Publisher))
                throw new PublisherIsEmptyException("Publisher cannot be empty");

            if (_allNewspapers.Any(n => n.Title.Equals(newspaper.Title, StringComparison.OrdinalIgnoreCase) &&
                                         n.Publisher.Equals(newspaper.Publisher, StringComparison.OrdinalIgnoreCase)))
                throw new NewspaperAlreadyExistsException($"Newspaper with Title '{newspaper.Title}' and Publisher '{newspaper.Publisher}' already exists in extent");

            _allNewspapers.Add(newspaper);

            try
            {
                _persistenceService.Save(_allNewspapers);
            }
            catch (Exception ex)
            {
                _allNewspapers.Remove(newspaper);
                throw new InvalidOperationException("Failed to persist Newspaper to file", ex);
            }
        }
    }

    public static bool RemoveNewspaper(string title, string publisher)
    {
        lock (_lockNewspaper)
        {
            var newspaper = _allNewspapers.FirstOrDefault(n => n.Title.Equals(title, StringComparison.OrdinalIgnoreCase) &&
                                                                n.Publisher.Equals(publisher, StringComparison.OrdinalIgnoreCase));
            if (newspaper != null)
            {
                return _allNewspapers.Remove(newspaper);
            }
            return false;
        }
    }

    public static Newspaper? GetNewspaper(string title, string publisher)
    {
        lock (_lockNewspaper)
        {
            return _allNewspapers.FirstOrDefault(n => n.Title.Equals(title, StringComparison.OrdinalIgnoreCase) &&
                                                      n.Publisher.Equals(publisher, StringComparison.OrdinalIgnoreCase));
        }
    }

    public static IReadOnlyList<Newspaper> GetAllNewspapers()
    {
        lock (_lockNewspaper)
        {
            return _allNewspapers.AsReadOnly();
        }
    }

    public static void ClearNewspaperExtent()
    {
        lock (_lockNewspaper)
        {
            _allNewspapers.Clear();
            _persistenceService.Save(_allNewspapers);
        }
    }
}
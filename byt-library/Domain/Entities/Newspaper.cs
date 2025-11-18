using byt_library.Domain.Enums;
using byt_library.Domain.Interfaces;

namespace byt_library.Domain.Entities;

public class Newspaper : IPrintedResource
{
    public string Publisher { get; set; }
    public required string Title { get; set; }
    public required string Description { get; set; }
    public required CoverType CoverType { get; set; }
    public int Quantity { get; set; }

    private static List<Newspaper> _allNewspapers = new();
    private static readonly object _lockNewspaper = new();

    public static void AddNewspaper(Newspaper newspaper)
    {
        if (newspaper == null)
            throw new ArgumentNullException(nameof(newspaper), "Cannot add null newspaper to extent");

        lock (_lockNewspaper)
        {
            if (string.IsNullOrWhiteSpace(newspaper.Title))
                throw new ArgumentException("Title cannot be empty");

            if (string.IsNullOrWhiteSpace(newspaper.Publisher))
                throw new ArgumentException("Publisher cannot be empty");

            if (_allNewspapers.Any(n => n.Title.Equals(newspaper.Title, StringComparison.OrdinalIgnoreCase) &&
                                         n.Publisher.Equals(newspaper.Publisher, StringComparison.OrdinalIgnoreCase)))
                throw new InvalidOperationException($"Newspaper with Title '{newspaper.Title}' and Publisher '{newspaper.Publisher}' already exists in extent");

            _allNewspapers.Add(newspaper);
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
        }
    }
}
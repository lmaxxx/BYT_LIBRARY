using System.Text.Json;

namespace byt_library.Domain.Entities;

public class Newspaper
{
    public int Id { get; private set; }
    public string Publisher { get; set; }

    private static List<Newspaper> _allNewspapers = new();
    private static int _nextId = 1;
    private static readonly object _lockNewspaper = new();

    public static void AddNewspaper(Newspaper newspaper)
    {
        if (newspaper == null)
            throw new ArgumentNullException(nameof(newspaper), "Cannot add null newspaper to extent");

        lock (_lockNewspaper)
        {
            if (newspaper.Id == 0)
            {
                newspaper.Id = _nextId++;
            }

            if (_allNewspapers.Any(n => n.Id == newspaper.Id))
                throw new InvalidOperationException($"Newspaper with ID {newspaper.Id} already exists in extent");

            _allNewspapers.Add(newspaper);
        }
    }

    public static bool RemoveNewspaper(int id)
    {
        lock (_lockNewspaper)
        {
            var newspaper = _allNewspapers.FirstOrDefault(n => n.Id == id);
            if (newspaper != null)
            {
                return _allNewspapers.Remove(newspaper);
            }
            return false;
        }
    }

    public static Newspaper? GetNewspaperById(int id)
    {
        lock (_lockNewspaper)
        {
            return _allNewspapers.FirstOrDefault(n => n.Id == id);
        }
    }

    public static IReadOnlyList<Newspaper> GetAllNewspapers()
    {
        lock (_lockNewspaper)
        {
            return _allNewspapers.AsReadOnly();
        }
    }

    public static int GetNewspaperCount()
    {
        lock (_lockNewspaper)
        {
            return _allNewspapers.Count;
        }
    }

    public static void ClearNewspaperExtent()
    {
        lock (_lockNewspaper)
        {
            _allNewspapers.Clear();
            _nextId = 1;
        }
    }

    public static void SaveNewspapersToFile(string filePath)
    {
        lock (_lockNewspaper)
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNameCaseInsensitive = true
            };

            var json = JsonSerializer.Serialize(_allNewspapers, options);
            File.WriteAllText(filePath, json);
        }
    }

    public static void LoadNewspapersFromFile(string filePath)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException($"File not found: {filePath}");

        lock (_lockNewspaper)
        {
            var json = File.ReadAllText(filePath);
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var newspaperList = JsonSerializer.Deserialize<List<Newspaper>>(json, options);

            if (newspaperList != null)
            {
                ClearNewspaperExtent();

                foreach (var newspaper in newspaperList)
                {
                    AddNewspaper(newspaper);
                }
            }
        }
    }
}
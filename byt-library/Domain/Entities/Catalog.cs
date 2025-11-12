using byt_library.Domain.Interfaces;
using System.Text.Json;

namespace byt_library.Domain.Entities;

public class Catalog
{
    public int Id { get; private set; }
    public string Name { get; set; }
    public Dictionary<string, IResource> resources { get; }

    private static List<Catalog> _allCatalogs = new();
    private static int _nextId = 1;
    private static readonly object _lockCatalog = new();

    public Catalog()
    {
        resources = new Dictionary<string, IResource>();
    }

    public Catalog(string name)
    {
        Name = name;
        resources = new Dictionary<string, IResource>();
    }

    public void AddResource(IResource resource)
    {
        resources.Add(resource.Title, resource);
    }

    public static void AddCatalog(Catalog catalog)
    {
        if (catalog == null)
            throw new ArgumentNullException(nameof(catalog), "Cannot add null catalog to extent");

        lock (_lockCatalog)
        {
            if (catalog.Id == 0)
            {
                catalog.Id = _nextId++;
            }

            if (_allCatalogs.Any(c => c.Id == catalog.Id))
                throw new InvalidOperationException($"Catalog with ID {catalog.Id} already exists in extent");

            _allCatalogs.Add(catalog);
        }
    }

    public static bool RemoveCatalog(int id)
    {
        lock (_lockCatalog)
        {
            var catalog = _allCatalogs.FirstOrDefault(c => c.Id == id);
            if (catalog != null)
            {
                return _allCatalogs.Remove(catalog);
            }
            return false;
        }
    }

    public static Catalog? GetCatalogById(int id)
    {
        lock (_lockCatalog)
        {
            return _allCatalogs.FirstOrDefault(c => c.Id == id);
        }
    }

    public static IReadOnlyList<Catalog> GetAllCatalogs()
    {
        lock (_lockCatalog)
        {
            return _allCatalogs.AsReadOnly();
        }
    }

    public static int GetCatalogCount()
    {
        lock (_lockCatalog)
        {
            return _allCatalogs.Count;
        }
    }

    public static void ClearCatalogExtent()
    {
        lock (_lockCatalog)
        {
            _allCatalogs.Clear();
            _nextId = 1;
        }
    }

    public static void SaveCatalogsToFile(string filePath)
    {
        lock (_lockCatalog)
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNameCaseInsensitive = true
            };

            var json = JsonSerializer.Serialize(_allCatalogs, options);
            File.WriteAllText(filePath, json);
        }
    }

    public static void LoadCatalogsFromFile(string filePath)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException($"File not found: {filePath}");

        lock (_lockCatalog)
        {
            var json = File.ReadAllText(filePath);
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var catalogList = JsonSerializer.Deserialize<List<Catalog>>(json, options);

            if (catalogList != null)
            {
                ClearCatalogExtent();

                foreach (var catalog in catalogList)
                {
                    AddCatalog(catalog);
                }
            }
        }
    }
}
using byt_library.Domain.Interfaces;

namespace byt_library.Domain.Entities;

public class Catalog
{
    public string Name { get; set; }
    public Dictionary<string, IResource> resources { get; }

    private static List<Catalog> _allCatalogs = new();
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
            throw new CatalogIsNullException(nameof(catalog), "Cannot add null catalog to extent");

        lock (_lockCatalog)
        {
            if (string.IsNullOrWhiteSpace(catalog.Name))
                throw new ArgumentException("Catalog name cannot be empty");

<<<<<<< HEAD
            if (_allCatalogs.Any(c => c.Name.Equals(catalog.Name, StringComparison.OrdinalIgnoreCase)))
                throw new InvalidOperationException($"Catalog with name {catalog.Name} already exists in extent");
=======
            if (_allCatalogs.Any(c => c.Id == catalog.Id))
                throw new CatalogAlreadyExistsException($"Catalog with ID {catalog.Id} already exists in extent");
>>>>>>> feat/customexceptions

            _allCatalogs.Add(catalog);
        }
    }

    public static bool RemoveCatalog(string name)
    {
        lock (_lockCatalog)
        {
            var catalog = _allCatalogs.FirstOrDefault(c => c.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
            if (catalog != null)
            {
                return _allCatalogs.Remove(catalog);
            }
            return false;
        }
    }

    public static Catalog? GetCatalogByName(string name)
    {
        lock (_lockCatalog)
        {
            return _allCatalogs.FirstOrDefault(c => c.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        }
    }

    public static IReadOnlyList<Catalog> GetAllCatalogs()
    {
        lock (_lockCatalog)
        {
            return _allCatalogs.AsReadOnly();
        }
    }

    public static void ClearCatalogExtent()
    {
        lock (_lockCatalog)
        {
            _allCatalogs.Clear();
        }
    }
}
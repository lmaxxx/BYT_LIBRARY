using System.Diagnostics;
using System.Text.Json.Serialization;
using byt_library.Domain.Exceptions;
using byt_library.Domain.Interfaces;
using byt_library.Domain.Services;

namespace byt_library.Domain.Entities;

public class Catalog
{
    public string Name { get; set; }
    
    [JsonInclude]
    private Dictionary<string, Resource> _resources { get; }

    private static List<Catalog> _allCatalogs = new();
    private static readonly object _lockCatalog = new();
    private static readonly JsonPersistenceService _persistenceService = new("data");

    static Catalog()
    {
        try
        {
            var loadedItems = _persistenceService.Load<Catalog>();
            lock (_lockCatalog)
            {
                _allCatalogs = loadedItems;
            }
        }
        catch (FileNotFoundException)
        {
            _allCatalogs = new List<Catalog>();
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Failed to load {typeof(Catalog).Name}: {ex.Message}");
            _allCatalogs = new List<Catalog>();
        }
    }

    public Catalog()
    {
        _resources = new Dictionary<string, Resource>();
        AddCatalog(this);
    }

    public Catalog(string name)
    {
        Name = name;
        _resources = new Dictionary<string, Resource>();
        AddCatalog(this);
    }

    public void AddResource(Resource resource)
    {
        if (!_resources.ContainsKey(resource.Title))
        {
            _resources.Add(resource.Title, resource);
            resource.AddCatalog(this);
        }
        else
        {
            throw new ResourceIsAlreadyPresentInTheCatalogException($"Resource with title {resource.Title} already exists in catalog");
        }
    }
    
    public void RemoveResource(Resource resource)
    {
        if (_resources.ContainsKey(resource.Title))
        {
            _resources.Remove(resource.Title);
            if (resource.RemoveCatalog(this))
            {
                throw new CatalogNotFoundInResourceException("Resource has been removed from the catalog");
            }
        }
        else
        {
            throw new ResourceIsNotPresentInTheCatalogException("There is no resource with the title: " +  resource.Title);
        }
    }

    private static void AddCatalog(Catalog catalog)
    {
        if (catalog == null)
            throw new CatalogIsNullException(nameof(catalog), "Cannot add null catalog to extent");

        lock (_lockCatalog)
        {
            if (string.IsNullOrWhiteSpace(catalog.Name))
                throw new CatalogIsEmptyException("Catalog name cannot be empty");

            if (_allCatalogs.Any(c => c.Name.Equals(catalog.Name, StringComparison.OrdinalIgnoreCase)))
                throw new CatalogWithThisNameAlreadyExistsException($"Catalog with name {catalog.Name} already exists in extent");

            _allCatalogs.Add(catalog);

            try
            {
                _persistenceService.Save(_allCatalogs);
            }
            catch (Exception ex)
            {
                _allCatalogs.Remove(catalog);
                throw new InvalidOperationException("Failed to persist Catalog to file", ex);
            }
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
            _persistenceService.Save(_allCatalogs);
        }
    }
}
using System.Text.Json.Serialization;
using byt_library.Domain.Exceptions;
using byt_library.Domain.Interfaces;
using byt_library.Domain.Services;

namespace byt_library.Domain.Entities;

public class Resource
{
    static int MaxBorrowingPeriodDays = 360;
    public string Title { get; set; }
    public string Description { get; set; }
    
    [JsonInclude]
    private IDigitalResource? _digitalResource;
    
    [JsonInclude]
    private IPrintedResource? _printedResource;
    
    // Relations
    private readonly HashSet<BorrowRecord> _borrowRecords = new();
    private readonly HashSet<Catalog> _catalogs = new();  // Hash set as this is aggregation, which allows sharing parts.
    private readonly HashSet<Author> _authors = new();
    
    public IReadOnlyCollection<Author> GetAuthors()
        => _authors.ToList().AsReadOnly();
    
    private static List<Resource> _allResources;
    private static readonly object _lockResource;
    private static readonly JsonPersistenceService _persistenceService = new("data");

    static Resource()
    {
        try
        {
            var loadedItems = _persistenceService.Load<Resource>();
            lock (_lockResource)
            {
                _allResources = loadedItems;
            }
        }
        catch (FileNotFoundException)
        {
            _allResources = new List<Resource>();
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Failed to load {typeof(Resource).Name}: {ex.Message}");
            _allResources = new List<Resource>();
        }
    }
    
    public Resource(
        string title,
        string description,
        ICollection<Author> authors)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new BookISBNIsEmptyException("Resource title cannot be empty");

        if (authors == null || !authors.Any())
            throw new InvalidOperationException(
                "Resource must have at least one author.");

        Title = title;
        Description = description;

        foreach (var author in authors)
        {
            AddAuthor(author);
        }

        AddResource(this);
    }
    
    public void AddAuthor(Author author)
    {
        if (author == null)
            throw new AuthorIsNullException(nameof(author), "Author is null");

        if (_authors.Contains(author))
            return;

        _authors.Add(author);

        if (!author.GetResources().Contains(this))
            author.AddResource(this);
    }

    public void RemoveAuthor(Author author)
    {
        if (author == null)
            throw new AuthorIsNullException(nameof(author), "Author is null");

        if (!_authors.Contains(author))
            return;

        if (_authors.Count == 1)
            throw new InvalidOperationException(
                "Resource must have at least one author.");

        _authors.Remove(author);

        if (author.GetResources().Contains(this))
            author.RemoveResource(this);
    }

    public void AddCatalog(Catalog catalog)
    {
        _catalogs.Add(catalog);
    }

    public bool RemoveCatalog(Catalog catalog)
    {
        if (!_catalogs.Contains(catalog)) return false;
        _catalogs.Remove(catalog);
        return true;
    }

    public void AddBorrowRecord(BorrowRecord borrowRecord)
    {
        _borrowRecords.Add(borrowRecord);
    }

    public void AssignDigitalResource(IDigitalResource digitalResource)
    {
        if (_digitalResource == null) _digitalResource = digitalResource;
        else
        {
            throw new ResourceAlreadyHaveChildClassException("Resource already have an assigned digital resource instance.");
        }
    }
    
    public void AssignPrintedResource(IPrintedResource printedResource)
    {
        if (_printedResource == null) _printedResource = printedResource;
        else
        {
            throw new ResourceAlreadyHaveChildClassException("Resource already have an assigned printed resource instance.");
        }
    }
    
    private static void AddResource(Resource resource)
    {
        if (resource == null)
            throw new BookIsNullException(nameof(resource), "Cannot add null book to extent");
        
        if (string.IsNullOrWhiteSpace(resource.Title))
            throw new BookISBNIsEmptyException("Book ISBN cannot be empty");

        if (_allResources.Any(b => b.Title.Equals(resource.Title, StringComparison.OrdinalIgnoreCase)))
            throw new BookAlreadyExistsException($"Book with ISBN {resource.Title} already exists in extent");

        _allResources.Add(resource);

        try
        {
            _persistenceService.Save(_allResources);
        }
        catch (Exception ex)
        {
            _allResources.Remove(resource);
            throw new InvalidOperationException("Failed to persist Book to file", ex);
        }
        
    }

    public static bool RemoveResource(string title)
    {
        lock (_lockResource)
        {
            var resource = _allResources.FirstOrDefault(b =>
                b.Title.Equals(title, StringComparison.OrdinalIgnoreCase));

            if (resource != null)
            {
                // Cascade delete: remove all children of this resource
                if (resource._digitalResource != null)
                {
                    if (resource._digitalResource.GetType() == typeof(Book))
                    {
                        if (!Book.RemoveBook((resource._digitalResource as Book).ISBN)) return false;
                    }
                    else if (resource._digitalResource.GetType() == typeof(OnlineMagazine))  // Else if as the further inheritance is disjoint
                    {
                        if (!OnlineMagazine.RemoveOnlineMagazine((resource._digitalResource as OnlineMagazine)
                                .PageLink)) return false;
                    }
                }

                if (resource._printedResource != null)  // If instead of else-if as inheritance is overlapping and can be both at the same time
                {
                    if (!Newspaper.RemoveNewspaper((resource._printedResource as Newspaper).Title,
                            (resource._printedResource as Newspaper).Publisher)) return false;
                }
                return _allResources.Remove(resource);
            }
            return false;
        }
    }
    
    public static IReadOnlyList<Resource> GetAllResources()
    {
        lock (_lockResource)
        {
            return _allResources.AsReadOnly();
        }
    }
    
    public static void ClearBookExtent()
    {
        lock (_lockResource)
        {
            while (_allResources.Count != 0) RemoveResource(_allResources.Last().Title);  // Properly removes all the books, including translations.
            _persistenceService.Save(_allResources);
        }
    }

}
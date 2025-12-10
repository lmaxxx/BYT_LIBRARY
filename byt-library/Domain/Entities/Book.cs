using System.Text.Json.Serialization;
using byt_library.Domain.Enums;
using byt_library.Domain.Interfaces;
using byt_library.Domain.Services;
using byt_library.Domain.Exceptions;

namespace byt_library.Domain.Entities;

public class Book : IDigitalResource, IPrintedResource
{
    public string ISBN { get; set; }
    public bool HasAudio { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public int Size { get; set; }
    public string Link { get; set; }
    [JsonInclude] 
    private readonly HashSet<Translation> _translations = new();
    public CoverType CoverType { get; set; }
    public int Quantity { get; set; }

    private static List<Book> _allBooks = new();
    private static readonly object _lockBook = new();
    private static readonly JsonPersistenceService _persistenceService = new("data");

    static Book()
    {
        try
        {
            var loadedItems = _persistenceService.Load<Book>();
            lock (_lockBook)
            {
                _allBooks = loadedItems;
            }
        }
        catch (FileNotFoundException)
        {
            _allBooks = new List<Book>();
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Failed to load {typeof(Book).Name}: {ex.Message}");
            _allBooks = new List<Book>();
        }
    }

    public Book()
    {
        _translations = new HashSet<Translation>();
    }

    [JsonConstructor]
    public Book(string isbn, string title, string description,
                bool hasAudio = false, int size = 0, string link = "",
                CoverType coverType = CoverType.Soft, int quantity = 1, HashSet<Translation>? _translations = null)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new TitleIsEmptyException("Book title cannot be empty");
        
        if (string.IsNullOrWhiteSpace(description))
            throw new DescriptionIsEmptyException();

        if (quantity <= 0)
            throw new InvalidQuantityException();
        
        ISBN = isbn;
        Title = title;
        Description = description;
        HasAudio = hasAudio;
        Size = size;
        Link = link;
        CoverType = coverType;
        Quantity = quantity;
        this._translations = _translations ?? new HashSet<Translation>();
        AddBook(this);
    }

    private static void AddBook(Book book)
    {
        if (book == null)
            throw new BookIsNullException(nameof(book), "Cannot add null book to extent");

        lock (_lockBook)
        {
            if (string.IsNullOrWhiteSpace(book.ISBN))
                throw new BookISBNIsEmptyException("Book ISBN cannot be empty");

            if (_allBooks.Any(b => b.ISBN.Equals(book.ISBN, StringComparison.OrdinalIgnoreCase)))
                throw new BookAlreadyExistsException($"Book with ISBN {book.ISBN} already exists in extent");

            _allBooks.Add(book);

            try
            {
                _persistenceService.Save(_allBooks);
            }
            catch (Exception ex)
            {
                _allBooks.Remove(book);
                throw new InvalidOperationException("Failed to persist Book to file", ex);
            }
        }
    }

    public static bool RemoveBook(string isbn)
    {
        lock (Translation._lockTranslation)  // Lock order: Translation first to prevent deadlock
        {
            lock (_lockBook)
            {
                var book = _allBooks.FirstOrDefault(b =>
                    b.ISBN.Equals(isbn, StringComparison.OrdinalIgnoreCase));

                if (book != null)
                {
                    // Cascade delete: remove all translations for this book
                    Translation.RemoveTranslationsByOwner(book);
                    return _allBooks.Remove(book);
                }
                return false;
            }
        }
    }

    public static Book? GetBookByIsbn(string isbn)
    {
        lock (_lockBook)
        {
            return _allBooks.FirstOrDefault(b => b.ISBN.Equals(isbn, StringComparison.OrdinalIgnoreCase));
        }
    }

    public static IReadOnlyList<Book> GetAllBooks()
    {
        lock (_lockBook)
        {
            return _allBooks.AsReadOnly();
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
        _translations.Add(new Translation($"https://some.storage/online-books/{ISBN}/{language}", language,
            ISBN));
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

    public static void ClearBookExtent()
    {
        lock (_lockBook)
        {
            while (_allBooks.Count != 0) RemoveBook(_allBooks.Last().ISBN);  // Properly removes all the books, including translations.
            _persistenceService.Save(_allBooks);
        }
    }
}
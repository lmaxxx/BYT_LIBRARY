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
    public List<Translation> Translations { get; set; }
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
        Translations = new List<Translation>();
    }

    public Book(string isbn, string title, string description,
                bool hasAudio = false, int size = 0, string link = "",
                CoverType coverType = CoverType.Soft, int quantity = 1)
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
        Translations = new List<Translation>();
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
        lock (_lockBook)
        {
            var book = _allBooks.FirstOrDefault(b => b.ISBN.Equals(isbn, StringComparison.OrdinalIgnoreCase));
            if (book != null)
            {
                return _allBooks.Remove(book);
            }
            return false;
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

    public static void ClearBookExtent()
    {
        lock (_lockBook)
        {
            _allBooks.Clear();
            _persistenceService.Save(_allBooks);
        }
    }
}
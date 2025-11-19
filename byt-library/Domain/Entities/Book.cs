using byt_library.Domain.Enums;
using byt_library.Domain.Interfaces;

namespace byt_library.Domain.Entities;

public class Book : IDigitalResource, IPrintedResource
{
    public required string ISBN { get; set; }
    public bool HasAudio { get; set; }
    public required string Title { get; set; }
    public required string Description { get; set; }
    public int Size { get; set; }
    public required string Link { get; set; }
    public required List<Translation> Translations { get; set; }
    public required CoverType CoverType { get; set; }
    public int Quantity { get; set; }

    private static List<Book> _allBooks = new();
    private static readonly object _lockBook = new();

    public static void AddBook(Book book)
    {
        if (book == null)
            throw new BookIsNullException(nameof(book), "Cannot add null book to extent");

        lock (_lockBook)
        {
            if (string.IsNullOrWhiteSpace(book.ISBN))
                throw new ArgumentException("Book ISBN cannot be empty");

<<<<<<< HEAD
            if (_allBooks.Any(b => b.ISBN.Equals(book.ISBN, StringComparison.OrdinalIgnoreCase)))
                throw new InvalidOperationException($"Book with ISBN {book.ISBN} already exists in extent");
=======
            if (_allBooks.Any(b => b.Id == book.Id))
                throw new BookAlreadyExistsException($"Book with ID {book.Id} already exists in extent");
>>>>>>> feat/customexceptions

            _allBooks.Add(book);
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
        }
    }
}
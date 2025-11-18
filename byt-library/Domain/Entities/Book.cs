using System.Text.Json;

using byt_library.Domain.Enums;
using byt_library.Domain.Interfaces;

namespace byt_library.Domain.Entities;

public class Book : IDigitalResource, IPrintedResource
{
    public int Id { get; private set; }
    private static List<Book> Books = new List<Book>();

    public bool HasAudio { get; set; }
    public required string Title { get; set; }
    public required string Description { get; set; }
    public int Size { get; set; }
    public required string Link { get; set; }
    public required List<Translation> Translations { get; set; }
    public required CoverType CoverType { get; set; }
    public int Quantity { get; set; }
    public string IBSN { get; set; }
    
    public Book(string ISBN, bool hasAudio, string title, string description, CoverType coverType, int quantity, int size, string link)
    {
        ISBN = ISBN;
        HasAudio = hasAudio;
        Title = title;
        Description = description;
        CoverType = coverType;
        Quantity = quantity;
        Size = size;
        Link = link;
        Books.Add(this);
    }

    private static List<Book> _allBooks = new();
    private static int _nextId = 1;
    private static readonly object _lockBook = new();

    public static void AddBook(Book book)
    {
        if (book == null)
            throw new BookIsNullException(nameof(book), "Cannot add null book to extent");

        lock (_lockBook)
        {
            if (book.Id == 0)
            {
                book.Id = _nextId++;
            }

            if (_allBooks.Any(b => b.Id == book.Id))
                throw new BookAlreadyExistsException($"Book with ID {book.Id} already exists in extent");

            _allBooks.Add(book);
        }
    }

    public static bool RemoveBook(int id)
    {
        lock (_lockBook)
        {
            var book = _allBooks.FirstOrDefault(b => b.Id == id);
            if (book != null)
            {
                return _allBooks.Remove(book);
            }
            return false;
        }
    }

    public static Book? GetBookById(int id)
    {
        lock (_lockBook)
        {
            return _allBooks.FirstOrDefault(b => b.Id == id);
        }
    }

    public static IReadOnlyList<Book> GetAllBooks()
    {
        lock (_lockBook)
        {
            return _allBooks.AsReadOnly();
        }
    }

    public static int GetBookCount()
    {
        lock (_lockBook)
        {
            return _allBooks.Count;
        }
    }

    public static void ClearBookExtent()
    {
        lock (_lockBook)
        {
            _allBooks.Clear();
            _nextId = 1;
        }
    }

    public static void SaveBooksToFile(string filePath)
    {
        lock (_lockBook)
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNameCaseInsensitive = true
            };

            var json = JsonSerializer.Serialize(_allBooks, options);
            File.WriteAllText(filePath, json);
        }
    }

    public static void LoadBooksFromFile(string filePath)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException($"File not found: {filePath}");

        lock (_lockBook)
        {
            var json = File.ReadAllText(filePath);
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var bookList = JsonSerializer.Deserialize<List<Book>>(json, options);

            if (bookList != null)
            {
                ClearBookExtent();

                foreach (var book in bookList)
                {
                    AddBook(book);
                }
            }
        }
    }
}
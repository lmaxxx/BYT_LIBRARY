using System.Text.Json;

namespace byt_library.Domain.Entities;

public class Author : Person
{
    public string? Nickname { get; set; }

    private static List<Author> _allAuthors = new();
    private static readonly object _lockAuthor = new();

    public Author(string firstName, string lastName, DateTime dateOfBirth, string? email = null, string? nickname = null, int id = 0)
        : base(firstName, lastName, dateOfBirth, email, id)
    {
        Nickname = nickname;
    }

    public static void AddAuthor(Author author)
    {
        if (author == null)
            throw new ArgumentNullException(nameof(author), "Cannot add null author to extent");

        AddPerson(author);

        lock (_lockAuthor)
        {
            if (_allAuthors.Any(a => a.Id == author.Id))
                throw new InvalidOperationException($"Author with ID {author.Id} already exists in Author extent");

            if (!string.IsNullOrWhiteSpace(author.Nickname) &&
                _allAuthors.Any(a => a.Nickname != null && a.Nickname.Equals(author.Nickname, StringComparison.OrdinalIgnoreCase)))
                throw new InvalidOperationException($"Author with nickname '{author.Nickname}' already exists in Author extent");

            _allAuthors.Add(author);
        }
    }

    public static bool RemoveAuthor(int id)
    {
        lock (_lockAuthor)
        {
            var author = _allAuthors.FirstOrDefault(a => a.Id == id);
            if (author != null)
            {
                _allAuthors.Remove(author);
                RemovePerson(id);
                return true;
            }
            return false;
        }
    }

    public static Author? GetAuthorById(int id)
    {
        lock (_lockAuthor)
        {
            return _allAuthors.FirstOrDefault(a => a.Id == id);
        }
    }

    public static Author? GetAuthorByNickname(string nickname)
    {
        lock (_lockAuthor)
        {
            return _allAuthors.FirstOrDefault(a => a.Nickname != null &&
                                                   a.Nickname.Equals(nickname, StringComparison.OrdinalIgnoreCase));
        }
    }

    public static IReadOnlyList<Author> GetAllAuthors()
    {
        lock (_lockAuthor)
        {
            return _allAuthors.AsReadOnly();
        }
    }

    public static IReadOnlyList<Author> GetAuthorsByLastName(string lastName)
    {
        lock (_lockAuthor)
        {
            return _allAuthors.Where(a => a.LastName.Equals(lastName, StringComparison.OrdinalIgnoreCase))
                             .ToList()
                             .AsReadOnly();
        }
    }

    public static int GetAuthorCount()
    {
        lock (_lockAuthor)
        {
            return _allAuthors.Count;
        }
    }

    public static void ClearAuthorExtent()
    {
        lock (_lockAuthor)
        {
            foreach (var author in _allAuthors.ToList())
            {
                RemovePerson(author.Id);
            }
            _allAuthors.Clear();
        }
    }

    public static void SaveAuthorsToFile(string filePath)
    {
        lock (_lockAuthor)
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNameCaseInsensitive = true
            };

            var json = JsonSerializer.Serialize(_allAuthors, options);
            File.WriteAllText(filePath, json);
        }
    }

    public static void LoadAuthorsFromFile(string filePath)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException($"File not found: {filePath}");

        lock (_lockAuthor)
        {
            var json = File.ReadAllText(filePath);
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var authorList = JsonSerializer.Deserialize<List<Author>>(json, options);

            if (authorList != null)
            {
                ClearAuthorExtent();

                foreach (var author in authorList)
                {
                    AddAuthor(author);
                }
            }
        }
    }

    public override string ToString()
    {
        return base.ToString() + (Nickname != null ? $" (aka {Nickname})" : "");
    }
}
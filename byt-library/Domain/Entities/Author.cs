namespace byt_library.Domain.Entities;

public class Author : Person
{
    public string? Nickname { get; set; }

    private static List<Author> _allAuthors = new();
    private static readonly object _lockAuthor = new();

    public Author(string firstName, string lastName, string email, DateTime dateOfBirth, string? nickname = null)
        : base(firstName, lastName, email, dateOfBirth)
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
            if (_allAuthors.Any(a => a.Email.Equals(author.Email, StringComparison.OrdinalIgnoreCase)))
                throw new InvalidOperationException($"Author with email {author.Email} already exists in Author extent");

            if (!string.IsNullOrWhiteSpace(author.Nickname) &&
                _allAuthors.Any(a => a.Nickname != null && a.Nickname.Equals(author.Nickname, StringComparison.OrdinalIgnoreCase)))
                throw new InvalidOperationException($"Author with nickname '{author.Nickname}' already exists in Author extent");

            _allAuthors.Add(author);
        }
    }

    public static bool RemoveAuthor(string email)
    {
        lock (_lockAuthor)
        {
            var author = _allAuthors.FirstOrDefault(a => a.Email.Equals(email, StringComparison.OrdinalIgnoreCase));
            if (author != null)
            {
                _allAuthors.Remove(author);
                RemovePerson(email);
                return true;
            }
            return false;
        }
    }

    public static Author? GetAuthorByEmail(string email)
    {
        lock (_lockAuthor)
        {
            return _allAuthors.FirstOrDefault(a => a.Email.Equals(email, StringComparison.OrdinalIgnoreCase));
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

    public static void ClearAuthorExtent()
    {
        lock (_lockAuthor)
        {
            foreach (var author in _allAuthors.ToList())
            {
                RemovePerson(author.Email);
            }
            _allAuthors.Clear();
        }
    }

    public override string ToString()
    {
        return base.ToString() + (Nickname != null ? $" (aka {Nickname})" : "");
    }
}
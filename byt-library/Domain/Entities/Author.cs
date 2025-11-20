namespace byt_library.Domain.Entities;

public class Author : Person
{
    public string? Nickname { get; set; }

    private static List<Author> _allAuthors = new();
    private static readonly object _lockAuthor = new();

    public Author(string firstName, string lastName, DateTime dateOfBirth, string? email = null, string? nickname = null)
        : base(firstName, lastName, dateOfBirth, email)
    {
        Nickname = nickname;
    }

    public static void AddAuthor(Author author)
    {
        if (author == null)
            throw new AuthorIsNullException(nameof(author), "Cannot add null author to extent");

        AddPerson(author);

        lock (_lockAuthor)
        {
            if (_allAuthors.Any(a => a.FirstName.Equals(author.FirstName, StringComparison.OrdinalIgnoreCase) &&
                                     a.LastName.Equals(author.LastName, StringComparison.OrdinalIgnoreCase)))
                throw new AuthorWithSuchNameAlreadyExistsException($"Author with name {author.FirstName} {author.LastName} already exists in Author extent");
            
            _allAuthors.Add(author);
        }
    }

    public static bool RemoveAuthor(string firstName, string lastName)
    {
        lock (_lockAuthor)
        {
            var author = _allAuthors.FirstOrDefault(a =>
                a.FirstName.Equals(firstName, StringComparison.OrdinalIgnoreCase) &&
                a.LastName.Equals(lastName, StringComparison.OrdinalIgnoreCase));
            if (author != null)
            {
                _allAuthors.Remove(author);
                RemovePerson(firstName, lastName);
                return true;
            }
            return false;
        }
    }

    public static Author? GetAuthorByName(string firstName, string lastName)
    {
        lock (_lockAuthor)
        {
            return _allAuthors.FirstOrDefault(a =>
                a.FirstName.Equals(firstName, StringComparison.OrdinalIgnoreCase) &&
                a.LastName.Equals(lastName, StringComparison.OrdinalIgnoreCase));
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
                RemovePerson(author.FirstName, author.LastName);
            }
            _allAuthors.Clear();
        }
    }

    public override string ToString()
    {
        return base.ToString() + (Nickname != null ? $" (aka {Nickname})" : "");
    }
}
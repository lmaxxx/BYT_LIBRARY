using byt_library.Domain.Services;
using byt_library.Domain.Exceptions;
using byt_library.Domain.Interfaces;

namespace byt_library.Domain.Entities;

public class Author : IAuthor
{
    private readonly Person _person;
    public Person GetPerson() => _person;
    
    private readonly HashSet<Resource> _resources = new();
    public IReadOnlyCollection<Resource> GetResources()
        => _resources.ToList().AsReadOnly();
    
    public string? Nickname { get; set; }

    private static List<Author> _allAuthors = new();
    private static readonly object _lockAuthor = new();
    private static readonly JsonPersistenceService _persistenceService = new("data");

    static Author()
    {
        try
        {
            var loadedItems = _persistenceService.Load<Author>();
            lock (_lockAuthor)
            {
                _allAuthors = loadedItems;
            }
        }
        catch (FileNotFoundException)
        {
            _allAuthors = new List<Author>();
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Failed to load {typeof(Author).Name}: {ex.Message}");
            _allAuthors = new List<Author>();
        }
    }
    
    public Author(Person person, string? nickname = null)
    {
        if (person == null)
            throw new PersonIsNullException(nameof(person), "Person is null");

        if (nickname != null && string.IsNullOrWhiteSpace(nickname))
            throw new NicknameIsEmptyException();

        if (person.GetAuthor() != null)
            throw new AuthorWithSuchNameAlreadyExistsException(
                "Person already has an Author role.");

        _person = person;
        Nickname = nickname;

        person.AssignAuthor(this);
        AddAuthor(this);
    }

    private static void AddAuthor(Author author)
    {
        if (author == null)
            throw new AuthorIsNullException(nameof(author), "Cannot add null author to extent");

        var person = author.GetPerson();

        lock (_lockAuthor)
        {
            if (_allAuthors.Any(a =>
                    a.GetPerson().FirstName.Equals(person.FirstName, StringComparison.OrdinalIgnoreCase) &&
                    a.GetPerson().LastName.Equals(person.LastName, StringComparison.OrdinalIgnoreCase)))
            {
                throw new AuthorWithSuchNameAlreadyExistsException(
                    $"Author with name {person.FirstName} {person.LastName} already exists in Author extent");
            }

            if (!string.IsNullOrWhiteSpace(author.Nickname) &&
                _allAuthors.Any(a => a.Nickname != null &&
                                     a.Nickname.Equals(author.Nickname, StringComparison.OrdinalIgnoreCase)))
            {
                throw new AuthorWithSuchNicknameAlreadyExistsException(
                    $"Author with nickname {author.Nickname} already exists in Author extent");
            }

            _allAuthors.Add(author);
            _persistenceService.Save(_allAuthors);
        }
    }

    public static bool RemoveAuthor(string firstName, string lastName)
    {
        lock (_lockAuthor)
        {
            var author = _allAuthors.FirstOrDefault(a =>
                a.GetPerson().FirstName.Equals(firstName, StringComparison.OrdinalIgnoreCase) &&
                a.GetPerson().LastName.Equals(lastName, StringComparison.OrdinalIgnoreCase));

            if (author != null)
            {
                _allAuthors.Remove(author);
                author.GetPerson().RemoveAuthor(); // remove role only
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
                a.GetPerson().FirstName.Equals(firstName, StringComparison.OrdinalIgnoreCase) &&
                a.GetPerson().LastName.Equals(lastName, StringComparison.OrdinalIgnoreCase));
        }
    }

    public static Author? GetAuthorByNickname(string nickname)
    {
        lock (_lockAuthor)
        {
            return _allAuthors.FirstOrDefault(a =>
                a.Nickname != null &&
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
            return _allAuthors
                .Where(a => a.GetPerson().LastName.Equals(lastName, StringComparison.OrdinalIgnoreCase))
                .ToList()
                .AsReadOnly();
        }
    }

    public static void ClearAuthorExtent()
    {
        lock (_lockAuthor)
        {
            foreach (var author in _allAuthors)
            {
                author.GetPerson().RemoveAuthor();
            }

            _allAuthors.Clear();
            _persistenceService.Save(_allAuthors);
        }
    }
    
    public void AddResource(Resource resource)
    {
        if (resource == null)
            throw new ResourceIsNullException(nameof(resource), "Resource is null");

        if (_resources.Contains(resource))
            return;

        _resources.Add(resource);

        if (!resource.GetAuthors().Contains(this))
            resource.AddAuthor(this);
    }

    public void RemoveResource(Resource resource)
    {
        if (resource == null)
            throw new ResourceIsNullException(nameof(resource), "Resource is null");

        if (!_resources.Contains(resource))
            return;

        // enforce min cardinality on Resource
        if (resource.GetAuthors().Count == 1)
            throw new InvalidOperationException(
                "Resource must have at least one author.");

        _resources.Remove(resource);

        if (resource.GetAuthors().Contains(this))
            resource.RemoveAuthor(this);
    }

    public override string ToString()
    {
        return base.ToString() + (Nickname != null ? $" (aka {Nickname})" : "");
    }
}
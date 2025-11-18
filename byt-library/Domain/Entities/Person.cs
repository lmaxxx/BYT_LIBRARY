namespace byt_library.Domain.Entities;

public class Person
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public DateTime DateOfBirth { get; set; }

    public int Age
    {
        get
        {
            var today = DateTime.Today;
            var age = today.Year - DateOfBirth.Year;

            if (DateOfBirth.Date > today.AddYears(-age))
            {
                age--;
            }

            return age;
        }
    }

    private static List<Person> _allPersons = new();
    private static readonly object _lock = new();

    public Person(string firstName, string lastName, string email, DateTime dateOfBirth)
    {
        FirstName = firstName;
        LastName = lastName;
        Email = email;
        DateOfBirth = dateOfBirth;
    }

    public static void AddPerson(Person person)
    {
        if (person == null)
            throw new ArgumentNullException(nameof(person), "Cannot add null person to extent");

        lock (_lock)
        {
            if (string.IsNullOrWhiteSpace(person.Email))
                throw new ArgumentException("Person email cannot be empty");

            if (_allPersons.Any(p => p.Email.Equals(person.Email, StringComparison.OrdinalIgnoreCase)))
                throw new InvalidOperationException($"Person with email {person.Email} already exists in extent");

            _allPersons.Add(person);
        }
    }

    public static bool RemovePerson(string email)
    {
        lock (_lock)
        {
            var person = _allPersons.FirstOrDefault(p => p.Email.Equals(email, StringComparison.OrdinalIgnoreCase));
            if (person != null)
            {
                return _allPersons.Remove(person);
            }
            return false;
        }
    }

    public static Person? GetPersonByEmail(string email)
    {
        lock (_lock)
        {
            return _allPersons.FirstOrDefault(p => p.Email.Equals(email, StringComparison.OrdinalIgnoreCase));
        }
    }

    public static IReadOnlyList<Person> GetAllPersons()
    {
        lock (_lock)
        {
            return _allPersons.AsReadOnly();
        }
    }

    public static void ClearExtent()
    {
        lock (_lock)
        {
            _allPersons.Clear();
        }
    }

    public override string ToString()
    {
        return $"{FirstName} {LastName} (Age: {Age}) - {Email}";
    }
}
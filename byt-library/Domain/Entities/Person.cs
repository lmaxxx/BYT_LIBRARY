namespace byt_library.Domain.Entities;

public class Person
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string? Email { get; set; }
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

    public Person(string firstName, string lastName, DateTime dateOfBirth, string? email = null)
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
            if (string.IsNullOrWhiteSpace(person.FirstName))
                throw new ArgumentException("Person first name cannot be empty");

            if (string.IsNullOrWhiteSpace(person.LastName))
                throw new ArgumentException("Person last name cannot be empty");

            if (_allPersons.Any(p => p.FirstName.Equals(person.FirstName, StringComparison.OrdinalIgnoreCase) &&
                                     p.LastName.Equals(person.LastName, StringComparison.OrdinalIgnoreCase)))
                throw new InvalidOperationException($"Person with name {person.FirstName} {person.LastName} already exists in extent");

            _allPersons.Add(person);
        }
    }

    public static bool RemovePerson(string firstName, string lastName)
    {
        lock (_lock)
        {
            var person = _allPersons.FirstOrDefault(p =>
                p.FirstName.Equals(firstName, StringComparison.OrdinalIgnoreCase) &&
                p.LastName.Equals(lastName, StringComparison.OrdinalIgnoreCase));
            if (person != null)
            {
                return _allPersons.Remove(person);
            }
            return false;
        }
    }

    public static Person? GetPersonByName(string firstName, string lastName)
    {
        lock (_lock)
        {
            return _allPersons.FirstOrDefault(p =>
                p.FirstName.Equals(firstName, StringComparison.OrdinalIgnoreCase) &&
                p.LastName.Equals(lastName, StringComparison.OrdinalIgnoreCase));
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
        var emailPart = string.IsNullOrWhiteSpace(Email) ? "No email" : Email;
        return $"{FirstName} {LastName} (Age: {Age}) - {emailPart}";
    }
}
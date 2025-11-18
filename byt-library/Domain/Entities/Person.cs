using System.Text.Json;

namespace byt_library.Domain.Entities;

public class Person
{
    public int Id { get; private set; }
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
    private static int _nextId = 1;
    private static readonly object _lock = new();

    public Person(string firstName, string lastName, DateTime dateOfBirth, string? email = null, int id = 0)
    {
        FirstName = firstName;
        LastName = lastName;
        DateOfBirth = dateOfBirth;
        Email = email;

        if (id > 0)
        {
            Id = id;
            lock (_lock)
            {
                if (id >= _nextId)
                    _nextId = id + 1;
            }
        }
    }

    public static void AddPerson(Person person)
    {
        if (person == null)
            throw new PersonIsNullException(nameof(person), "Cannot add null person to extent");

        lock (_lock)
        {
            if (person.Id == 0)
            {
                person.Id = _nextId++;
            }

            if (_allPersons.Any(p => p.Id == person.Id))
                throw new PersonAlreadyExistsException($"Person with ID {person.Id} already exists in extent");

            if (!string.IsNullOrWhiteSpace(person.Email) &&
                _allPersons.Any(p => p.Email != null && p.Email.Equals(person.Email, StringComparison.OrdinalIgnoreCase)))
                throw new PersonWithThisEmailAlreadyExistsException($"Person with email {person.Email} already exists in extent");

            _allPersons.Add(person);
        }
    }

    public static bool RemovePerson(int id)
    {
        lock (_lock)
        {
            var person = _allPersons.FirstOrDefault(p => p.Id == id);
            if (person != null)
            {
                return _allPersons.Remove(person);
            }
            return false;
        }
    }

    public static Person? GetPersonById(int id)
    {
        lock (_lock)
        {
            return _allPersons.FirstOrDefault(p => p.Id == id);
        }
    }

    public static IReadOnlyList<Person> GetAllPersons()
    {
        lock (_lock)
        {
            return _allPersons.AsReadOnly();
        }
    }

    public static int GetPersonCount()
    {
        lock (_lock)
        {
            return _allPersons.Count;
        }
    }

    public static void ClearExtent()
    {
        lock (_lock)
        {
            _allPersons.Clear();
            _nextId = 1;
        }
    }

    public static void SaveToFile(string filePath)
    {
        lock (_lock)
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNameCaseInsensitive = true
            };

            var json = JsonSerializer.Serialize(_allPersons, options);
            File.WriteAllText(filePath, json);
        }
    }

    public static void LoadFromFile(string filePath)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException($"File not found: {filePath}");

        lock (_lock)
        {
            var json = File.ReadAllText(filePath);
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var persons = JsonSerializer.Deserialize<List<Person>>(json, options);

            if (persons != null)
            {
                _allPersons.Clear();
                _nextId = 1;

                foreach (var person in persons)
                {
                    AddPerson(person);
                }
            }
        }
    }

    public override string ToString()
    {
        return $"[{Id}] {FirstName} {LastName} (Age: {Age})" +
               (Email != null ? $" - {Email}" : "");
    }
}
using byt_library.Domain.Services;
using byt_library.Domain.Exceptions;

namespace byt_library.Domain.Entities;

public class Student : Person
{
    public DateTime EnrollmentDate { get; set; }
    
    private static List<Student> _allStudents = new();
    private static readonly object _lockStudent = new();
    private static readonly JsonPersistenceService _persistenceService = new("data");

    static Student()
    {
        try
        {
            var loadedItems = _persistenceService.Load<Student>();
            lock (_lockStudent)
            {
                _allStudents = loadedItems;
            }
        }
        catch (FileNotFoundException)
        {
            _allStudents = new List<Student>();
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Failed to load {typeof(Student).Name}: {ex.Message}");
            _allStudents = new List<Student>();
        }
    }

    public Student(string firstName, string lastName, DateTime dateOfBirth, DateTime enrollmentDate, string? email = null)
        : base(firstName, lastName, dateOfBirth, email)
    {
        if (enrollmentDate > DateTime.Now)
            throw new InvalidEnrollmentDateException("Enrollment date cannot be in the future.");
        
        EnrollmentDate = enrollmentDate;

        AddStudent(this);
    }

    public bool isAllowedToBorrow()
    {
        return false;
    }
    
    private static void AddStudent(Student student)
    {
        if (student == null)
            throw new StudentIsNullException(nameof(student), "Cannot add null student to extent");

        lock (_lockStudent)
        {
            if (_allStudents.Any(s => s.FirstName.Equals(student.FirstName, StringComparison.OrdinalIgnoreCase) &&
                                      s.LastName.Equals(student.LastName, StringComparison.OrdinalIgnoreCase)))
                throw new StudentAlreadyExistsException($"Student with name {student.FirstName} {student.LastName} already exists in Student extent");

            _allStudents.Add(student);

            try
            {
                _persistenceService.Save(_allStudents);
            }
            catch (Exception ex)
            {
                _allStudents.Remove(student);
                throw new InvalidOperationException("Failed to persist Student to file", ex);
            }
        }
    }

    public static bool RemoveStudent(string firstName, string lastName)
    {
        lock (_lockStudent)
        {
            var student = _allStudents.FirstOrDefault(s =>
                s.FirstName.Equals(firstName, StringComparison.OrdinalIgnoreCase) &&
                s.LastName.Equals(lastName, StringComparison.OrdinalIgnoreCase));
            if (student != null)
            {
                _allStudents.Remove(student);
                RemovePerson(firstName, lastName);
                return true;
            }
            return false;
        }
    }

    public static Student? GetStudentByName(string firstName, string lastName)
    {
        lock (_lockStudent)
        {
            return _allStudents.FirstOrDefault(s =>
                s.FirstName.Equals(firstName, StringComparison.OrdinalIgnoreCase) &&
                s.LastName.Equals(lastName, StringComparison.OrdinalIgnoreCase));
        }
    }
    
    public static IReadOnlyList<Student> GetAllStudents()
    {
        lock (_lockStudent)
        {
            return _allStudents.AsReadOnly();
        }
    }
    
    public static IReadOnlyList<Student> GetStudentsEnrolledAfter(DateTime date)
    {
        lock (_lockStudent)
        {
            return _allStudents.Where(s => s.EnrollmentDate > date)
                              .ToList()
                              .AsReadOnly();
        }
    }
    
    public static IReadOnlyList<Student> GetStudentsByEnrollmentYear(int year)
    {
        lock (_lockStudent)
        {
            return _allStudents.Where(s => s.EnrollmentDate.Year == year)
                              .ToList()
                              .AsReadOnly();
        }
    }

    public static void ClearStudentExtent()
    {
        lock (_lockStudent)
        {
            _allStudents.Clear();
            _persistenceService.Save(_allStudents);
        }
    }

    public override string ToString()
    {
        return base.ToString() + $" - Enrolled: {EnrollmentDate:yyyy-MM-dd}";
    }
}
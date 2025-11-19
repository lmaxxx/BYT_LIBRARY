namespace byt_library.Domain.Entities;

public class Student : Person
{
    public DateTime EnrollmentDate { get; set; }
    
    private static List<Student> _allStudents = new();
    private static readonly object _lockStudent = new();

    public Student(string firstName, string lastName, DateTime dateOfBirth, DateTime enrollmentDate, string? email = null)
        : base(firstName, lastName, dateOfBirth, email)
    {
        EnrollmentDate = enrollmentDate;
    }

    public bool isAllowedToBorrow()
    {
        return false;
    }
    
    public static void AddStudent(Student student)
    {
        if (student == null)
            throw new StudentIsNullException(nameof(student), "Cannot add null student to extent");

        AddPerson(student);

        lock (_lockStudent)
        {
            if (_allStudents.Any(s => s.Id == student.Id))
                throw new StudentAlreadyExistsException($"Student with ID {student.Id} already exists in Student extent");

            _allStudents.Add(student);
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
            foreach (var student in _allStudents.ToList())
            {
                RemovePerson(student.FirstName, student.LastName);
            }
            _allStudents.Clear();
        }
    }

    public override string ToString()
    {
        return base.ToString() + $" - Enrolled: {EnrollmentDate:yyyy-MM-dd}";
    }
}
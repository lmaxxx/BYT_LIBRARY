namespace byt_library.Domain.Entities;

public class Student : Person
{
    public DateTime EnrollmentDate { get; set; }
    
    private static List<Student> _allStudents = new();
    private static readonly object _lockStudent = new();

    public Student(string firstName, string lastName, string email, DateTime dateOfBirth, DateTime enrollmentDate)
        : base(firstName, lastName, email, dateOfBirth)
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
            throw new ArgumentNullException(nameof(student), "Cannot add null student to extent");

        AddPerson(student);

        lock (_lockStudent)
        {
            if (_allStudents.Any(s => s.Email.Equals(student.Email, StringComparison.OrdinalIgnoreCase)))
                throw new InvalidOperationException($"Student with email {student.Email} already exists in Student extent");

            _allStudents.Add(student);
        }
    }

    public static bool RemoveStudent(string email)
    {
        lock (_lockStudent)
        {
            var student = _allStudents.FirstOrDefault(s => s.Email.Equals(email, StringComparison.OrdinalIgnoreCase));
            if (student != null)
            {
                _allStudents.Remove(student);
                RemovePerson(email);
                return true;
            }
            return false;
        }
    }

    public static Student? GetStudentByEmail(string email)
    {
        lock (_lockStudent)
        {
            return _allStudents.FirstOrDefault(s => s.Email.Equals(email, StringComparison.OrdinalIgnoreCase));
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
                RemovePerson(student.Email);
            }
            _allStudents.Clear();
        }
    }

    public override string ToString()
    {
        return base.ToString() + $" - Enrolled: {EnrollmentDate:yyyy-MM-dd}";
    }
}
using System.Text.Json;

namespace byt_library.Domain.Entities;

public class Student : Person
{
    public DateTime EnrollmentDate { get; set; }
    
    private static List<Student> _allStudents = new();
    private static readonly object _lockStudent = new();

    public Student(string firstName, string lastName, DateTime dateOfBirth, DateTime enrollmentDate, string? email = null, int id = 0)
        : base(firstName, lastName, dateOfBirth, email, id)
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
            if (_allStudents.Any(s => s.Id == student.Id))
                throw new InvalidOperationException($"Student with ID {student.Id} already exists in Student extent");

            _allStudents.Add(student);
        }
    }
    
    public static bool RemoveStudent(int id)
    {
        lock (_lockStudent)
        {
            var student = _allStudents.FirstOrDefault(s => s.Id == id);
            if (student != null)
            {
                _allStudents.Remove(student);
                RemovePerson(id);
                return true;
            }
            return false;
        }
    }
    
    public static Student? GetStudentById(int id)
    {
        lock (_lockStudent)
        {
            return _allStudents.FirstOrDefault(s => s.Id == id);
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
    
    public static int GetStudentCount()
    {
        lock (_lockStudent)
        {
            return _allStudents.Count;
        }
    }
    
    public static void ClearStudentExtent()
    {
        lock (_lockStudent)
        {
            foreach (var student in _allStudents.ToList())
            {
                RemovePerson(student.Id);
            }
            _allStudents.Clear();
        }
    }
    
    public static void SaveStudentsToFile(string filePath)
    {
        lock (_lockStudent)
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNameCaseInsensitive = true
            };

            var json = JsonSerializer.Serialize(_allStudents, options);
            File.WriteAllText(filePath, json);
        }
    }
    
    public static void LoadStudentsFromFile(string filePath)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException($"File not found: {filePath}");

        lock (_lockStudent)
        {
            var json = File.ReadAllText(filePath);
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var studentList = JsonSerializer.Deserialize<List<Student>>(json, options);

            if (studentList != null)
            {
                ClearStudentExtent();

                foreach (var student in studentList)
                {
                    AddStudent(student);
                }
            }
        }
    }

    public override string ToString()
    {
        return base.ToString() + $" - Enrolled: {EnrollmentDate:yyyy-MM-dd}";
    }
}
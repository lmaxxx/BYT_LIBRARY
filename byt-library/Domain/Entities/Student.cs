using byt_library.Domain.Enums;
using byt_library.Domain.Exceptions;
using byt_library.Domain.Services;
using byt_library.Domain.Exceptions;
using byt_library.Domain.Interfaces;

namespace byt_library.Domain.Entities;

public class Student : IStudent
{
    private readonly Person _person;
    public Person GetPerson() => _person;
    public DateTime EnrollmentDate { get; set; }
    
    private Subscription? _subscription;
    
    private static List<Student> _allStudents = new();
    private static readonly object _lockStudent = new();
    private static readonly JsonPersistenceService _persistenceService = new("data");
    
    private readonly HashSet<BorrowRecord> _borrowRecords = new();

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
    
    public Student(Person person, DateTime enrollmentDate)
    {
        if (person == null)
            throw new PersonIsNullException(nameof(person), "Person is null");
            
        if (person.GetStaff() != null)
            throw new StudentAlreadyExistsException("Person already has a Student role.");

        if (enrollmentDate > DateTime.Now)
            throw new InvalidEnrollmentDateException("Enrollment date cannot be in the future.");

        EnrollmentDate = enrollmentDate;
        _person = person;
        _person.AssignStudent(this);
        AddStudent(this);
    }

    public void BorrowResource(IResource resource)
    {
        // Check if already exists and borrow using Bag History association
        var record = _borrowRecords.FirstOrDefault(record => record.GetResource() == resource);
        if (record != null)
        {
            _borrowRecords.Add(record.BorrowResource(DateTime.Now, DateTime.Now.AddDays(30)));
        }
        
        // Else create new borrow record
        _borrowRecords.Add(new BorrowRecord(DateTime.Now, DateTime.Now.AddDays(30), null, BorrowRecordStatus.Requested, null, this, resource, null));
    }

    public void AddBorrowRecord(BorrowRecord borrowRecord)  // Required for Borrow record to itself on duplicating
    {
        _borrowRecords.Add(borrowRecord);
    }

    public bool IsAllowedToBorrow()
    {
        return false;
    }
    
    private static void AddStudent(Student student)
    {
        if (student == null)
            throw new StudentIsNullException(nameof(student), "Cannot add null student to extent");

        var person = student.GetPerson();

        lock (_lockStudent)
        {
            if (_allStudents.Any(s =>
                    s.GetPerson().FirstName.Equals(person.FirstName, StringComparison.OrdinalIgnoreCase) &&
                    s.GetPerson().LastName.Equals(person.LastName, StringComparison.OrdinalIgnoreCase)))
            {
                throw new StudentAlreadyExistsException(
                    $"Student with name {person.FirstName} {person.LastName} already exists in Student extent");
            }

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
                s.GetPerson().FirstName.Equals(firstName, StringComparison.OrdinalIgnoreCase) &&
                s.GetPerson().LastName.Equals(lastName, StringComparison.OrdinalIgnoreCase));

            if (student != null)
            {
                _allStudents.Remove(student);
                student.GetPerson().RemoveStudent(); // remove role from person
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
                s.GetPerson().FirstName.Equals(firstName, StringComparison.OrdinalIgnoreCase) &&
                s.GetPerson().LastName.Equals(lastName, StringComparison.OrdinalIgnoreCase));
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
            foreach (var student in _allStudents)
            {
                student.GetPerson().RemoveStudent();
            }

            _allStudents.Clear();
            _persistenceService.Save(_allStudents);
        }
    }

    public override string ToString()
    {
        return base.ToString() + $" - Enrolled: {EnrollmentDate:yyyy-MM-dd}";
    }
    
    public Subscription? GetSubscription()
    {
        return _subscription; 
    }

    public void AddSubscription(Subscription subscription)
    {
        if (subscription == null)
            throw new SubscriptionIsNullException(nameof(subscription), "Subscription must exist.");

        // student can have only 1 subscription
        if (_subscription != null)
            throw new SubscriptionAlreadyBelongsException("Student already has a subscription.");

        // subscription must not belong to a different student
        if (subscription.GetStudent() != null && subscription.GetStudent() != this)
            throw new SubscriptionAlreadyBelongsException("Subscription belongs to another student.");

        _subscription = subscription;

        // reverse connection
        if (subscription.GetStudent() != this)
            subscription.SetStudent(this);
    }

    public void RemoveSubscription()
    {
        if (_subscription == null)
            throw new SubscriptionIsNotAssignedException("Student has no subscription to remove.");

        _subscription = null;
    }
    
    public void UpdateSubscription(Subscription newSub)
    {
        if (newSub == null)
            throw new SubscriptionIsNullException(nameof(newSub), "Subscription must exist.");

        if (_subscription == null)
            throw new SubscriptionIsNotAssignedException("Student has no subscription to update.");

        RemoveSubscription();
        AddSubscription(newSub);
    }

}
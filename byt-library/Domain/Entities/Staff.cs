using byt_library.Domain.Exceptions;
using byt_library.Domain.Services;
using byt_library.Domain.Exceptions;

namespace byt_library.Domain.Entities;

public class Staff : Person
{
    public string Department { get; set; }
    
    private Staff? _supervisor;                        
    private readonly HashSet<Staff> _subordinates = new(); 
    
    private static List<Staff> _allStaff = new();
    private static readonly object _lockStaff = new();
    private static readonly JsonPersistenceService _persistenceService = new("data");

    static Staff()
    {
        try
        {
            var loadedItems = _persistenceService.Load<Staff>();
            lock (_lockStaff)
            {
                _allStaff = loadedItems;
            }
        }
        catch (FileNotFoundException)
        {
            _allStaff = new List<Staff>();
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Failed to load {typeof(Staff).Name}: {ex.Message}");
            _allStaff = new List<Staff>();
        }
    }

    public Staff(string firstName, string lastName, DateTime dateOfBirth, string department, string? email = null)
        : base(firstName, lastName, dateOfBirth, email)
    {
        if (string.IsNullOrWhiteSpace(department))
            throw new DepartmentIsEmptyException();
        
        Department = department;

        AddStaff(this);
    }

    private static void AddStaff(Staff staff)
    {
        if (staff == null)
            throw new StaffIsNullException(nameof(staff), "Cannot add null staff to extent");

        lock (_lockStaff)
        {
            if (_allStaff.Any(s => s.FirstName.Equals(staff.FirstName, StringComparison.OrdinalIgnoreCase) &&
                                   s.LastName.Equals(staff.LastName, StringComparison.OrdinalIgnoreCase)))
                throw new StaffAlreadyExistsException($"Staff with name {staff.FirstName} {staff.LastName} already exists in Staff extent");

            _allStaff.Add(staff);

            try
            {
                _persistenceService.Save(_allStaff);
            }
            catch (Exception ex)
            {
                _allStaff.Remove(staff);
                throw new InvalidOperationException("Failed to persist Staff to file", ex);
            }
        }
    }

    public static bool RemoveStaff(string firstName, string lastName)
    {
        lock (_lockStaff)
        {
            var staff = _allStaff.FirstOrDefault(s =>
                s.FirstName.Equals(firstName, StringComparison.OrdinalIgnoreCase) &&
                s.LastName.Equals(lastName, StringComparison.OrdinalIgnoreCase));
            if (staff != null)
            {
                _allStaff.Remove(staff);
                RemovePerson(firstName, lastName);
                return true;
            }
            return false;
        }
    }

    public static Staff? GetStaffByName(string firstName, string lastName)
    {
        lock (_lockStaff)
        {
            return _allStaff.FirstOrDefault(s =>
                s.FirstName.Equals(firstName, StringComparison.OrdinalIgnoreCase) &&
                s.LastName.Equals(lastName, StringComparison.OrdinalIgnoreCase));
        }
    }

    public static IReadOnlyList<Staff> GetAllStaff()
    {
        lock (_lockStaff)
        {
            return _allStaff.AsReadOnly();
        }
    }

    public static IReadOnlyList<Staff> GetStaffByDepartment(string department)
    {
        lock (_lockStaff)
        {
            return _allStaff.Where(s => s.Department.Equals(department, StringComparison.OrdinalIgnoreCase))
                           .ToList()
                           .AsReadOnly();
        }
    }

    public static void ClearStaffExtent()
    {
        lock (_lockStaff)
        {
            _allStaff.Clear();
            _persistenceService.Save(_allStaff);
        }
    }

    public override string ToString()
    {
        return base.ToString() + $" - Department: {Department}";
    }
    
    public Staff? GetSupervisor()
    {
        return _supervisor;
    }

    public IReadOnlyCollection<Staff> GetSubordinates()
    {
        return _subordinates.ToList().AsReadOnly(); // return copy
    }

    public void SetSupervisor(Staff supervisor)
    {
        if (supervisor == null)
            throw new StaffIsNullException(nameof(supervisor), "Supervisor must exist.");

        if (supervisor == this)
            throw new StaffSelfSupervisionException("A staff member cannot supervise themselves.");

        // avoid infinite recursion
        if (_supervisor == supervisor)
            return;

        // if this staff already has a supervisor, remove old connection
        if (_supervisor != null)
        {
            RemoveSupervisor();
        }

        _supervisor = supervisor;

        // reverse connection
        if (!supervisor._subordinates.Contains(this))
        {
            supervisor.AddSubordinate(this);
        }
    }

    public void RemoveSupervisor()
    {
        if (_supervisor == null)
            return;

        Staff oldSupervisor = _supervisor;
        _supervisor = null;

        // reverse connection
        if (oldSupervisor._subordinates.Contains(this))
        {
            oldSupervisor.RemoveSubordinate(this);
        }
    }

    public void AddSubordinate(Staff subordinate)
    {
        if (subordinate == null)
            throw new StaffIsNullException(nameof(subordinate), "Subordinate must exist.");

        if (subordinate == this)
            throw new StaffSelfSupervisionException("A staff member cannot supervise themselves.");

        if (_subordinates.Contains(subordinate))
            return;

        // if the subordinate already had a supervisor, detach it first
        if (subordinate._supervisor != null && subordinate._supervisor != this)
        {
            subordinate.RemoveSupervisor();
        }

        _subordinates.Add(subordinate);

        // reverse connection
        if (subordinate._supervisor != this)
        {
            subordinate.SetSupervisor(this);
        }
    }
    
    public void RemoveSubordinate(Staff subordinate)
    {
        if (subordinate == null)
            throw new StaffIsNullException(nameof(subordinate), "Subordinate must exist.");

        if (!_subordinates.Contains(subordinate))
            return;

        _subordinates.Remove(subordinate);

        // reverse connection
        if (subordinate._supervisor == this)
        {
            subordinate._supervisor = null;
        }
    }

    public void ChangeSupervisor(Staff newSupervisor)
    {
        if (newSupervisor == null)
            throw new StaffIsNullException(nameof(newSupervisor), "Supervisor must exist.");

        if (newSupervisor == this)
            throw new StaffSelfSupervisionException("A staff member cannot supervise themselves.");

        RemoveSupervisor();
        SetSupervisor(newSupervisor);
    }
}
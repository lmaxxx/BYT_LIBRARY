using System.Text.Json;

namespace byt_library.Domain.Entities;

public class OnlineMaganize
{
    public int Id { get; private set; }
    public string PageLink { get; set; }

    private static List<OnlineMaganize> _allOnlineMaganizes = new();
    private static int _nextId = 1;
    private static readonly object _lockOnlineMaganize = new();

    public static void AddOnlineMaganize(OnlineMaganize onlineMaganize)
    {
        if (onlineMaganize == null)
            throw new ArgumentNullException(nameof(onlineMaganize), "Cannot add null online magazine to extent");

        lock (_lockOnlineMaganize)
        {
            if (onlineMaganize.Id == 0)
            {
                onlineMaganize.Id = _nextId++;
            }

            if (_allOnlineMaganizes.Any(om => om.Id == onlineMaganize.Id))
                throw new InvalidOperationException($"OnlineMaganize with ID {onlineMaganize.Id} already exists in extent");

            _allOnlineMaganizes.Add(onlineMaganize);
        }
    }

    public static bool RemoveOnlineMaganize(int id)
    {
        lock (_lockOnlineMaganize)
        {
            var onlineMaganize = _allOnlineMaganizes.FirstOrDefault(om => om.Id == id);
            if (onlineMaganize != null)
            {
                return _allOnlineMaganizes.Remove(onlineMaganize);
            }
            return false;
        }
    }

    public static OnlineMaganize? GetOnlineMaganizeById(int id)
    {
        lock (_lockOnlineMaganize)
        {
            return _allOnlineMaganizes.FirstOrDefault(om => om.Id == id);
        }
    }

    public static IReadOnlyList<OnlineMaganize> GetAllOnlineMaganizes()
    {
        lock (_lockOnlineMaganize)
        {
            return _allOnlineMaganizes.AsReadOnly();
        }
    }

    public static int GetOnlineMaganizeCount()
    {
        lock (_lockOnlineMaganize)
        {
            return _allOnlineMaganizes.Count;
        }
    }

    public static void ClearOnlineMaganizeExtent()
    {
        lock (_lockOnlineMaganize)
        {
            _allOnlineMaganizes.Clear();
            _nextId = 1;
        }
    }

    public static void SaveOnlineMaganizesToFile(string filePath)
    {
        lock (_lockOnlineMaganize)
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNameCaseInsensitive = true
            };

            var json = JsonSerializer.Serialize(_allOnlineMaganizes, options);
            File.WriteAllText(filePath, json);
        }
    }

    public static void LoadOnlineMaganizesFromFile(string filePath)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException($"File not found: {filePath}");

        lock (_lockOnlineMaganize)
        {
            var json = File.ReadAllText(filePath);
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var onlineMaganizeList = JsonSerializer.Deserialize<List<OnlineMaganize>>(json, options);

            if (onlineMaganizeList != null)
            {
                ClearOnlineMaganizeExtent();

                foreach (var onlineMaganize in onlineMaganizeList)
                {
                    AddOnlineMaganize(onlineMaganize);
                }
            }
        }
    }
}
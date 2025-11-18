using System.Collections.Concurrent;
using System.Text.Json;

namespace byt_library.Domain.Services;

public class JsonPersistenceService : IPersistenceService
{
    private readonly string _baseDirectory;
    private readonly ConcurrentDictionary<Type, object> _locks;
    private readonly JsonSerializerOptions _serializerOptions;

    public JsonPersistenceService(string baseDirectory = "data")
    {
        _baseDirectory = baseDirectory;
        _locks = new ConcurrentDictionary<Type, object>();
        _serializerOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNameCaseInsensitive = true
        };

        if (!Directory.Exists(_baseDirectory))
        {
            Directory.CreateDirectory(_baseDirectory);
        }
    }

    public void Save<T>(List<T> items) where T : class
    {
        var lockObj = _locks.GetOrAdd(typeof(T), _ => new object());
        var filePath = GetFilePath<T>();

        lock (lockObj)
        {
            var json = JsonSerializer.Serialize(items, _serializerOptions);
            File.WriteAllText(filePath, json);
        }
    }

    public List<T> Load<T>() where T : class
    {
        var lockObj = _locks.GetOrAdd(typeof(T), _ => new object());
        var filePath = GetFilePath<T>();

        if (!File.Exists(filePath))
            throw new FileNotFoundException($"File not found: {filePath}");

        lock (lockObj)
        {
            var json = File.ReadAllText(filePath);
            var items = JsonSerializer.Deserialize<List<T>>(json, _serializerOptions);
            return items ?? new List<T>();
        }
    }

    public void SaveAll(Dictionary<Type, object> entities)
    {
        foreach (var kvp in entities)
        {
            var type = kvp.Key;
            var items = kvp.Value;
            var saveMethod = GetType().GetMethod(nameof(Save))?.MakeGenericMethod(type);
            saveMethod?.Invoke(this, new[] { items });
        }
    }

    public Dictionary<Type, List<object>> LoadAll(params Type[] types)
    {
        var result = new Dictionary<Type, List<object>>();

        foreach (var type in types)
        {
            var loadMethod = GetType().GetMethod(nameof(Load))?.MakeGenericMethod(type);
            var items = loadMethod?.Invoke(this, null);

            if (items != null)
            {
                result[type] = ((System.Collections.IEnumerable)items).Cast<object>().ToList();
            }
        }

        return result;
    }

    private string GetFilePath<T>()
    {
        var typeName = typeof(T).Name;
        return Path.Combine(_baseDirectory, $"{typeName}.json");
    }
}

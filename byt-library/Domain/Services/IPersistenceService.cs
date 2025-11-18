namespace byt_library.Domain.Services;

public interface IPersistenceService
{
    void Save<T>(List<T> items) where T : class;
    List<T> Load<T>() where T : class;
    void SaveAll(Dictionary<Type, object> entities);
    Dictionary<Type, List<object>> LoadAll(params Type[] types);
}

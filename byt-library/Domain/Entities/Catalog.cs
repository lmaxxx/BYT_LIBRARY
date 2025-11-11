namespace byt_library.Domain.Entities;

public class Catalog
{
    public string Name { get; set; }
    public Dictionary<string, IResource> resources { get; }

    public void AddResource(IResource resource)
    {
    }
}
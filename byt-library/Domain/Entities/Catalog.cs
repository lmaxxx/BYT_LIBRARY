using byt_library.Domain.Interfaces;
namespace byt_library.Domain.Entities;

public class Catalog
{
    public string Name { get; set; }
    public Dictionary<string, IResource> resources { get; }

    public Catalog(string name)
    {
        Name = name;
        resources = new Dictionary<string, IResource>();
    }

    public void AddResource(IResource resource)
    {
        resources.Add(resource.Title, resource);
    }
}
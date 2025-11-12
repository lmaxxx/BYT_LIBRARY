using byt_library.Domain.Interfaces;

namespace byt_library.Domain.Entities;

public class OnlineMagazine : IDigitalResource
{
    private static List<OnlineMagazine> OnlineMagazines = new List<OnlineMagazine>();

    public OnlineMagazine(string pageLink, string title, string description, int size, string link)
    {
        PageLink = pageLink;
        Title = title;
        Description = description;
        Size = size;
        Link = link;
        OnlineMagazines.Add(this);
    }
    
    public string PageLink { get; set; }
    public required string Title { get; set; }
    public required string Description { get; set; }
    public int Size { get; set; }
    public required string Link { get; set; }
    public required List<Translation> Translations { get; set; }
}
using byt_library.Domain.Enums;
using byt_library.Domain.Interfaces;

namespace byt_library.Domain.Entities;

public class Newspaper : IPrintedResource
{
    private static List<Newspaper> Newspapers = new List<Newspaper>();

    public Newspaper(string publisher, string title, string description, CoverType coverType, int quantity)
    {
        Publisher = publisher;
        Title = title;
        Description = description;
        CoverType = coverType;
        Quantity = quantity;
        Newspapers.Add(this);
    }
    
    public string Publisher { get; set; }
    public required string Title { get; set; }
    public required string Description { get; set; }
    public required CoverType CoverType { get; set; }
    public int Quantity { get; set; }
}
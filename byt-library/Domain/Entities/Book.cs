using byt_library.Domain.Enums;
using byt_library.Domain.Interfaces;

namespace byt_library.Domain.Entities;

public class Book : IDigitalResource, IPrintedResource
{
    private static List<Book> Books = new List<Book>();

    public Book(string ISBN, bool hasAudio, string title, string description, CoverType coverType, int quantity, int size, string link)
    {
        ISBN = ISBN;
        HasAudio = hasAudio;
        Title = title;
        Description = description;
        CoverType = coverType;
        Quantity = quantity;
        Size = size;
        Link = link;
        Books.Add(this);
    }
    
    public string IBSN { get; set; }
    public bool HasAudio { get; set; }
    public required string Title { get; set; }
    public required string Description { get; set; }
    public int Size { get; set; }
    public required string Link { get; set; }
    public required List<Translation> Translations { get; set; }
    public required CoverType CoverType { get; set; }
    public int Quantity { get; set; }
}
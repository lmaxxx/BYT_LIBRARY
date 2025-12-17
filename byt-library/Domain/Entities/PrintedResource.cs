using System.Text.Json.Serialization;
using byt_library.Domain.Enums;
using byt_library.Domain.Interfaces;

namespace byt_library.Domain.Entities;

public class PrintedResource : IPrintedResource
{
    [JsonInclude]
    private readonly Resource _resource;
    
    public CoverType CoverType { get; set; }
    public int Quantity { get; set; }

    [JsonConstructor]
    public PrintedResource(Resource resource, CoverType coverType, int quantity)
    {
        Quantity = quantity;
        CoverType = coverType;
        _resource = resource;
        _resource.AssignPrintedResource(this);
    }
}
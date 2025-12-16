using System.Text.Json.Serialization;
using byt_library.Domain.Enums;
using byt_library.Domain.Interfaces;

namespace byt_library.Domain.Entities;

public class PrintedResource(Resource resource, CoverType coverType, int quantity) : IPrintedResource
{
    [JsonInclude]
    private readonly Resource _resource = resource;
    
    public CoverType CoverType { get; set; } = coverType;
    public int Quantity { get; set; } = quantity;
}
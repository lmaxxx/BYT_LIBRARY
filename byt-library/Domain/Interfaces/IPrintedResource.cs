using byt_library.Domain.Enums;

namespace byt_library.Domain.Interfaces;

public interface IPrintedResource : IResource
{
    public CoverType CoverType { get; set; }
    public int Quantity { get; set; }
}
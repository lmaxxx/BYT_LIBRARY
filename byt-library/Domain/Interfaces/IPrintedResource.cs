using byt_library.Domain.Enums;
using byt_library.Domain.Entities;

namespace byt_library.Domain.Interfaces;

public interface IPrintedResource
{
    public CoverType CoverType { get; set; }
    public int Quantity { get; set; }

    public Resource GetResource();
}
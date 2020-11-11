using System;

namespace ResourceAllocationAuction.Models
{
    public interface INode : IEquatable<INode>
    {
        int Id { get; }
        double Production { get; }
    }
}

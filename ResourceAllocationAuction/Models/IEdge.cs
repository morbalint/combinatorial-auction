using System;

namespace ResourceAllocationAuction.Models
{
    public interface IEdge : IEquatable<IEdge>
    {
        Capacity Capacity { get; }
        INode From { get; }
        INode To { get; }
    }
}

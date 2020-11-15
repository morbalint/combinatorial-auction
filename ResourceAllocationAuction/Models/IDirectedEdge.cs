using System;

namespace ResourceAllocationAuction.Models
{
    public interface IDirectedEdge : IEquatable<IDirectedEdge>
    {
        IEdge Edge { get; }

        Direction Direction { get; }

        double Capacity { get; }

        double SignedCapacity { get; }
    }
}

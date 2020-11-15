using System;

namespace ResourceAllocationAuction.Models
{
    public record Capacity(double Positive, double Negative)
    {
        public static Capacity create(double symetric) => new Capacity(symetric, symetric);
    };

    public record Edge(INode From, INode To, Capacity Capacity) : IEdge
    {
        public bool Equals(IEdge? other) => Equals((object?)other);

        public override int GetHashCode() => HashCode.Combine(this.From, this.To);

        public override string ToString() => $"Edge from {From.Id} => to {To.Id}";
    }
}

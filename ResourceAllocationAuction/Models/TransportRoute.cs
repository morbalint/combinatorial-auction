using System.Collections.Immutable;

namespace ResourceAllocationAuction.Models
{
    public record TransportRoute : Route, ITransportRoute
    {
        public double Quantity { get; init; }

        public double UnitPrice { get; init; }

        public TransportRoute(IPlayer Player, ImmutableArray<IDirectedEdge> Edges) : base(Player, Edges)
        {
        }

        public TransportRoute(IPlayer Player, ImmutableArray<IDirectedEdge> Edges, double quantity, double unitPrice)
            : base(Player, Edges)
        => (Quantity, UnitPrice) = (quantity, unitPrice);
    }
}

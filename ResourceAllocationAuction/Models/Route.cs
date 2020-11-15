using System.Collections.Immutable;
using System.Linq;

namespace ResourceAllocationAuction.Models
{
    public record Route(IPlayer Player, ImmutableArray<IDirectedEdge> Edges) : IRoute
    {
        public override string ToString() => Edges.Length switch
        {
            0 => "Empty route",
            1 => $"{Edges[0]}",
            _ => @$"{Edges[0]} => {
                string.Join(" => ",
                    Edges
                    .Skip(1)
                    .Select(e => e.Direction is Direction.Positive ? e.Edge.To.Id : e.Edge.From.Id))}"
        };
    }
}

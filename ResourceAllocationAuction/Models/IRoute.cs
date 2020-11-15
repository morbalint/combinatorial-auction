using System.Collections.Immutable;

namespace ResourceAllocationAuction.Models
{
    public interface IRoute
    {
        ImmutableArray<IDirectedEdge> Edges { get; }

        IPlayer Player { get; }
    }
}

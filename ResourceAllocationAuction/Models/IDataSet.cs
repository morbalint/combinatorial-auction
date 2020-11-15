using System.Collections.Immutable;

namespace ResourceAllocationAuction.Models
{
    public interface IDataSet
    {
        ImmutableArray<IDemand> Demands { get; }
        ImmutableArray<IEdge> Edges { get; }
        ImmutableArray<INode> Nodes { get; }
        ImmutableArray<IPlayer> Players { get; }
        ImmutableArray<IRoute> Routes { get; }
        double SourcePrice { get; }
        ImmutableArray<ITransferPrice> TransferPrices { get; }
    }
}

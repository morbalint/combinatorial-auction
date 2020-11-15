using System.Collections.Immutable;

namespace ResourceAllocationAuction.Models
{
    public class DataSet : IDataSet
    {
        public ImmutableArray<INode> Nodes { get; init; }

        public ImmutableArray<IEdge> Edges { get; init; }

        public ImmutableArray<IPlayer> Players { get; init; }

        public ImmutableArray<IDemand> Demands { get; init; }

        public ImmutableArray<ITransferPrice> TransferPrices { get; init; }

        public double SourcePrice { get; init; }

        public ImmutableArray<IRoute> Routes { get; init; }
    }
}

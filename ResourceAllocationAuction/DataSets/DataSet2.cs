using ResourceAllocationAuction.Models;

using System.Collections.Immutable;
using System.Linq;

namespace ResourceAllocationAuction.DataSets
{
    public static class DataSet2
    {
        public static ImmutableArray<INode> Nodes { get; } = Enumerable
            .Range(1, 4)
            .Select<int, INode>(i => new Node(i))
            .ToImmutableArray();

        public static ImmutableArray<IEdge> Edges { get; } = new()
        {
            new Edge(Nodes[1], Nodes[0], new Capacity(80, 80)), // 0
            new Edge(Nodes[2], Nodes[0], new Capacity(75, 75)), // 1
            new Edge(Nodes[3], Nodes[0], new Capacity(70, 70)), // 2
            new Edge(Nodes[2], Nodes[1], new Capacity(60, 60)), // 3
            new Edge(Nodes[3], Nodes[1], new Capacity(60, 60)), // 4
            new Edge(Nodes[3], Nodes[2], new Capacity(60, 60)), // 5
        };

        public static ImmutableArray<IPlayer> Players { get; } = Enumerable
            .Range(0, 4)
            .Select<int, IPlayer>(i => new Player(i, Nodes[i]))
            .ToImmutableArray();

        public static ImmutableArray<ITransferPrice> TransferPrices { get; } = new()
        {
            new TransferPrice(Edges[0], 9),
            new TransferPrice(Edges[1], 8),
            new TransferPrice(Edges[2], 11),
            new TransferPrice(Edges[3], 4),
            new TransferPrice(Edges[4], 4.5),
            new TransferPrice(Edges[5], 5),
        };

        public const double SOURCE_PRICE = 23;

        public static ImmutableArray<IDemand> Demands { get; } = new()
        {
            new Demand(Players[1], 0, 50, 47),
            new Demand(Players[1], 50, 90, 39),
            new Demand(Players[1], 90, 125, 30),

            new Demand(Players[2], 0, 40, 46),
            new Demand(Players[2], 40, 85, 38),
            new Demand(Players[2], 85, 120, 32),

            new Demand(Players[3], 0, 50, 53),
            new Demand(Players[3], 50, 85, 49),
            new Demand(Players[3], 85, 130, 36),
        };

        // TODO: This should be calculated
        public static ImmutableArray<IRoute> Routes { get; } = new()
        {
            new Route(Players[1], new() { new DirectedEdge(Edges[0], Direction.Negative) }),
            new Route(Players[1], new()
            {
                new DirectedEdge(Edges[1], Direction.Negative),
                new DirectedEdge(Edges[3], Direction.Positive),
            }),
            new Route(Players[1], new()
            {
                new DirectedEdge(Edges[2], Direction.Negative),
                new DirectedEdge(Edges[4], Direction.Positive),
            }),
            new Route(Players[1], new()
            {
                new DirectedEdge(Edges[1], Direction.Negative),
                new DirectedEdge(Edges[5], Direction.Negative),
                new DirectedEdge(Edges[4], Direction.Positive),
            }),
            new Route(Players[1], new()
            {
                new DirectedEdge(Edges[2], Direction.Negative),
                new DirectedEdge(Edges[5], Direction.Positive),
                new DirectedEdge(Edges[3], Direction.Positive),
            }),

            new Route(Players[2], new() { new DirectedEdge(Edges[1], Direction.Negative) }),
            new Route(Players[2], new()
            {
                new DirectedEdge(Edges[0], Direction.Negative),
                new DirectedEdge(Edges[3], Direction.Negative),
            }),
            new Route(Players[2], new()
            {
                new DirectedEdge(Edges[2], Direction.Negative),
                new DirectedEdge(Edges[5], Direction.Positive),
            }),
            new Route(Players[2], new()
            {
                new DirectedEdge(Edges[2], Direction.Negative),
                new DirectedEdge(Edges[4], Direction.Positive),
                new DirectedEdge(Edges[3], Direction.Negative),
            }),
            new Route(Players[2], new()
            {
                new DirectedEdge(Edges[0], Direction.Negative),
                new DirectedEdge(Edges[4], Direction.Negative),
                new DirectedEdge(Edges[5], Direction.Positive),
            }),

            new Route(Players[3], new() { new DirectedEdge(Edges[2], Direction.Negative) }),
            new Route(Players[3], new()
            {
                new DirectedEdge(Edges[0], Direction.Negative),
                new DirectedEdge(Edges[4], Direction.Negative),
            }),
            new Route(Players[3], new()
            {
                new DirectedEdge(Edges[1], Direction.Negative),
                new DirectedEdge(Edges[5], Direction.Negative),
            }),
            new Route(Players[3], new()
            {
                new DirectedEdge(Edges[1], Direction.Negative),
                new DirectedEdge(Edges[3], Direction.Positive),
                new DirectedEdge(Edges[4], Direction.Negative),
            }),
            new Route(Players[3], new()
            {
                new DirectedEdge(Edges[0], Direction.Negative),
                new DirectedEdge(Edges[3], Direction.Negative),
                new DirectedEdge(Edges[5], Direction.Negative),
            }),
        };

        public static DataSet DataSet { get; } = new()
        {
            Nodes = Nodes,
            Edges = Edges,
            Players = Players,
            Demands = Demands,
            SourcePrice = SOURCE_PRICE,
            TransferPrices = TransferPrices,
            Routes = Routes,
        };
    }
}

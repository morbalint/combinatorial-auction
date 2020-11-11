namespace ResourceAllocationAuction.Models
{
    public class DirectedEdge
    {
        public DirectedEdge(Edge edge, Direction direction)
        {
            this.Edge = edge;
            this.Direction = direction;
        }

        public Edge Edge { get; }

        public Direction Direction { get; }
    }
}

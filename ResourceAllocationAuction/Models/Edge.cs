namespace ResourceAllocationAuction.Models
{


    public class Edge : IEdge
    {
        public Edge(int id, INode from, INode to, Capacity capacity)
        {
            this.Id = id;
            this.From = from;
            this.To = to;
            this.Capacity = capacity;
        }

        public int Id { get; }

        public INode From { get; }

        public INode To { get; }

        public Capacity Capacity { get; }
    }
}

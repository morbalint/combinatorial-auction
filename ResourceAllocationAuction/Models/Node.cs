namespace ResourceAllocationAuction.Models
{
    public record Node(int Id) : INode
    {
        public override string ToString() => $"Node_{Id}";

        public bool Equals(INode? other) => other is INode && other.Id == Id;
    }
}

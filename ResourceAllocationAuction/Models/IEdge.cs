namespace ResourceAllocationAuction.Models
{
    public interface IEdge
    {
        Capacity Capacity { get; }
        INode From { get; }
        int Id { get; }
        INode To { get; }
    }
}
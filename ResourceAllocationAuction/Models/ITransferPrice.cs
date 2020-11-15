namespace ResourceAllocationAuction.Models
{
    public interface ITransferPrice
    {
        IEdge OnEdge { get; }
        double Price { get; }
    }
}

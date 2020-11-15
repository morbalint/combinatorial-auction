namespace ResourceAllocationAuction.Models
{
    public record TransferPrice(IEdge OnEdge, double Price) : ITransferPrice;
}

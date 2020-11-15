namespace ResourceAllocationAuction.Models
{
    public interface ITransportRoute : IRoute
    {
        double Quantity { get; }

        double UnitPrice { get; }
    }
}

namespace ResourceAllocationAuction.Models
{
    public interface IDemand
    {
        double FromAmount { get; }
        IPlayer Player { get; }
        double Price { get; }
        double ToAmount { get; }
    }
}

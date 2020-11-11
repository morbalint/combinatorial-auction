namespace ResourceAllocationAuction.Models
{
    public interface IDemand
    {
        double FromAmount { get; set; }
        IPlayer Player { get; }
        double Price { get; set; }
        double ToAmount { get; set; }
    }
}
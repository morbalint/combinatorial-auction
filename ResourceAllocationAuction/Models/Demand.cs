namespace ResourceAllocationAuction.Models
{
    public record Demand(IPlayer Player, double FromAmount, double ToAmount, double Price) : IDemand;
}

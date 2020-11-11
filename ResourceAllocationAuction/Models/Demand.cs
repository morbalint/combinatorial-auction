namespace ResourceAllocationAuction.Models
{
    public class Demand : IDemand
    {
        public Demand(IPlayer player) => this.Player = player;

        public IPlayer Player { get; }

        public double FromAmount { get; set; }

        public double ToAmount { get; set; }

        public double Price { get; set; }
    }
}

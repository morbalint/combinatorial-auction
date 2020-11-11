namespace ResourceAllocationAuction.Models
{
    public class Player : IPlayer
    {
        public Player(int id, INode home)
        {
            this.Id = id;
            this.Home = home;
        }

        public int Id { get; }

        public INode Home { get; }
    }
}

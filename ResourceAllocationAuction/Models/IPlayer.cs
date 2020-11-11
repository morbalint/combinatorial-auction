namespace ResourceAllocationAuction.Models
{
    public interface IPlayer
    {
        INode Home { get; }
        int Id { get; }
    }
}
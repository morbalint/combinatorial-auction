namespace ResourceAllocationAuction.Models
{
    public record Player(int Id, INode Home) : IPlayer
    {
        public bool Equals(IPlayer? other) => Equals((object?)other);

        public override int GetHashCode() => Id;

        public override string ToString() => $"Player_{Id}";
    }
}

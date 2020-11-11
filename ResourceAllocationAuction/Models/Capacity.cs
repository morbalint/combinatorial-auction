namespace ResourceAllocationAuction.Models
{
    public struct Capacity
    {
        public double Positive { get; set; }

        public double Negative { get; set; }

        public override bool Equals(object? obj) => obj switch
        {
            Capacity other => other.Negative == Negative && other.Positive == Positive,
            _ => false,
        };

        public override int GetHashCode() => -(Negative.GetHashCode()) ^ Positive.GetHashCode();

        public static bool operator ==(Capacity left, Capacity right) => left.Equals(right);

        public static bool operator !=(Capacity left, Capacity right) => !(left == right);
    }
}

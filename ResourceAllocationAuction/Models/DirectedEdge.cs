namespace ResourceAllocationAuction.Models
{
    public record DirectedEdge(IEdge Edge, Direction Direction) : IDirectedEdge
    {
        public double Capacity => Direction switch
        {
            Direction.Positive => Edge.Capacity.Positive,
            Direction.Negative => Edge.Capacity.Negative,
            _ => throw EnumException,
        };

        public double SignedCapacity => Direction switch
        {
            Direction.Positive => Edge.Capacity.Positive,
            Direction.Negative => -Edge.Capacity.Negative,
            _ => throw EnumException,
        };

        public override string ToString() => Direction switch
        {
            Direction.Positive => $"{Edge.From.Id} => {Edge.To.Id}",
            Direction.Negative => $"{Edge.To.Id} => {Edge.From.Id}",
            _ => throw EnumException,
        };

        public bool Equals(IDirectedEdge? other) => other is IDirectedEdge && Equals((object)this);

        private NotSupportedEnumValueException<Direction> EnumException => new NotSupportedEnumValueException<Direction>(Direction);
    }
}

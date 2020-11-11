namespace ResourceAllocationAuction.Models
{
    public class Node : INode
    {
        public Node(int id, double production)
        {
            this.Id = id;
            this.Production = production;
        }

        public int Id { get; }

        /// <summary>
        /// Base resource production of node, which is to be transported.
        /// + means production, - means consumption.
        /// Serves as an absolute limit in calculations (currently no calculation uses it).
        /// </summary>
        public double Production { get; }

        public override string ToString() => $"Node_{Id}";

        // override object.Equals
        public override bool Equals(object? obj) => obj switch
        {
            Node other => other.Id == Id,
            _ => false,
        };

        // override object.GetHashCode
        public override int GetHashCode() => Id;

        public bool Equals(INode? other) => other switch
        {
            null => false,
            INode real => real.Id == Id,
        };
    }
}

namespace NDP
{
    public interface IEdge
    {
        double? Capacity { get; }
        double CostPerFlowUnit { get; }
        double DesignCost { get; }
        double Flow { get; set; }
        INode FromNode { get; }
        bool IsUsed { get; set; }
        INode ToNode { get; }

        string ToString();
    }
}
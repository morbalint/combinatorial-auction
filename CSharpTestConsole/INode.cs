namespace NDP
{
    public interface INode
    {
        double Demand { get; }
        string Name { get; }

        string ToString();
    }
}
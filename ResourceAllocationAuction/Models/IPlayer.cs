using System;

namespace ResourceAllocationAuction.Models
{
    public interface IPlayer : IEquatable<IPlayer>
    {
        INode Home { get; }

        int Id { get; }
    }
}

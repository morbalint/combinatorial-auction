using ResourceAllocationAuction.Models;

using System.Collections.Generic;
using System.Linq;

namespace ResourceAllocationAuction.ACA
{
    public class ACA
    {
        public static (TransportRoute, List<Demand>) bid(List<Demand> demands, TransportRoute route)
        {
            var is_finished = !demands.Select(d => d.Price >= route.UnitPrice).Any();

            if (is_finished)
            {
                var qq = route with { Quantity = 0 };
                return (route with { Quantity = 0 }, demands);
            }

            var current_demand_capacity = demands[0].ToAmount - demands[0].FromAmount;
            var spill = route.Quantity - current_demand_capacity;

            if (spill <= 0)
            {
                //var qq = demands[0] with { FromAmount = demands[0].FromAmount + route.Quantity };
                //var ff = demands[1..3];
                //return (route, demands[1..3]);
            }

            return default;
        }

        public static ITransportRoute[] make_all_bids_for_a_player(IList<IDemand> demands, IEnumerable<ITransportRoute> routes)
        {
            return default;
        }

        public static ITransportRoute[] make_bids(IEnumerable<IDemand> demands, IEnumerable<ITransportRoute> pricedRoutes)
        {
            return default;
        }
    }
}

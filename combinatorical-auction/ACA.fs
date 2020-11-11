module CombinatorialAuction.ACA

open CombinatorialAuction.Models
open CombinatorialAuction.DataSet2
open CombinatorialAuction.Bidding

type ACA_Bid = TransportRoute

let reference_priced_routes = List.map (priceSingleRouteWithSource edgePrices prices) routes

// assign integrated unit prices for all demands
let demand_integrated_unit_prices demands =
    let transform price_so_far (demand: Demand)  =
        let amount = demand.toAmount - demand.fromAmount
        let price_area = amount * demand.price
        let total_price_area = price_area + price_so_far
        let new_average_price = total_price_area / demand.toAmount
        ( { demand with price = new_average_price }, total_price_area )
    demands
        |> Seq.mapFold transform 0.
        |> fst

let rec bid (demands: Demand list) (route: TransportRoute) =
    let is_finished = demands |> Seq.exists (fun d -> d.price >= route.unitPrice) |> not
    if is_finished then
        { route with capacity = 0. }, demands
    else
        let current_demand_capacity = demands.Head.toAmount - demands.Head.fromAmount
        let spill = route.capacity - current_demand_capacity
        if (spill <= 0.) then
            route, { demands.Head with fromAmount = demands.Head.fromAmount + route.capacity } :: demands.Tail
        else
            let bid2,dem2 = bid demands.Tail { route with capacity = spill }
            {bid2 with capacity= bid2.capacity + current_demand_capacity}, dem2

let make_all_bids_for_a_player (demands: Demand list, routes: TransportRoute seq) =
    let test =
        routes
        |> Seq.sortBy ( fun pr -> pr.unitPrice )
        |> Seq.mapFold bid demands
        |> fst
        |> Seq.filter (fun b -> b.capacity > 0.)
        |> Seq.toArray
    test

/// make bids for demands
let make_bids (demands: Demand seq) (pricedRoutes: TransportRoute seq) =
    let aca_demands =
        demands
        // |> demand_integrated_unit_prices
        |> Seq.groupBy (fun d -> d.player)
        |> Seq.map (fun (k,s) -> k,Seq.toList s)
        |> Seq.toArray
    let prg = pricedRoutes |> Seq.groupBy (fun r -> r.player) |> Seq.toArray

    let qq =
        (Seq.allPairs aca_demands prg)
        |> Seq.filter (fun ((p1,_),(p2,_)) -> p1 = p2)
        |> Seq.map (fun ((_,d),(_,r)) -> d,r) // delete line
        |> Seq.collect make_all_bids_for_a_player
        |> Seq.toArray
    qq

/// calculates the closed edges from the list of bids.
let closed_edges (bids: TransportRoute seq) =
    bids
    |> Seq.collect ( fun b -> b.edges |> Seq.map ( fun (e,d) ->
        match d with
        | Direction.Negative -> (e, -b.capacity)
        | _ -> (e, b.capacity)
    ) )
    |> Seq.groupBy fst
    |> Seq.map ( fun (e,quantities) ->
        let quantity = quantities |> Seq.sumBy snd
        let on_going = quantity > e.capacityPositive || quantity < -e.capacityNegative
        (e, on_going)
    )
    |> Seq.filter (snd >> not)
    |> Seq.map fst

/// increments price for a new round
let increment_price (increment: float) (closed_edges : Edge Set) (bid: TransportRoute) =
    let number_of_open_edges =
        bid.edges
        |> Seq.map fst
        |> Seq.filter ( fun e -> not (Set.contains e closed_edges))
        |> Seq.length
    { bid with unitPrice = bid.unitPrice + increment * (float number_of_open_edges ) }

let assign_price_to_closed_edge (initialPrices: TransferPrice list) (total_increment: float) (closed_edge: Edge) =
    let initial =
        initialPrices
        |> Seq.find ( fun tp -> tp.onEdge = closed_edge )
    closed_edge,initial.price + total_increment

let decrement_on_route (closed_edges_with_price: (Edge * float) seq) (route: TransportRoute) =
    let route_edges = route.edges |> Seq.map fst |> set
    closed_edges_with_price
    |> Seq.filter (fun (e,_) -> route_edges.Contains e)
    |> Seq.sumBy snd

/// closes edges on a route by calculating their current price and removing it from the route price.
let close_edges_on_route decrement (route: TransportRoute) =
    { route with unitPrice = route.unitPrice - decrement }

// updates demand of a single player
let rec update_demands decrement (demands : Demand list) =
    match demands with
    | act::next::tail -> 
        let act_amount = act.toAmount - act.fromAmount
        let surplus = act_amount * (act.price - next.price)
        if surplus >= decrement
        then 
            let updated_price = next.price + ((surplus - decrement) / act_amount) 
            { act with price = updated_price } :: next :: tail
        else 
            let unified = { next with fromAmount = act.fromAmount }
            update_demands (decrement - surplus) ( unified :: tail )
    | last::[] ->
        let amount = last.toAmount - last.fromAmount
        let updated_price = (amount * last.price - decrement) / amount
        assert (updated_price >= 0.)
        [{last with price = updated_price}]
    | [] -> demands

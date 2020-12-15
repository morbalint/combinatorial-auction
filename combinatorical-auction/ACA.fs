module CombinatorialAuction.ACA

open CombinatorialAuction.Models

type ResultSegment = {
    player: Player;
    edge: Edge;
    capacity: float;
    unitPrice: float;
}

let rec bid (demands: Demand list) (route: TransportRoute) =
    match demands with
    | demand::tail when demand.price > route.unitPrice ->
        let current_demand_capacity = demand.toAmount - demand.fromAmount
        let spill = route.capacity - current_demand_capacity
        if (spill <= 0.) then
            route, { demand with fromAmount = demand.fromAmount + route.capacity } :: tail
        else
            let bid2,dem2 = bid tail { route with capacity = spill }
            {bid2 with capacity = (bid2.capacity + current_demand_capacity)}, dem2
    | _ -> { route with capacity = 0. }, demands

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

    let bids =
        (Seq.allPairs aca_demands prg)
        |> Seq.filter (fun ((p1,_),(p2,_)) -> p1 = p2)
        |> Seq.map (fun ((_,d),(_,r)) -> d,r) // delete line
        |> Seq.collect make_all_bids_for_a_player
        |> Seq.toArray
    bids

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
        let on_going = quantity > e.capacity || quantity < -e.capacity
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
let rec update_demands decrement demands =
    match demands with
    | act::tail ->
        let currentArea = (act.toAmount - act.fromAmount) * act.price
        if currentArea <= decrement then
            update_demands (decrement - currentArea) tail
        else
            let dividePoint = act.fromAmount + (decrement / act.price)
            { act with fromAmount = dividePoint } :: tail
    | [] -> demands

let runACA demands routes edge_prices increment_per_step =

    let initial_bids = make_bids demands routes
    let my_increment_price_function = increment_price increment_per_step

    let rec loop already_closed_edges total_increment bids previous_results =

        let closed_edges_next =
            closed_edges bids
            |> set
            |> Set.union already_closed_edges

        let newly_closed_edges = closed_edges_next - already_closed_edges

        // previous bids are the new routes!
        // close off everything you didn't bid in the previous step

        let priced_closed_edges =
            newly_closed_edges
            |> Seq.map (assign_price_to_closed_edge edge_prices total_increment)
            |> Seq.toArray

        let partial_results =
            bids
            |> Seq.collect (fun bid ->
                bid.edges
                |> Seq.map fst
                |> set
                |> Set.intersect newly_closed_edges
                |> Seq.map (fun e -> {
                    player = bid.player;
                    edge = e;
                    capacity = bid.capacity;
                    unitPrice = bid.unitPrice }))

        let results = Seq.append previous_results partial_results

        let routes_next =
            bids
            |> Seq.map ( decrement_on_route priced_closed_edges )
            |> Seq.zip bids
            |> Seq.map (
                (fun (r,dec) -> close_edges_on_route dec r) >>
                (my_increment_price_function closed_edges_next ))
            |> Seq.toList

        let bids_next = make_bids demands routes_next

        // is it done?
        let edges =
            bids_next
            |> Seq.collect (fun b -> b.edges |> Seq.map fst )
            |> set

        let open_edges = edges - closed_edges_next

        if not (Set.isEmpty open_edges) then
            loop closed_edges_next (total_increment + increment_per_step) bids_next results
        else
            results

    loop Set.empty 0. initial_bids []

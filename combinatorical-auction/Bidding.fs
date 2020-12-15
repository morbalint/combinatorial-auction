module CombinatorialAuction.Bidding

open CombinatorialAuction.Models
open CombinatorialAuction.DataSet2

let private calcRouteCapacity (edges: (Edge * Direction) list) =
    edges
    |> List.map (fun (e,d) ->
        match d with
        | Direction.Positive -> e.capacity
        | Direction.Negative -> e.capacity
        | _ -> failwith (sprintf "unknown direction: '%A'" d))
    |> List.min

let private calcRoutePrice (edgePrices: TransferPrice list) (route: Route) =
    route.edges
    |> List.sumBy (fun (e,_) ->
        edgePrices
        |> List.choose (fun p -> if p.onEdge = e then Some p.price else None)
        |> List.head)

let public priceSingeRoute (edgePrices: TransferPrice list) (route: Route) =
    {
        id = route.id;
        player = route.player;
        edges = route.edges;
        capacity = calcRouteCapacity route.edges
        unitPrice = calcRoutePrice edgePrices route
    }

let public priceSingleRouteWithSource (edgePrices: TransferPrice list) (route: Route) =
    {
        id = route.id;
        player = route.player;
        edges = route.edges;
        capacity = calcRouteCapacity route.edges
        unitPrice = (calcRoutePrice edgePrices route) + dataset.sourcePrice
    }

let private getSourceNodeFromRoute route =
    let (firstEdge, firstDirection) = route.edges.[0];
    match firstDirection with
    | Direction.Positive -> firstEdge.fromNode
    | Direction.Negative -> firstEdge.toNode
    | _ -> failwith (sprintf "unknown direction: '%A'" firstDirection)

let calcBidsForSingleRoute (demands: Demand list) route =
    let bidPieces =
        demands
        |> List.filter (fun d -> d.player = route.player)
        |> List.sortBy  (fun d -> d.fromAmount)
        |> List.map (fun d ->
            {
                route = route;
                quantity = d.toAmount;
                totalPrice = (d.toAmount - d.fromAmount) * ( d.price - route.unitPrice - dataset.sourcePrice );
            })
    let (bids : Bid list, _) =
        List.mapFold
            (fun state piece -> ( { piece with totalPrice = piece.totalPrice + state } , (piece.totalPrice + state) ) )
            0.0
            bidPieces
    bids |> List.filter (fun bid -> bid.totalPrice > 0.0)

let bidsFromPricedRoutes (demands: Demand list) routes =
    routes
    |> List.collect (calcBidsForSingleRoute demands)
    |> List.sortBy (fun bid -> bid.quantity)
    |> List.sortBy (fun bid -> bid.route.player.id)

/// TODO: remove data dependency
let getPricedRoutes () = List.map (priceSingeRoute edgePrices) routes;

/// TODO: remove data dependency
let getBids () = bidsFromPricedRoutes demands (getPricedRoutes ())

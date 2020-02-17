module CombinatorialAuction.Bidding

open CombinatorialAuction.Models
open CombinatorialAuction.Data

let private calcRouteCapacity (edges: (Edge * Direction) list) =
    edges
    |> List.map (fun (e,d) ->
        match d with
        | Positive -> e.capacityPositive
        | Negative -> e.capacityNegative)
    |> List.min

let private calcRoutePrice (edgePrices: EdgePrice list) (route: PartialRoute) =
    route.edges
    |> List.sumBy (fun (e,d) ->
        edgePrices
        |> List.choose (fun p -> if p.player = route.player && p.edge = e then Some p.price else None)
        |> List.head)

let private priceSingeRoute (edgePrices: EdgePrice list) (route: PartialRoute) =
    {
        id = route.id;
        player = route.player;
        edges = route.edges;
        capacity = calcRouteCapacity route.edges
        price = calcRoutePrice edgePrices route
    }

let private getSourceNodeFromRoute route =
    let (firstEdge, firstDirection) = route.edges.[0];
    match firstDirection with
    | Positive -> firstEdge.fromNode
    | Negative -> firstEdge.toNode

let calcSourcePriceFromPricesAndRoute prices route =
    let sourceNode = getSourceNodeFromRoute route
    let sourcePrice =
        prices
        |> List.filter (fun sp -> sp.toConsumer = route.player && sp.fromProducer = sourceNode)
        |> List.map (fun sp -> sp.price)
    match sourcePrice with
    | [] -> failwith "source price not found"
    | [head] -> head
    | _::_ -> failwith "too many source prices found"

/// TODO: remove data dependency
let calcSourcePrice = calcSourcePriceFromPricesAndRoute prices;

let calcBidsForSingleRoute (demands: Demand list) route =
    let sourcePrice = calcSourcePrice route
    let bidPieces =
        demands
        |> List.filter (fun d -> d.player = route.player)
        |> List.sortBy  (fun d -> d.fromAmount)
        |> List.map (fun d ->
            {
                route = route;
                quantity = d.toAmount;
                totalPrice = (d.toAmount - d.fromAmount) * ( d.price - route.price - sourcePrice );
            })
    let (bids : Bid list, _) =
        List.mapFold
            (fun state piece -> ( { piece with totalPrice = piece.totalPrice + state } , (state + piece.totalPrice) ) )
            0.0
            bidPieces
    bids |> List.filter (fun bid -> bid.totalPrice > 0.0)

let bidsFromPricedRoutes (demands: Demand list) routes =
    routes
    |> List.collect (calcBidsForSingleRoute demands)
    |> List.sortBy (fun bid -> bid.quantity)
    |> List.sortBy (fun bid -> bid.route.player.id)
    |> List.indexed

/// TODO: remove data dependency
let getPricedRoutes () = List.map (priceSingeRoute edgePrices) routes;

/// TODO: remove data dependency
let getBids () = bidsFromPricedRoutes demands (getPricedRoutes ())

let bid2viewModel (idx,bid) =
    {
        id = idx;
        routeId = bid.route.id;
        playerId = bid.route.player.id;
        quantity = bid.quantity;
        totalPrice = bid.totalPrice;
    }

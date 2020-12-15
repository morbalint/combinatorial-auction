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
        |> List.choose (fun p -> if p.forPlayer = route.player && p.onEdge = e then Some p.price else None)
        |> List.head)

let private selectSourcePrice (sourcePrices: SourcePrice list) (player: Player) =
    sourcePrices
    |> Seq.filter ( fun sp -> sp.toConsumer = player )
    |> Seq.map (fun sp -> sp.price)
    |> Seq.head

let public priceSingeRoute (edgePrices: TransferPrice list) (route: Route) =
    {
        id = route.id;
        player = route.player;
        edges = route.edges;
        capacity = calcRouteCapacity route.edges
        unitPrice = calcRoutePrice edgePrices route
    }

let public priceSingleRouteWithSource (edgePrices: TransferPrice list) (sourcePrices: SourcePrice list) (route: Route) =
    {
        id = route.id;
        player = route.player;
        edges = route.edges;
        capacity = calcRouteCapacity route.edges
        unitPrice = (calcRoutePrice edgePrices route) + (selectSourcePrice sourcePrices route.player)
    }

let private getSourceNodeFromRoute route =
    let (firstEdge, firstDirection) = route.edges.[0];
    match firstDirection with
    | Direction.Positive -> firstEdge.fromNode
    | Direction.Negative -> firstEdge.toNode
    | _ -> failwith (sprintf "unknown direction: '%A'" firstDirection)

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
                totalPrice = (d.toAmount - d.fromAmount) * ( d.price - route.unitPrice - sourcePrice );
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

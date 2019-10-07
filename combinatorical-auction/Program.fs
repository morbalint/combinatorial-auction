// Learn more about F# at http://fsharp.org

open System
open Newtonsoft.Json

type Node = {
    production: double;
    id: int
}

type Edge = {
    id: int;
    fromNode : Node;
    toNode : Node;
    capacityPositive : double;
    capacityNegative : double;
}

type Player = {
    id: int;
    node: Node;
}

type TransferCost = {
    onEdge: Edge;
    forPlayer: Player;
    price: double;
}

type SourcePrices = {
    fromProducer: Node;
    toConsumer: Player;
    price: double;
}

// piece of constant inverse demand curve
type Demand = {
    player: Player;
    fromAmount: double;
    toAmount: double;
    price: double;
}

type EdgePrice = {
    player: Player;
    edge: Edge;
    price: double;
}

type Direction =
    | Positive
    | Negative

type Transport = {
    player: Player;
    onEdge: Edge;
    Price: double;
    amount: double;
    direction: Direction;
}

let nodes = [
    { id = 1; production = 1000.0 };
    { id = 2; production = -95.0; };
    { id = 3; production = -100.0; };
    { id = 4; production = -100.0; };
]

let edges = [
    { id = 1; fromNode = nodes.[1]; toNode = nodes.[0]; capacityPositive = 80.0; capacityNegative = 80.0; };
    { id = 2; fromNode = nodes.[2]; toNode = nodes.[0]; capacityPositive = 75.0; capacityNegative = 75.0; };
    { id = 3; fromNode = nodes.[3]; toNode = nodes.[0]; capacityPositive = 70.0; capacityNegative = 70.0; };
    { id = 4; fromNode = nodes.[2]; toNode = nodes.[1]; capacityPositive = 60.0; capacityNegative = 60.0; };
    { id = 5; fromNode = nodes.[3]; toNode = nodes.[1]; capacityPositive = 60.0; capacityNegative = 60.0; };
    { id = 6; fromNode = nodes.[3]; toNode = nodes.[2]; capacityPositive = 60.0; capacityNegative = 60.0; };
]

let players = [
    { id = 0; node = nodes.[0] };
    { id = 1; node = nodes.[1] };
    { id = 2; node = nodes.[2] };
    { id = 3; node = nodes.[3] };
]

let sources = players |> List.map (fun p -> p.node.production > 0.0)
let consumers = players |> List.map (fun p -> p.node.production < 0.0)

let edgePrices = [
    { player = players.[1]; edge = edges.[0]; price = 9.0; }
    { player = players.[2]; edge = edges.[0]; price = 9.0; }
    { player = players.[3]; edge = edges.[0]; price = 4.0; }
    { player = players.[1]; edge = edges.[1]; price = 5.0; }
    { player = players.[2]; edge = edges.[1]; price = 8.0; }
    { player = players.[3]; edge = edges.[1]; price = 8.0; }
    { player = players.[1]; edge = edges.[2]; price = 11.0; }
    { player = players.[2]; edge = edges.[2]; price = 11.0; }
    { player = players.[3]; edge = edges.[2]; price = 11.0; }
    { player = players.[1]; edge = edges.[3]; price = 1.0; }
    { player = players.[2]; edge = edges.[3]; price = 4.0; }
    { player = players.[3]; edge = edges.[3]; price = 4.0; }
    { player = players.[1]; edge = edges.[4]; price = 4.5; }
    { player = players.[2]; edge = edges.[4]; price = 4.5; }
    { player = players.[3]; edge = edges.[4]; price = 4.5; }
    { player = players.[1]; edge = edges.[5]; price = 5.0; }
    { player = players.[2]; edge = edges.[5]; price = 1.5; }
    { player = players.[3]; edge = edges.[5]; price = 5.0; }
]

let prices = [
    { fromProducer = nodes.[0]; toConsumer = players.[1]; price = 25.0; };
    { fromProducer = nodes.[0]; toConsumer = players.[2]; price = 23.0; };
    { fromProducer = nodes.[0]; toConsumer = players.[3]; price = 20.0; };
]

let demands = [
    { player = players.[1]; fromAmount = 0.0; toAmount = 40.0; price = 47.0; };
    { player = players.[1]; fromAmount = 40.0; toAmount = 70.0; price = 39.0; }
    { player = players.[1]; fromAmount = 70.0; toAmount = 95.0; price = 30.0; }
    { player = players.[2]; fromAmount = 0.0; toAmount = 30.0; price = 46.0; }
    { player = players.[2]; fromAmount = 30.0; toAmount = 70.0; price = 38.0; }
    { player = players.[2]; fromAmount = 70.0; toAmount = 100.0; price = 32.0; }
    { player = players.[3]; fromAmount = 0.0; toAmount = 50.0; price = 53.0; }
    { player = players.[3]; fromAmount = 50.0; toAmount = 80.0; price = 49.0; }
    { player = players.[3]; fromAmount = 80.0; toAmount = 100.0; price = 36.0; }
]

type PartialRoute = {
    player: Player;
    edges: (Edge * Direction) list;
}

// TODO: this should be calculated;
let partialRoutes = [
    { player = players.[1]; edges = [ edges.[0], Negative ]; }
    { player = players.[1]; edges = [ edges.[1], Negative; edges.[3], Positive ]; }
    { player = players.[1]; edges = [ edges.[2], Negative; edges.[4], Positive ]; }
    { player = players.[1]; edges = [ edges.[1], Negative; edges.[5], Negative; edges.[4], Positive ]; }
    { player = players.[1]; edges = [ edges.[2], Negative; edges.[5], Positive; edges.[3], Positive ]; }
    { player = players.[2]; edges = [ edges.[1], Negative ]; }
    { player = players.[2]; edges = [ edges.[0], Negative; edges.[3], Negative ]; }
    { player = players.[2]; edges = [ edges.[2], Negative; edges.[5], Positive ]; }
    { player = players.[2]; edges = [ edges.[2], Negative; edges.[4], Positive; edges.[3], Negative ]; }
    { player = players.[2]; edges = [ edges.[2], Negative ]; }
    { player = players.[2]; edges = [ edges.[0], Negative; edges.[4], Negative ]; }
    { player = players.[2]; edges = [ edges.[1], Negative; edges.[5], Negative ]; }
    { player = players.[2]; edges = [ edges.[1], Negative; edges.[3], Positive; edges.[4], Negative ]; }
]

type Route = {
    player: Player;
    edges: (Edge * Direction) list;
    capacity: double;
    price: double;
}

let calcRouteCapacity (edges: (Edge * Direction) list) =
    edges
    |> List.map (fun (e,d) ->
        match d with
        | Positive -> e.capacityPositive
        | Negative -> e.capacityNegative)
    |> List.min

let calcRoutePrice (edgePrices: EdgePrice list) (route: PartialRoute) =
    route.edges
    |> List.map (fun (e,d) ->
        edgePrices
        |> List.choose (fun p -> if p.player = route.player && p.edge = e then Some p.price else None)
        |> List.head)
    |> List.sum

let priceSingeRoute (edgePrices: EdgePrice list) (route: PartialRoute) =
    {
        player = route.player;
        edges = route.edges;
        capacity = calcRouteCapacity route.edges
        price = calcRoutePrice edgePrices route
    }

type Bid = {
    route: Route;
    quantity: double;
    totalPrice: double;
}

let getSourceNodeFromRoute route =
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
    | head::[] -> head
    | _::_ -> failwith "too many source prices found"

let calcSourcePrice = calcSourcePriceFromPricesAndRoute prices;

let calcBidsForSingleRoute (demands: Demand list) route =
    let sourcePrice = calcSourcePrice route
    demands
    |> List.filter (fun d -> d.player = route.player)
    |> List.map (fun d ->
        let quantity = List.min [ route.capacity; d.toAmount - d.fromAmount ];
        {
            route = route;
            quantity = quantity;
            totalPrice = quantity * (d.price - route.price - sourcePrice );
        })
    |> List.filter (fun bid -> bid.totalPrice > 0.0)

let calcBids (demands: Demand list) routes =
    routes
    |> List.map (calcBidsForSingleRoute demands)
    |> List.concat
    |> List.sortByDescending (fun bid -> bid.totalPrice)

[<EntryPoint>]
let main argv =
    printfn "Hello World from F#!"
    let routes = List.map (priceSingeRoute edgePrices) partialRoutes;
    printfn "routes:"
    printfn "%A" routes
    let bids = calcBids demands routes
    printfn "bids:"
    printfn "%A" bids
    System.IO.File.WriteAllText("output.json", JsonConvert.SerializeObject bids)
    0 // return an integer exit code

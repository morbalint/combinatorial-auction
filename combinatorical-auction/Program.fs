// Learn more about F# at http://fsharp.org

open Newtonsoft.Json
open MathNet.Numerics
open MathNet.Numerics.Data.Matlab

type Node = {
    production: float;
    id: int
}

type Edge = {
    id: int;
    fromNode : Node;
    toNode : Node;
    capacityPositive : float;
    capacityNegative : float;
}

type Player = {
    id: int;
    node: Node;
}

type TransferCost = {
    onEdge: Edge;
    forPlayer: Player;
    price: float;
}

type SourcePrices = {
    fromProducer: Node;
    toConsumer: Player;
    price: float;
}

// piece of constant inverse demand curve
type Demand = {
    player: Player;
    fromAmount: float;
    toAmount: float;
    price: float;
}

type EdgePrice = {
    player: Player;
    edge: Edge;
    price: float;
}

type Direction =
    | Positive
    | Negative

type Transport = {
    player: Player;
    onEdge: Edge;
    Price: float;
    amount: float;
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
    { player = players.[3]; edge = edges.[4]; price = 1.5; }
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
    { player = players.[3]; fromAmount = 80.0; toAmount = 105.0; price = 36.0; }
]

type PartialRoute = {
    id: int;
    player: Player;
    edges: (Edge * Direction) list;
}

// TODO: this should be calculated;
let partialRoutes = [
    { id = 1; player = players.[1]; edges = [ edges.[0], Negative ]; }
    { id = 2; player = players.[1]; edges = [ edges.[1], Negative; edges.[3], Positive ]; }
    { id = 3; player = players.[1]; edges = [ edges.[2], Negative; edges.[4], Positive ]; }
    { id = 4; player = players.[1]; edges = [ edges.[1], Negative; edges.[5], Negative; edges.[4], Positive ]; }
    { id = 5; player = players.[1]; edges = [ edges.[2], Negative; edges.[5], Positive; edges.[3], Positive ]; }
    { id = 1; player = players.[2]; edges = [ edges.[1], Negative ]; }
    { id = 2; player = players.[2]; edges = [ edges.[0], Negative; edges.[3], Negative ]; }
    { id = 4; player = players.[2]; edges = [ edges.[2], Negative; edges.[5], Positive ]; }
    { id = 3; player = players.[2]; edges = [ edges.[2], Negative; edges.[4], Positive; edges.[3], Negative ]; }
    { id = 1; player = players.[3]; edges = [ edges.[2], Negative ]; }
    { id = 2; player = players.[3]; edges = [ edges.[0], Negative; edges.[4], Negative ]; }
    { id = 3; player = players.[3]; edges = [ edges.[1], Negative; edges.[5], Negative ]; }
    { id = 4; player = players.[3]; edges = [ edges.[1], Negative; edges.[3], Positive; edges.[4], Negative ]; }
]

type Route = {
    id: int;
    player: Player;
    edges: (Edge * Direction) list;
    capacity: float;
    price: float;
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
    |> List.sumBy (fun (e,d) ->
        edgePrices
        |> List.choose (fun p -> if p.player = route.player && p.edge = e then Some p.price else None)
        |> List.head)

let priceSingeRoute (edgePrices: EdgePrice list) (route: PartialRoute) =
    {
        id = route.id;
        player = route.player;
        edges = route.edges;
        capacity = calcRouteCapacity route.edges
        price = calcRoutePrice edgePrices route
    }

type Bid = {
    route: Route;
    quantity: float;
    totalPrice: float;
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
    | [head] -> head
    | _::_ -> failwith "too many source prices found"

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
    let (bids, _) =
        List.mapFold
            (fun state piece -> ( { piece with totalPrice = piece.totalPrice + state } , (state + piece.totalPrice) ) )
            0.0
            bidPieces
    bids |> List.filter (fun bid -> bid.totalPrice > 0.0)

let calcBids (demands: Demand list) routes =
    routes
    |> List.collect (calcBidsForSingleRoute demands)
    |> List.sortBy (fun bid -> bid.quantity)
    |> List.sortBy (fun bid -> bid.route.player.id)
    |> List.indexed

type BidViewModel = {
    id: int;
    routeId: int;
    playerId: int;
    quantity: float;
    totalPrice: float;
}

let bid2viewModel (idx,bid) =
    {
        id = idx;
        routeId = bid.route.id;
        playerId = bid.route.player.id;
        quantity = bid.quantity;
        totalPrice = bid.totalPrice;
    }

type ConstraintRow = {
    weights: float list;
    upperBound: float;
}

let createRowFromPlayerConstraints numberOfAllBids bids = 
    Array.map (fun n -> if (List.contains n bids) then 1.0 else 0.0) [|0..numberOfAllBids-1|]

let calcPlayerConstraints bids =
    let createRow = createRowFromPlayerConstraints (List.length bids)
    bids 
    |> List.groupBy (fun bid -> bid.playerId)
    |> List.map (fun (_, subBids) -> subBids |> List.map (fun x -> x.id) |> createRow )

let edgeConstraintTupleFromBids (bids: (int * Bid * Edge * Direction) list) =
    bids
    |> List.map 
        (fun (idx,bid,_,dir) ->
            match dir with
            | Positive -> (idx, bid.quantity)
            | Negative -> (idx, -bid.quantity)
        )

let createrRowFromEdgeConstraints numberOfAllBids (constraints: (int*float) list) =
    Array.map (fun n ->
            match (List.tryFind (fun (idx, _) -> n = idx) constraints) with
            | Some (_, weight) -> weight
            | None -> 0.0
    ) [| 0..numberOfAllBids |]

let calcEdgeConstraints bids = 
    let createRow = createrRowFromEdgeConstraints (List.length bids)
    bids 
    |> List.collect (fun (idx,bid) -> bid.route.edges |> List.map (fun (e,d) -> (idx, bid, e,d)))
    |> List.groupBy (fun (idx, bid, edge, direction) -> edge.id)
    |> List.collect 
        (fun (_, l) -> 
            let (_,_,edge,_) = (List.head l)
            let constraints = edgeConstraintTupleFromBids l
            let row = createRow constraints
            [ (row, edge.capacityPositive);
              ((Array.map ((*) -1.0) row), edge.capacityNegative)]
        )


[<EntryPoint>]
let main argv =
    printfn "Hello World from F#!"
    let routes = List.map (priceSingeRoute edgePrices) partialRoutes;
    if not (System.IO.Directory.Exists "out") then
        System.IO.Directory.CreateDirectory "out" |> ignore
    System.IO.File.WriteAllText("out/routes.json", JsonConvert.SerializeObject(routes))
    let bids = calcBids demands routes
    let bidsView = List.map bid2viewModel bids
    System.IO.File.WriteAllText("out/bids.json", JsonConvert.SerializeObject bidsView)
    let playerConstraints = calcPlayerConstraints bidsView
    let edgeConstraints = calcEdgeConstraints bids
    let fVector = List.map (fun x -> x.totalPrice) bidsView
    let aMatrix = List.concat [ playerConstraints; (List.map (fun (row, _) -> row) edgeConstraints) ]
    let bVector = List.concat [ (List.replicate (List.length playerConstraints) 1.0); (List.map (fun (_, ub) -> ub) edgeConstraints) ]
    System.IO.File.WriteAllText("out/f.json", JsonConvert.SerializeObject fVector)
    System.IO.File.WriteAllText("out/A.json", JsonConvert.SerializeObject aMatrix)
    System.IO.File.WriteAllText("out/b.json", JsonConvert.SerializeObject bVector)
    let f = LinearAlgebra.Matrix.Build.DenseOfColumns ( Seq.singleton ( Seq.ofList fVector ) )
    let a = LinearAlgebra.Matrix.Build.DenseOfRows (aMatrix |> Seq.map Seq.ofArray)
    let b = LinearAlgebra.Matrix.Build.DenseOfColumns ( Seq.singleton ( Seq.ofList bVector ) )
    let fPacked = MatlabWriter.Pack(f, "f")
    let aPacked = MatlabWriter.Pack(a, "A")
    let bPacked = MatlabWriter.Pack(b, "b")
    let matrices = Seq.ofList [ fPacked; aPacked; bPacked ]
    MatlabWriter.Store("out/lin_problem.mat", matrices)
    0 // return an integer exit code

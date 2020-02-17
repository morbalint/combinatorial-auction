module CombinatorialAuction.Program

// Learn more about F# at http://fsharp.org
open Newtonsoft.Json
open MathNet.Numerics
open MathNet.Numerics.Data.Matlab

open CombinatorialAuction.Models
open CombinatorialAuction.Data
open OPTANO.Modeling.Optimization
open OPTANO.Modeling.Optimization.Enums
open OPTANO.Modeling.Optimization.Solver.Gurobi810

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
    let (bids : Bid list, _) =
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

let bid2viewModel (idx,bid) =
    {
        id = idx;
        routeId = bid.route.id;
        playerId = bid.route.player.id;
        quantity = bid.quantity;
        totalPrice = bid.totalPrice;
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

let matlabExport routes bids =
    if not (System.IO.Directory.Exists "out") then
        System.IO.Directory.CreateDirectory "out" |> ignore
    System.IO.File.WriteAllText("out/routes.json", JsonConvert.SerializeObject(routes))
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

let optanoGurobiSolution bids =
    // create model
    let model = new Model ()
    // create variable
    let x = new VariableCollection<(Bid)>(
                                                model,
                                                List.map (fun (_,bid) -> bid) bids,
                                                "x",
                                                (fun bid -> sprintf "Bid by player %i on route %i" bid.route.player.id bid.route.id),
                                                (fun _ -> 0.0),
                                                (fun _ -> 1.0),
                                                (fun _ -> VariableType.Continuous))

    /// add constraints ///

    // add player constraints
    bids
        |> List.groupBy (fun (_,bid) -> bid.route.player.id)
        |> List.iter (fun (playerId, playerBids) ->
            let playerBidVariables = playerBids |> List.map (fun (_,bid) -> x.[bid])
            model.AddConstraint (
                Expression.op_LessThanOrEqual (
                    Expression.Sum ( playerBidVariables ), 1.0 ),
                    (sprintf "player %i convexity" playerId ) )
        )

    // add edge constraints
    bids
        |> List.collect (fun (_,bid) -> bid.route.edges |> List.map (fun (edge,direction) -> (bid,edge,direction)) )
        |> List.groupBy (fun (_,edge,_) -> edge)
        |> List.iter (fun (edge, edgeBids) ->
            let edgeBidVariables =
                edgeBids
                |> List.map ( fun (bid,_,direction) ->
                    let quantity = match direction with
                                   | Direction.Negative -> -bid.quantity
                                   | Direction.Positive -> +bid.quantity
                    Variable.op_Multiply (x.[bid], quantity ) )
            model.AddConstraint (
                Expression.op_LessThanOrEqual (
                    Expression.Sum ( edgeBidVariables ), edge.capacityPositive),
                    (sprintf "edge %i upper bound" edge.id )
                )
            model.AddConstraint (
                Expression.op_GreaterThanOrEqual (
                    Expression.Sum ( edgeBidVariables ), -edge.capacityNegative),
                    (sprintf "edge %i lower bound" edge.id )
                )
        )

    // add objective
    model.AddObjective (new Objective ( Expression.Sum( List.map (fun (_,bid:Bid) -> bid.totalPrice * x.[bid]) bids ), "weighted sum of bids" ,ObjectiveSense.Maximize ) )

    using (new GurobiSolver ()) (fun solver ->
        let solution = solver.Solve(model)
        x.SetVariableValues(solution.VariableValues))

    bids
        |> List.filter (fun (_,bid) -> x.[bid].Value > 0.0)
        |> List.iter (fun (i,bid) -> printfn "bid %i, of player %i was accepted at rate %f, assigned capacity: %f, routeId: %i" i bid.route.player.id x.[bid].Value (x.[bid].Value * bid.quantity) bid.route.id)

[<EntryPoint>]
let main argv =
    printfn "Hello World from F#!"
    let routes = List.map (priceSingeRoute edgePrices) partialRoutes;
    let bids = calcBids demands routes
    printfn "Solving capacity allocation:"
    optanoGurobiSolution bids
    printfn "Calculating payments for every player:"
    bids
    |> List.groupBy (fun (_,bid) -> bid.route.player.id)
    |> List.map (fun (id,_) -> id)
    |> List.map (fun playerId -> bids |> List.filter (fun (_,bid) -> bid.route.player.id <> playerId))
    |> List.iter optanoGurobiSolution

    0 // return an integer exit code

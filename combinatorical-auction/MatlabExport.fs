module CombinatorialAuction.MatlabExport

open Newtonsoft.Json
open MathNet.Numerics
open MathNet.Numerics.Data.Matlab

open CombinatorialAuction.Models
open CombinatorialAuction.Bidding

let private createRowFromPlayerConstraints numberOfAllBids bids =
    Array.map (fun n -> if (List.contains n bids) then 1.0 else 0.0) [|0..numberOfAllBids-1|]

let calcPlayerConstraints bids =
    let createRow = createRowFromPlayerConstraints (List.length bids)
    bids
    |> List.groupBy (fun bid -> bid.playerId)
    |> List.map (fun (_, subBids) -> subBids |> List.map (fun x -> x.id) |> createRow )

let private edgeConstraintTupleFromBids (bids: (int * Bid * Edge * Direction) list) =
    bids
    |> List.map
        (fun (idx,bid,_,dir) ->
            match dir with
            | Positive -> (idx, bid.quantity)
            | Negative -> (idx, -bid.quantity)
        )

let private createrRowFromEdgeConstraints numberOfAllBids (constraints: (int*float) list) =
    Array.map (fun n ->
            match (List.tryFind (fun (idx, _) -> n = idx) constraints) with
            | Some (_, weight) -> weight
            | None -> 0.0
    ) [| 0..numberOfAllBids |]

let calcEdgeConstraints bids =
    let createRow = createrRowFromEdgeConstraints (List.length bids)
    bids
    |> List.collect (fun (idx,bid) -> bid.route.edges |> List.map (fun (e,d) -> (idx, bid, e,d)))
    |> List.groupBy (fun (_, _, edge, _) -> edge.id)
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

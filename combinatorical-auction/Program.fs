module CombinatorialAuction.Program

open System

open Bidding
open CCA
open Models
open DataSet2

let directedEdgePrinter (edge, direction) =
    match direction with
    | Direction.Positive -> edge.toNode.id
    | Direction.Negative -> edge.fromNode.id
    | _ -> failwith "unknown direction"
    |> string

let routePrinter arrow (route: List<(Edge * Direction)>) =
    let (firstEdge, firstDirection) = route.Head
    let primer =
        match firstDirection with
        | Direction.Positive -> firstEdge.fromNode.id
        | Direction.Negative -> firstEdge.toNode.id
        | _ -> failwith "unknown direction"
        |> string
    List.fold (fun state (directedEdge) -> state + arrow + (directedEdgePrinter directedEdge) ) primer route

let bidPrinter bid =
    sprintf "bid of player %i for capacity: %.1f with price %.1f on route %s" 
        bid.route.player.id 
        bid.quantity 
        bid.totalPrice
        (routePrinter " -> " bid.route.edges)

///<summary>
/// print bids in the following format:
/// \hline
/// $b^1_9$ & 1 $\rightarrow$ 3 $\rightarrow$ 4 $\rightarrow$ 2 & 90 & 265 \\  
///</summary>
let latexBidPrinter (bid, index) =
    printfn "\\hline"
    printfn "$b^{%i}_{%i}$ & %s & %.1f & %.1f \\\\"
        bid.route.player.id 
        index
        (routePrinter " $\\rightarrow$ " bid.route.edges)
        bid.quantity 
        bid.totalPrice
        

let printCcaResult (bids: List<float*Bid>) =
    bids
        |> List.filter (fun (accept,_) -> accept > 0.0)
        |> List.map (fun (accept, bid) -> (bid.route.player.id, accept, (accept * bid.quantity), bid.quantity, (routePrinter " -> " bid.route.edges)) )
        |> List.map (fun (playerId, accept, assignedCapacity, requestedCapacity, route) -> sprintf "bid of player %i was accepted at rate %.3f, assigned capacity: %.1f, requested capacity: %.1f, route: %s" playerId accept assignedCapacity requestedCapacity route)

[<EntryPoint>]
let main argv =
    printfn "Hello World from F#!"

    //routes
    //|> List.map (fun x -> x.edges)
    //|> List.map (routePrinter " -> ")
    //|> List.iter Console.WriteLine
    
    let bids = getBids ()
    
    // latex printing
    bids
    |> Seq.groupBy (fun bid -> bid.route.player.id) 
    |> Seq.collect (fun (_, items) -> Seq.zip items [1..(Seq.length items+1)] )
    |> Seq.iter latexBidPrinter
    
    // normal printing
    bids |> Seq.map bidPrinter |> Seq.iter Console.WriteLine

    printfn "Solving capacity allocation:"
    bids |> cca |> printCcaResult |> Seq.iter Console.WriteLine
    printfn "Calculating payments for every player:"

    bids
    |> VCG.vcg
    |> List.iter (fun (player, payment) ->
        printfn "Player %i, will pay: %f" player.id payment )

    0 // return an integer exit code

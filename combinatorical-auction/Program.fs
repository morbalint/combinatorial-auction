module CombinatorialAuction.Program

// Learn more about F# at http://fsharp.org

open Bidding
open CCA
open Models
open DataSet2

let directedEdgePrinter (edge, direction) =
    match direction with
    | Direction.Positive -> edge.toNode.id
    | Direction.Negative -> edge.fromNode.id
    | _ -> failwith "unkown direction"
    |> string

let routePrinter (route: List<(Edge * Direction)>) =
    let (firstEdge, firstDirection) = route.Head
    let primer = 
        match firstDirection with
        | Direction.Positive -> firstEdge.fromNode.id
        | Direction.Negative -> firstEdge.toNode.id
        | _ -> failwith "unkown direction"
        |> string
    List.fold (fun state (directedEdge) -> state + " -> " + (directedEdgePrinter directedEdge) ) primer route

let printCcaResult (bids: List<float*Bid>) =
    bids
        |> List.filter (fun (accept,_) -> accept > 0.0)
        |> List.iter (fun (accept,bid) -> printfn "bid of player %i was accepted at rate %f, assigned capacity: %f, route: %s" bid.route.player.id accept (accept * bid.quantity) (routePrinter bid.route.edges))

[<EntryPoint>]
let main argv =
    printfn "Hello World from F#!"

    //routes 
    //|> List.map (fun x -> x.edges)
    //|> List.map routePrinter2
    //|> List.iter (printfn "%s")
    
    let bids = getBids ()
    bids
    |> List.iter (fun bid -> 
        printfn 
            "bid of player %i for capacity: %.1f with price %.1f on route %s" 
            bid.route.player.id 
            bid.quantity 
            bid.totalPrice
            (routePrinter bid.route.edges) )
    printfn "Solving capacity allocation:"
    bids |> cca |> printCcaResult
    printfn "Calculating payments for every player:"

    bids 
    |> VCG.vcg
    |> List.iter (fun (player, payment) -> 
        printfn "Player %i, will pay: %f" player.id payment )
    
    

    0 // return an integer exit code

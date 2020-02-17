module CombinatorialAuction.Program

// Learn more about F# at http://fsharp.org

open CombinatorialAuction.Bidding
open CombinatorialAuction.CCA

[<EntryPoint>]
let main argv =
    printfn "Hello World from F#!"
    let bids = getBids ()
    printfn "Solving capacity allocation:"
    bids |> cca |> printResult
    printfn "Calculating payments for every player:"
    bids
    |> List.groupBy (fun bid -> bid.route.player.id)
    |> List.map (fun (id,_) -> 
        bids 
        |> List.filter (fun bid -> bid.route.player.id <> id) 
        |> cca)
    |> List.iter printResult

    0 // return an integer exit code

module CombinatorialAuction.Program

// Learn more about F# at http://fsharp.org

open CombinatorialAuction.Bidding
open CombinatorialAuction.CCA

[<EntryPoint>]
let main argv =
    printfn "Hello World from F#!"
    let bids = getBids ()
    printfn "Solving capacity allocation:"
    cca bids
    printfn "Calculating payments for every player:"
    bids
    |> List.groupBy (fun (_,bid) -> bid.route.player.id)
    |> List.map (fun (id,_) -> id)
    |> List.map (fun playerId -> bids |> List.filter (fun (_,bid) -> bid.route.player.id <> playerId))
    |> List.iter cca

    0 // return an integer exit code

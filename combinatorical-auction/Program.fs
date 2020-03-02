module CombinatorialAuction.Program

// Learn more about F# at http://fsharp.org

open CombinatorialAuction.Bidding
open CombinatorialAuction.CCA

[<EntryPoint>]
let main argv =
    printfn "Hello World from F#!"
    let bids = getBids ()
    bids 
    |> List.filter (fun bid -> bid.route.player.id = 2 && bid.route.id = 1) 
    |> List.iter (fun bid -> printfn "bid of player 2 on route 1 for capacity: %f with price %f" bid.quantity bid.totalPrice)
    printfn "Solving capacity allocation:"
    bids |> cca |> printResult
    printfn "Calculating payments for every player:"

    bids 
    |> VCG.vcg
    |> List.iter (fun (player, payment) -> 
        printfn "Player %i, will pay: %f" player.id payment )
    
    

    0 // return an integer exit code

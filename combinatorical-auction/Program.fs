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

let printAllRoutes () =
    routes
    |> List.map (fun x -> x.edges)
    |> List.map (routePrinter " -> ")
    |> List.iter Console.WriteLine

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

let printBidsForLatex bids = 
    bids
    |> Seq.groupBy (fun bid -> bid.route.player.id)
    |> Seq.collect (fun (_, items) -> Seq.zip items [1..(Seq.length items+1)] )
    |> Seq.iter latexBidPrinter

let printCcaResult (bids: List<float*Bid>) =
    bids
        |> List.filter (fun (accept,_) -> accept > 0.0)
        |> List.map (fun (accept, bid) -> (bid.route.player.id, accept, (accept * bid.quantity), bid.quantity, (routePrinter " -> " bid.route.edges)) )
        |> List.map (fun (playerId, accept, assignedCapacity, requestedCapacity, route) -> sprintf "bid of player %i was accepted at rate %.3f, assigned capacity: %.1f, requested capacity: %.1f, route: %s" playerId accept assignedCapacity requestedCapacity route)

let twistBid twister (bids: Bid list) index =
    let twistedBid = { bids.[index] with
                        totalPrice = twister * bids.[index].totalPrice }
    if index = 0 then
        twistedBid :: bids.GetSlice(Some 1, None)
    elif index = bids.Length - 1 then
        List.append (bids.GetSlice (Some 0, Some (index - 1))) [twistedBid]
    else
        List.concat [ bids.GetSlice (Some 0, Some (index - 1)); [twistedBid]; bids.GetSlice(Some (index+1), None) ]

let replaceBidInResult ccaResult originalBid =
    ccaResult
    |> List.map (fun (a, bid:Bid) ->
        if bid.route = originalBid.route && bid.quantity = originalBid.quantity
        then (a,originalBid)
        else (a,bid) )


let playerValue (ccaResult: (float*Bid) list) (player: Player) = 
    ccaResult
    |> List.filter (fun (_, bid: Bid) -> bid.route.player = player ) // filter for player
    |> List.sumBy (fun (acceptance, bid:Bid) -> bid.totalPrice * acceptance)

let profits (vcgResults: VCG.Results) =
    let payments = dict vcgResults.payments
    payments.Keys |> Seq.map (fun player ->
        let value = playerValue vcgResults.acceptance player
        player, value - payments.[player])

let tester (bids: Bid list) (originalProfits: System.Collections.Generic.IDictionary<Player, float> ) rate = 
    let twistedBids = seq { 0 .. bids.Length-1 } |> Seq.map (twistBid rate bids)
    twistedBids
    |> Seq.mapi (fun index tbids ->
                    let vcgRes = VCG.detailedVcg tbids
                    let payments = dict vcgRes.payments
                    let originalBids = replaceBidInResult vcgRes.acceptance bids.[index]
                    let profits = 
                        payments.Keys
                        |> Seq.map (fun player -> (player, ((playerValue originalBids player) - payments.[player])))
                    profits 
                    |> Seq.filter (fun (player, _) -> player = tbids.[index].route.player)
                    |> Seq.tryFind (fun (player, profit) ->
                        if (originalProfits.[player] * 1.001) < profit then
                            printfn "Found!"
                            printfn "Original profit: %f, twisted profit: %f" originalProfits.[player] profit
                            true
                        else
                            false
                    ) = None)
    |> Seq.tryFind id = None

let runCCA () =
    let bids = getBids ()
    // normal printing
    bids |> Seq.map bidPrinter |> Seq.iter Console.WriteLine
    printfn "Solving capacity allocation:"
    bids |> cca |> printCcaResult |> Seq.iter Console.WriteLine

[<EntryPoint>]
let main argv =
    printfn "Hello World from F#!"

    let clock = System.Diagnostics.Stopwatch.StartNew()

    let bids = getBids ()
    //let vcgRes = bids |> VCG.detailedVcg
    //vcgRes.modifiedCcaAcceptance.Head |> snd |> printCcaResult |> Seq.iter Console.WriteLine
    

    //bids
    //|> VCG.vcg
    //|> List.iter (fun (player, without, others) ->
    //    printfn "Player %i, will pay: %f" player.id (without - others)
    //    printfn "nominal payments without player: %f" without
    //    printfn "nominal payments of other players: %f" others
    //)

    let originalProfits =
        bids 
        |> VCG.detailedVcg
        |> profits
        |> dict

    let q = seq { (1.0 / 1024.0) .. (1.0 / 1024.0) .. 3.0 } |> Seq.map (tester bids originalProfits ) |> Seq.tryFind id <> None
    clock.Stop()
    printfn "%A" clock.Elapsed

    printfn "%A" q

    0 // return an integer exit code

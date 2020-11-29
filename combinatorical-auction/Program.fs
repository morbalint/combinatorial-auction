module CombinatorialAuction.Program

open System

open Bidding
open ACA
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

let printer title data =
    printfn "%s:" title
    data |> Seq.iter ( printfn "%A" )
    printfn "=================="

let ACAtester () =
    let transport_routes = List.map (priceSingleRouteWithSource edgePrices prices) routes
    let bids_0 = ACA.make_bids DataSet2.demands transport_routes
    let demands_0 = DataSet2.demands

    let increment_per_step = 1.
    let my_increment_price_function = ACA.increment_price increment_per_step

    let mutable i = 0;

    let rec loop already_closed_edges total_increment bids previous_results =
        printfn "Step %i:" i
        // printer "Bids" bids

        let closed_edges_next =
            closed_edges bids
            |> set
            |> Set.union already_closed_edges

        let newly_closed_edges = closed_edges_next - already_closed_edges

        // previous bids are the new routes! (close off everything you didn't bid in the previous step)

        let priced_closed_edges =
            newly_closed_edges
            |> Seq.map (assign_price_to_closed_edge DataSet2.edgePrices total_increment)
            |> Seq.toArray

        let partial_results = 
            bids 
            |> Seq.collect (fun bid -> 
                bid.edges 
                |> Seq.map fst
                |> set
                |> Set.intersect newly_closed_edges
                |> Seq.map (fun e -> { 
                    player = bid.player; 
                    edge = e; 
                    capacity = bid.capacity; 
                    unitPrice = bid.unitPrice }))

        let qq = 
            partial_results
            |> Seq.map (fun r -> { r with unitPrice = total_increment })
        printer "Results with increment instead of unit price" qq

        let results = Seq.append previous_results partial_results

        let routes_next =
            bids
            |> Seq.map ( decrement_on_route priced_closed_edges )
            |> Seq.zip bids
            |> Seq.map (
                (fun (r,dec) -> close_edges_on_route dec r) >> 
                (my_increment_price_function closed_edges_next ))
            |> Seq.toList

        let bids_next = ACA.make_bids demands_0 routes_next

        // is it done?
        let edges =
            bids_next
            |> Seq.collect (fun b -> b.edges |> Seq.map fst )
            |> set

        let open_edges = edges - closed_edges_next

        if not (Set.isEmpty open_edges) && i < 100 then
            i <- i + 1
            loop closed_edges_next (total_increment + increment_per_step) bids_next results
        else
            results, bids_next

    let (results, bids) = loop Set.empty 0. bids_0 []

    printer "All bids" bids
    let bids_of_player1 = 
        bids 
        |> Seq.filter ( fun bid -> bid.player.id = 1 ) 
        |> Seq.map ( fun b -> ((routePrinter "->" b.edges), b.unitPrice, b.capacity))
    printer "Bids of player 1" bids_of_player1
    printer "All results" results
    results
        |> Seq.groupBy (fun r -> r.player)
        |> Seq.iter (fun (p,q) -> 
            let res = q |> Seq.map (fun r -> (r.edge,r.capacity, r.unitPrice))
            printer (sprintf "Results of player %i" p.id) res
        )
    results
        |> Seq.groupBy (fun r -> r.edge)
        |> Seq.sortBy fst
        |> Seq.iter (fun (e,q) -> 
            let res = q |> Seq.map (fun r -> (r.player.id, r.capacity, r.unitPrice))
            printer (sprintf "Results on edge %i" e.id) res)

let runACA () =
    let transport_routes = List.map (priceSingleRouteWithSource edgePrices prices) routes
    let results = ACA.runACA demands transport_routes edgePrices 1.
    results
        |> Seq.groupBy (fun r -> r.player)
        |> Seq.iter (fun (p,q) -> 
            let res = q |> Seq.map (fun r -> (r.edge,r.capacity, r.unitPrice))
            printer (sprintf "Results of player %i" p.id) res
        )
    results
        |> Seq.groupBy (fun r -> r.edge)
        |> Seq.sortBy fst
        |> Seq.iter (fun (e,q) -> 
            let basePrice = ((edgePrices |> Seq.find (fun t -> t.onEdge = e)).price) + DataSet2.prices.Head.price
            let res = q |> Seq.map (fun r -> (r.player.id, r.capacity, r.unitPrice - basePrice))
            printer (sprintf "Results on edge %i" e.id) res)


[<EntryPoint>]
let main argv =
    printfn "Hello World from F#!"

    let clock = System.Diagnostics.Stopwatch.StartNew()

    runACA ()


    // let bids = getBids ()
    //let vcgRes = bids |> VCG.detailedVcg
    //vcgRes.modifiedCcaAcceptance.Head |> snd |> printCcaResult |> Seq.iter Console.WriteLine

    //bids
    //|> VCG.vcg
    //|> List.iter (fun (player, without, others) ->
    //    printfn "Player %i, will pay: %f" player.id (without - others)
    //    printfn "nominal payments without player: %f" without
    //    printfn "nominal payments of other players: %f" others
    //)

    //let originalProfits =
    //    bids
    //    |> VCG.detailedVcg
    //    |> profits
    //    |> dict

    printfn "%A" clock.Elapsed

    // printfn "%A" originalProfits

    //let profitTestResult = seq { (1.0 / 1024.0) .. (1.0 / 1024.0) .. 3.0 } |> Seq.map (tester bids originalProfits ) |> Seq.tryFind id <> None
    //clock.Stop()
    //printfn "%A" clock.Elapsed

    //printfn "%A" profitTestResult

    0 // return an integer exit code

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

let runACA2 () =
    let bids_0 = ACA.make_bids DataSet2.demands ACA.reference_priced_routes
    let demands_0 = DataSet2.demands

    let increment_per_step = 1.
    let my_increment_price_function = ACA.increment_price increment_per_step

    let mutable i = 0;

    let rec loop already_closed_edges total_increment (demands: Demand seq) bids =
        printfn "Step %i:" i
        printer "Bids" bids
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
        let decrements_per_route =
            bids
            |> Seq.map ( decrement_on_route priced_closed_edges )
            |> Seq.zip bids
            |> Seq.toArray

        let routes_next =
            decrements_per_route
            |> Seq.map (fun (r,dec) -> close_edges_on_route dec r)
            |> Seq.map (my_increment_price_function closed_edges_next )
            |> Seq.toList

        let player_payments =
            decrements_per_route
            |> Seq.map (fun (r,d) -> r,d*r.capacity)
            |> Seq.groupBy (fun (r,_) -> r.player)
            |> Seq.map ( fun (p, prs) -> p,(prs |> Seq.sumBy snd))
            |> Seq.toArray

        let demands_next =
            demands
            |> Seq.groupBy ( fun d -> d.player )
            |> Seq.allPairs player_payments
            |> Seq.filter ( fun ((p1,_),(p2,_)) -> p1 = p2)
            |> Seq.map ( fun ((_,decrement),(_,dems)) -> ACA.update_demands decrement (dems |> Seq.toList) )
            |> Seq.collect (fun x -> x)
            |> Seq.toArray

        let bids_next = ACA.make_bids demands_next routes_next

        // is it done?
        let edges =
            bids_next
            |> Seq.collect (fun b -> b.edges |> Seq.map fst )
            |> set

        let open_edges = edges - closed_edges_next

        if not (Set.isEmpty open_edges) && i < 100 then
            i <- i + 1
            loop closed_edges_next (total_increment + increment_per_step) demands_next bids_next
        else
            routes_next, demands_next, bids_next

    let (ipr_final, demands_final, bids_final) = loop Set.empty 0. demands_0 bids_0
    bids_final

[<EntryPoint>]
let main argv =
    printfn "Hello World from F#!"

    let clock = System.Diagnostics.Stopwatch.StartNew()

    let bids = runACA2 ()
    printer "Final bids" (bids |> Seq.filter ( fun bid -> bid.player.id = 1 ))

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

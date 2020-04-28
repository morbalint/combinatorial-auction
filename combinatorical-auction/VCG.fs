module CombinatorialAuction.VCG

open CombinatorialAuction.Models
open CombinatorialAuction.CCA

let bidResultsFromTuple (acceptance, bid) =
    {
        bid = bid
        acceptance = acceptance
        payment = bid.totalPrice * acceptance
    }

let totalPaymentsWithoutAPlayer bids playerId =
    bids
    |> List.filter (fun bid -> bid.route.player.id <> playerId)
    |> cca
    |> List.sumBy (fun (accept,bid) -> accept * bid.totalPrice)

let paymentOfOtherPlayers result playerId =
        result
        |> List.filter (fun (_,bid) -> bid.route.player.id <> playerId)
        |> List.sumBy (fun (accept,bid) -> accept * bid.totalPrice)

let vcg bids =
    let result = cca bids
    let totalPaymentsWithout = totalPaymentsWithoutAPlayer bids
    let paymentOfOthers = paymentOfOtherPlayers result
    bids
    |> List.groupBy (fun bid -> bid.route.player)
    |> List.map (fun (player,_) ->
        let without = totalPaymentsWithout player.id
        let others = paymentOfOthers player.id
        (player, without - others)
    )


type Results = {
    acceptance: (float * Bid) list;
    payments: (Player * float) list;
    modifiedCcaAcceptance: (Player * ((float * Bid) list)) list;
}

let detailedVcg bids =
    let result = cca bids
    let modified playerId = 
        bids
        |> List.filter (fun bid -> bid.route.player.id <> playerId)
        |> cca
    let paymentOfOthers = paymentOfOtherPlayers result
    let data = 
        bids
        |> List.groupBy (fun bid -> bid.route.player)
        |> List.map (fun (player,_) ->
            let modifiedResults = modified player.id
            let without = List.sumBy (fun (accept,bid) -> accept * bid.totalPrice) modifiedResults
            let others = paymentOfOthers player.id
            (player, without - others), (player, modifiedResults)
        )
    { acceptance = result
      payments = (List.map fst data)
      modifiedCcaAcceptance = (List.map (fun (_,q) -> q) data) }
    

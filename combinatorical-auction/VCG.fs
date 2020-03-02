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


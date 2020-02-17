module CombinatorialAuction.VCG

open CombinatorialAuction.Models
open CombinatorialAuction.CCA

let vcg bids =
    let result = cca bids
    bids
    |> List.groupBy (fun bid -> bid.route.player.id)
    |> List.map (fun (id,_) -> 
        let without = 
            bids 
            |> List.filter (fun bid -> bid.route.player.id <> id) 
            |> cca
            |> List.sumBy (fun (accept,bid) -> accept * bid.totalPrice)
        let others = 
            result 
            |> List.filter (fun (_,bid) -> bid.route.player.id <> id)
            |> List.sumBy (fun (accept,bid) -> accept * bid.totalPrice)
        (id, without - others)
    )

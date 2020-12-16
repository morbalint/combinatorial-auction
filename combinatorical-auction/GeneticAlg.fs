module CombinatorialAuction.GeneticAlg

open CombinatorialAuction.Models
open CombinatorialAuction.DataSet2
//open CombinatorialAuction.Bidding
//open CombinatorialAuction.ACA
//open CombinatorialAuction.CCA

open GeneticSharp.Domain.Fitnesses
open GeneticSharp.Domain.Chromosomes

let ``Sum result segments of the same edge and player`` (segments: ResultSegment seq) =
    assert (segments |> Seq.groupBy (fun seg -> seg.player) |> Seq.length = 1)
    assert (segments |> Seq.groupBy (fun seg -> seg.edge) |> Seq.length = 1)

    let head = segments |> Seq.head

    let cap = segments |> Seq.sumBy (fun seg -> seg.capacity)
    let totalPrice = segments |> Seq.sumBy (fun seg -> seg.capacity * seg.unitPrice)
    { player = head.player; edge = head.edge; capacity = cap; unitPrice = totalPrice / cap }

let sum_results (segments: ResultSegment seq) =
    segments
    |> Seq.groupBy (fun seg -> seg.player)
    |> Seq.collect (fun (_,segs) ->
        segs |> Seq.groupBy (fun seg -> seg.edge) |> Seq.map(snd >> ``Sum result segments of the same edge and player``) )

let result_segment_fitness (left: ResultSegment seq) (right: ResultSegment seq) =
    let union =
        left |> Seq.choose (fun l_seg ->
            right
            |> Seq.filter (fun r_seg -> l_seg.player = r_seg.player && l_seg.edge = r_seg.edge)
            |> Seq.map (fun r_seg ->
                let r_cap = abs r_seg.capacity
                let l_cap = abs l_seg.capacity
                let s = min l_cap r_cap
                let d = (max l_cap r_cap) - s
                (s,d))
            |> Seq.tryExactlyOne)
    let same = union |> Seq.sumBy fst
    let diff = union |> Seq.sumBy snd
    let l_diff =
        left
        |> Seq.filter (fun l_seg -> not (right |> Seq.exists (fun r_seg -> l_seg.player = r_seg.player && l_seg.edge = r_seg.edge)))
        |> Seq.sumBy(fun seg -> abs seg.capacity)
    let r_diff =
        right
        |> Seq.filter (fun r_seg -> not (left |> Seq.exists (fun l_seg -> r_seg.player = l_seg.player && r_seg.edge = l_seg.edge)))
        |> Seq.sumBy(fun seg -> abs seg.capacity)
    let total_diff = diff + r_diff + l_diff
    let total = same + total_diff
    total_diff / total

let ``Get transfer prices from chromosome`` (chromosome : IChromosome) =
    [0..dataset.transferPrices.Length-1]
    |> List.map (fun i ->
        let price = chromosome.GetGene(i).Value :?> float
        { dataset.transferPrices.[i] with price = price })

let run_aca_with_custom_prices prices =
    let transport_routes = List.map (Bidding.priceSingeRoute prices) dataset.routes;
    ACA.runACA dataset.demands transport_routes prices 1.

let run_cca_with_custom_prices prices =
    let transport_routes = List.map (Bidding.priceSingeRoute prices) dataset.routes;
    let cca_bids = Bidding.bidsFromPricedRoutes dataset.demands transport_routes
    let cca_bid_acceptance = CCA.cca cca_bids
    let cca_results =
        cca_bid_acceptance
        |> Seq.filter (fun (x,_) -> x > 0.)
        |> Seq.collect (fun (x,bid) ->
            bid.route.edges |> Seq.map(fun (e,d) ->
                let cap =
                    match d with
                    | Direction.Positive -> bid.quantity * x
                    | Direction.Negative -> -bid.quantity * x
                    | _ -> failwithf "unknown direction %A" d
                { player = bid.route.player; edge = e; capacity = cap; unitPrice = (bid.totalPrice * x) / bid.quantity }))
        |> sum_results
    cca_results

type MyFitness() =
    interface IFitness with
        member _.Evaluate chromosome =
            let updated_prices = ``Get transfer prices from chromosome`` chromosome
            let transport_routes = List.map (Bidding.priceSingeRoute updated_prices) dataset.routes;
            let aca_result = ACA.runACA dataset.demands transport_routes updated_prices 1.
            let cca_bids = Bidding.bidsFromPricedRoutes dataset.demands transport_routes
            let cca_bid_acceptance = CCA.cca cca_bids
            let cca_results =
                cca_bid_acceptance
                |> Seq.collect (fun (x,bid) ->
                    bid.route.edges |> Seq.map(fun (e,d) ->
                        let cap =
                            match d with
                            | Direction.Positive -> bid.quantity * x
                            | Direction.Negative -> -bid.quantity * x
                            | _ -> failwithf "unknown direction %A" d
                        { player = bid.route.player; edge = e; capacity = cap; unitPrice = (bid.totalPrice * x) / bid.quantity }))
                |> sum_results
            result_segment_fitness aca_result cca_results
    end

type MyChromosome() as this =
    inherit ChromosomeBase(dataset.transferPrices.Length)
    do this.CreateGenes()
    static member distribution = MathNet.Numerics.Distributions.Normal(7., 2.)
    override _.GenerateGene geneIndex =
        Gene (MyChromosome.distribution.Sample ())
    override _.CreateNew () =
        MyChromosome () :> IChromosome

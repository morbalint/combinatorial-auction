module CombinatorialAuction.CCA

open CombinatorialAuction.Models
open OPTANO.Modeling.Optimization
open OPTANO.Modeling.Optimization.Enums
open OPTANO.Modeling.Optimization.Solver.Gurobi810

let private addPlayerConstraints bids (model:Model) (variables:VariableCollection<Bid>) =
    bids
    |> List.groupBy (fun bid -> bid.route.player.id)
    |> List.iter (fun (playerId, playerBids) ->
        let playerBidVariables = playerBids |> List.map (fun bid -> variables.[bid])
        model.AddConstraint (
            Expression.op_LessThanOrEqual (
                Expression.Sum ( playerBidVariables ), 1.0 ),
                (sprintf "player %i convexity" playerId ) )
    )

let private addEdgeConstraints bids (model:Model) (x:VariableCollection<Bid>) =
    bids
    |> List.collect (fun bid -> bid.route.edges |> List.map (fun (edge,direction) -> (bid,edge,direction)) )
    |> List.groupBy (fun (_,edge,_) -> edge)
    |> List.iter (fun (edge, edgeBids) ->
        let edgeBidVariables =
            edgeBids
            |> List.map ( fun (bid,_,direction) ->
                let quantity = (float direction) * bid.quantity
                Variable.op_Multiply (x.[bid], quantity ) )
        model.AddConstraint (
            Expression.op_LessThanOrEqual (
                Expression.Sum ( edgeBidVariables ), edge.capacityPositive),
                (sprintf "edge %i upper bound" edge.id )
            )
        model.AddConstraint (
            Expression.op_GreaterThanOrEqual (
                Expression.Sum ( edgeBidVariables ), -edge.capacityNegative),
                (sprintf "edge %i lower bound" edge.id )
            )
    )

let private addObjective bids (model:Model) (x:VariableCollection<Bid>) =
    model.AddObjective (new Objective ( Expression.Sum( List.map (fun bid -> bid.totalPrice * x.[bid]) bids ), "weighted sum of bids" ,ObjectiveSense.Maximize ) )

let cca (bids: Bid list) =
    // create model
    let model = new Model ()
    // create variable
    let x = new VariableCollection<Bid>(
                                                model,
                                                bids,
                                                "x",
                                                (fun bid -> sprintf "Bid by player %i on route %i" bid.route.player.id bid.route.id),
                                                (fun _ -> 0.0),
                                                (fun _ -> 1.0),
                                                (fun _ -> VariableType.Continuous))

    addPlayerConstraints bids model x
    addEdgeConstraints bids model x
    addObjective bids model x

    // solve
    using (new GurobiSolver ()) (fun solver ->
        let solution = solver.Solve(model)
        x.SetVariableValues(solution.VariableValues))

    bids |> List.map (fun bid -> (x.[bid].Value, bid))

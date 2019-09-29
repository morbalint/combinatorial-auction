// Learn more about F# at http://fsharp.org

open System

type Node = {
    production: double;
    id: int
}

type Edge = {
   fromNode : Node;
   toNode : Node;
   capacityPositive : double;
   capacityNegative : double;
}

type Player = {
    id: int;
    node: Node;
}

type TransferCost = {
    onEdge: Edge;
    forPlayer: Player;
    price: double;
}

type SourcePrices = {
    fromProducer: Player;
    toConsumer: Player;
    price: double;
}

// piece of constant inverse demand curve
type Demand = {
    player: Player;
    fromAmount: double;
    toAmount: double;
    price: double;
}

type EdgePrice = {
    player: Player;
    edge: Edge;
    price: double;
}

type Direction =
    | Positive
    | Negative

type Transport = {
    player: Player;
    onEdge: Edge;
    Price: double;
    amount: double;
    direction: Direction;
}

let nodes = [
    { id = 1; production = 1000.0 };
    { id = 2; production = -95.0; };
    { id = 3; production = -100.0; };
    { id = 4; production = -100.0; };
]

let edges = [
    { fromNode = nodes.[1]; toNode = nodes.[0]; capacityPositive = 80.0; capacityNegative = 80.0; };
    { fromNode = nodes.[2]; toNode = nodes.[0]; capacityPositive = 75.0; capacityNegative = 75.0; };
    { fromNode = nodes.[3]; toNode = nodes.[0]; capacityPositive = 70.0; capacityNegative = 70.0; };
    { fromNode = nodes.[2]; toNode = nodes.[1]; capacityPositive = 60.0; capacityNegative = 60.0; };
    { fromNode = nodes.[3]; toNode = nodes.[1]; capacityPositive = 60.0; capacityNegative = 60.0; };
    { fromNode = nodes.[3]; toNode = nodes.[2]; capacityPositive = 60.0; capacityNegative = 60.0; };
]

let players = [
    { id = 0; node = nodes.[0]  };
    { id = 1; node = nodes.[1]  };
    { id = 2; node = nodes.[2]  };
    { id = 3; node = nodes.[3]  };
]

let demands = [
    { player = players.[1]; fromAmount = 0.0; toAmount = 40.0; price = 47.0; };
    { player = players.[1]; fromAmount = 40.0; toAmount = 70.0; price = 39.0; }
    { player = players.[1]; fromAmount = 70.0; toAmount = 95.0; price = 30.0; }
    { player = players.[2]; fromAmount = 0.0; toAmount = 30.0; price = 46.0; }
    { player = players.[2]; fromAmount = 30.0; toAmount = 70.0; price = 38.0; }
    { player = players.[2]; fromAmount = 70.0; toAmount = 100.0; price = 32.0; }
    { player = players.[3]; fromAmount = 0.0; toAmount = 50.0; price = 53.0; }
    { player = players.[3]; fromAmount = 50.0; toAmount = 80.0; price = 49.0; }
    { player = players.[3]; fromAmount = 80.0; toAmount = 100.0; price = 36.0; }
]

let prices = [
    { fromProducer = players.[0]; toConsumer = players.[1]; price = 25.0; };
    { fromProducer = players.[0]; toConsumer = players.[2]; price = 23.0; };
    { fromProducer = players.[0]; toConsumer = players.[3]; price = 20.0; };
]

type Route = {
    player: Player;
    nodes: Node list;
    capacity: double;
}

// TODO: this should be calculated;
let routes = [
    { player = players.[1]; nodes = [ nodes.[0]; nodes.[1] ]; capacity = edges.[0].capacityNegative };
    {  }
]

[<EntryPoint>]
let main argv =
    printfn "Hello World from F#!"
    0 // return an integer exit code

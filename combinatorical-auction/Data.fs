module CombinatorialAuction.Data

open CombinatorialAuction.Models

let nodes = [
    { id = 1; production = 1000.0 };
    { id = 2; production = -95.0; };
    { id = 3; production = -100.0; };
    { id = 4; production = -100.0; };
]

let edges = [
    { id = 1; fromNode = nodes.[1]; toNode = nodes.[0]; capacityPositive = 80.0; capacityNegative = 80.0; };
    { id = 2; fromNode = nodes.[2]; toNode = nodes.[0]; capacityPositive = 75.0; capacityNegative = 75.0; };
    { id = 3; fromNode = nodes.[3]; toNode = nodes.[0]; capacityPositive = 70.0; capacityNegative = 70.0; };
    { id = 4; fromNode = nodes.[2]; toNode = nodes.[1]; capacityPositive = 60.0; capacityNegative = 60.0; };
    { id = 5; fromNode = nodes.[3]; toNode = nodes.[1]; capacityPositive = 60.0; capacityNegative = 60.0; };
    { id = 6; fromNode = nodes.[3]; toNode = nodes.[2]; capacityPositive = 60.0; capacityNegative = 60.0; };
]

let players = [
    { id = 0; node = nodes.[0] };
    { id = 1; node = nodes.[1] };
    { id = 2; node = nodes.[2] };
    { id = 3; node = nodes.[3] };
]

let sources = players |> List.map (fun p -> p.node.production > 0.0)
let consumers = players |> List.map (fun p -> p.node.production < 0.0)

let edgePrices = [
    { player = players.[1]; edge = edges.[0]; price = 9.0; }
    { player = players.[2]; edge = edges.[0]; price = 9.0; }
    { player = players.[3]; edge = edges.[0]; price = 4.0; }
    { player = players.[1]; edge = edges.[1]; price = 5.0; }
    { player = players.[2]; edge = edges.[1]; price = 8.0; }
    { player = players.[3]; edge = edges.[1]; price = 8.0; }
    { player = players.[1]; edge = edges.[2]; price = 11.0; }
    { player = players.[2]; edge = edges.[2]; price = 11.0; }
    { player = players.[3]; edge = edges.[2]; price = 11.0; }
    { player = players.[1]; edge = edges.[3]; price = 1.0; }
    { player = players.[2]; edge = edges.[3]; price = 4.0; }
    { player = players.[3]; edge = edges.[3]; price = 4.0; }
    { player = players.[1]; edge = edges.[4]; price = 4.5; }
    { player = players.[2]; edge = edges.[4]; price = 4.5; }
    { player = players.[3]; edge = edges.[4]; price = 1.5; }
    { player = players.[1]; edge = edges.[5]; price = 5.0; }
    { player = players.[2]; edge = edges.[5]; price = 1.5; }
    { player = players.[3]; edge = edges.[5]; price = 5.0; }
]

let prices = [
    { fromProducer = nodes.[0]; toConsumer = players.[1]; price = 25.0; };
    { fromProducer = nodes.[0]; toConsumer = players.[2]; price = 23.0; };
    { fromProducer = nodes.[0]; toConsumer = players.[3]; price = 20.0; };
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
    { player = players.[3]; fromAmount = 80.0; toAmount = 105.0; price = 36.0; }
]

// TODO: this should be calculated;
let partialRoutes = [
    { id = 1; player = players.[1]; edges = [ edges.[0], Negative ]; }
    { id = 2; player = players.[1]; edges = [ edges.[1], Negative; edges.[3], Positive ]; }
    { id = 3; player = players.[1]; edges = [ edges.[2], Negative; edges.[4], Positive ]; }
    { id = 4; player = players.[1]; edges = [ edges.[1], Negative; edges.[5], Negative; edges.[4], Positive ]; }
    { id = 5; player = players.[1]; edges = [ edges.[2], Negative; edges.[5], Positive; edges.[3], Positive ]; }
    { id = 1; player = players.[2]; edges = [ edges.[1], Negative ]; }
    { id = 2; player = players.[2]; edges = [ edges.[0], Negative; edges.[3], Negative ]; }
    { id = 4; player = players.[2]; edges = [ edges.[2], Negative; edges.[5], Positive ]; }
    { id = 3; player = players.[2]; edges = [ edges.[2], Negative; edges.[4], Positive; edges.[3], Negative ]; }
    { id = 1; player = players.[3]; edges = [ edges.[2], Negative ]; }
    { id = 2; player = players.[3]; edges = [ edges.[0], Negative; edges.[4], Negative ]; }
    { id = 3; player = players.[3]; edges = [ edges.[1], Negative; edges.[5], Negative ]; }
    { id = 4; player = players.[3]; edges = [ edges.[1], Negative; edges.[3], Positive; edges.[4], Negative ]; }
]

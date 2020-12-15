module CombinatorialAuction.DataSet2

open CombinatorialAuction.Models

let nodes = [
    { id = 1; };
    { id = 2; };
    { id = 3; };
    { id = 4; };
]

let edges = [
    { id = 1; fromNode = nodes.[1]; toNode = nodes.[0]; capacity = 80.0; };
    { id = 2; fromNode = nodes.[2]; toNode = nodes.[0]; capacity = 75.0; };
    { id = 3; fromNode = nodes.[3]; toNode = nodes.[0]; capacity = 70.0; };
    { id = 4; fromNode = nodes.[2]; toNode = nodes.[1]; capacity = 60.0; };
    { id = 5; fromNode = nodes.[3]; toNode = nodes.[1]; capacity = 60.0; };
    { id = 6; fromNode = nodes.[3]; toNode = nodes.[2]; capacity = 60.0; };
]

let players = [
    { id = 0; node = nodes.[0] };
    { id = 1; node = nodes.[1] };
    { id = 2; node = nodes.[2] };
    { id = 3; node = nodes.[3] };
]

let edgePrices = [
    { onEdge = edges.[0]; price = 9.0; }
    { onEdge = edges.[1]; price = 8.0; }
    { onEdge = edges.[2]; price = 11.0; }
    { onEdge = edges.[3]; price = 4.0; }
    { onEdge = edges.[4]; price = 4.5; }
    { onEdge = edges.[5]; price = 5.0; }
]

let demands = [
    { player = players.[1]; fromAmount = 0.0; toAmount = 50.0; price = 47.0; };
    { player = players.[1]; fromAmount = 50.0; toAmount = 90.0; price = 39.0; }
    { player = players.[1]; fromAmount = 90.0; toAmount = 125.0; price = 30.0; }
    { player = players.[2]; fromAmount = 0.0; toAmount = 40.0; price = 46.0; }
    { player = players.[2]; fromAmount = 40.0; toAmount = 85.0; price = 38.0; }
    { player = players.[2]; fromAmount = 85.0; toAmount = 120.0; price = 32.0; }
    { player = players.[3]; fromAmount = 0.0; toAmount = 50.0; price = 53.0; }
    { player = players.[3]; fromAmount = 50.0; toAmount = 85.0; price = 49.0; }
    { player = players.[3]; fromAmount = 85.0; toAmount = 130.0; price = 36.0; }
]

// TODO: this should be calculated;
let routes = [
    { id = 1; player = players.[1]; edges = [ edges.[0], Direction.Negative ]; }
    { id = 2; player = players.[1]; edges = [ edges.[1], Direction.Negative; edges.[3], Direction.Positive ]; }
    { id = 3; player = players.[1]; edges = [ edges.[2], Direction.Negative; edges.[4], Direction.Positive ]; }
    { id = 4; player = players.[1]; edges = [ edges.[1], Direction.Negative; edges.[5], Direction.Negative; edges.[4], Direction.Positive ]; }
    { id = 5; player = players.[1]; edges = [ edges.[2], Direction.Negative; edges.[5], Direction.Positive; edges.[3], Direction.Positive ]; }
    { id = 1; player = players.[2]; edges = [ edges.[1], Direction.Negative ]; }
    { id = 2; player = players.[2]; edges = [ edges.[0], Direction.Negative; edges.[3], Direction.Negative ]; }
    { id = 4; player = players.[2]; edges = [ edges.[2], Direction.Negative; edges.[5], Direction.Positive ]; }
    { id = 3; player = players.[2]; edges = [ edges.[2], Direction.Negative; edges.[4], Direction.Positive; edges.[3], Direction.Negative ]; }
    { id = 5; player = players.[2]; edges = [ edges.[0], Direction.Negative; edges.[4], Direction.Negative; edges.[5], Direction.Positive ]; }
    { id = 1; player = players.[3]; edges = [ edges.[2], Direction.Negative ]; }
    { id = 2; player = players.[3]; edges = [ edges.[0], Direction.Negative; edges.[4], Direction.Negative ]; }
    { id = 3; player = players.[3]; edges = [ edges.[1], Direction.Negative; edges.[5], Direction.Negative ]; }
    { id = 4; player = players.[3]; edges = [ edges.[1], Direction.Negative; edges.[3], Direction.Positive; edges.[4], Direction.Negative ]; }
    { id = 5; player = players.[3]; edges = [ edges.[0], Direction.Negative; edges.[3], Direction.Negative; edges.[5], Direction.Negative ]; }
]

let dataset : DataSet = {
    nodes = nodes;
    edges = edges;
    players = players;
    demands = demands;
    sourcePrice = 23.;
    transferPrices = edgePrices;
    routes = routes;
}

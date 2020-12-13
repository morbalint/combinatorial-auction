module CombinatorialAuction.Dataset3

open CombinatorialAuction.Models

let nodes = [
    { id = 1; }
    { id = 2; }
]

let edges = [
    { id = 1; fromNode = nodes.[1]; toNode = nodes.[0]; capacityPositive = 100.; capacityNegative = 100.; }
]

let players = [
    { id = 1; node = nodes.[1] }
    { id = 2; node = nodes.[1] }
]

let edgePrices = [
    { forPlayer = players.[1]; onEdge = edges.[0]; price = 8.0; }
    { forPlayer = players.[2]; onEdge = edges.[0]; price = 8.0; }
]

let prices = [
    { fromProducer = nodes.[0]; toConsumer = players.[1]; price = 23.0; };
    { fromProducer = nodes.[0]; toConsumer = players.[2]; price = 23.0; };
]

let demands = [
    { player = players.[1]; fromAmount = 0.0; toAmount = 50.0; price = 47.0; };
    { player = players.[1]; fromAmount = 50.0; toAmount = 90.0; price = 39.0; }
    { player = players.[1]; fromAmount = 90.0; toAmount = 125.0; price = 30.0; }
    { player = players.[2]; fromAmount = 0.0; toAmount = 40.0; price = 46.0; }
    { player = players.[2]; fromAmount = 40.0; toAmount = 85.0; price = 38.0; }
    { player = players.[2]; fromAmount = 85.0; toAmount = 120.0; price = 32.0; }
]

// TODO: this should be calculated;
let routes = [
    { id = 1; player = players.[1]; edges = [ edges.[0], Direction.Negative ]; }
    { id = 1; player = players.[2]; edges = [ edges.[0], Direction.Negative ]; }
]

let dataset : DataSet = {
    nodes = nodes;
    edges = edges;
    players = players;
    demands = demands;
    sourcePrices = prices;
    transferPrices = edgePrices;
    routes = routes;
}

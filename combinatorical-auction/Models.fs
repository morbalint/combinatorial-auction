module CombinatorialAuction.Models

type Node = {
    production: float;
    id: int
}

type Edge = {
    id: int;
    fromNode : Node;
    toNode : Node;
    capacityPositive : float;
    capacityNegative : float;
}

type Player = {
    id: int;
    node: Node;
}

// piece of constant inverse demand curve
type Demand = {
    player: Player;
    fromAmount: float;
    toAmount: float;
    price: float;
}

type SourcePrice = {
    fromProducer: Node;
    toConsumer: Player;
    price: float;
}

type TransferPrice = {
    forPlayer: Player;
    onEdge: Edge;
    price: float;
}

type Direction = Positive = 1 | Negative = -1

type Route = {
    id: int;
    player: Player;
    edges: (Edge * Direction) list;
}

type DataSet = {
    nodes: Node list
    edges: Edge list
    players: Player list
    demands: Demand list
    sourcePrices: SourcePrice list
    transferPrices: TransferPrice list
    Routes: Route list
}

type TransportRoute = {
    id: int;
    player: Player;
    edges: (Edge * Direction) list;
    capacity: float;
    price: float;
}

type Bid = {
    route: TransportRoute;
    quantity: float;
    totalPrice: float;
}

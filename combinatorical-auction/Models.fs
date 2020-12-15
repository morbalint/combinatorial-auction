module CombinatorialAuction.Models

type Node = {
    id: int
}

type Edge = {
    id: int;
    fromNode : Node;
    toNode : Node;
    capacity : float;
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

type TransferPrice = {
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
    sourcePrice: float
    transferPrices: TransferPrice list
    routes: Route list
}

type TransportRoute = {
    id: int;
    player: Player;
    edges: (Edge * Direction) list;
    capacity: float;
    unitPrice: float;
}

type Bid = {
    route: TransportRoute;
    quantity: float;
    totalPrice: float;
}

type BidResult = {
    bid: Bid
    acceptance: float
    payment: float
}

type ResultSegment = {
    player: Player;
    edge: Edge;
    capacity: float;
    unitPrice: float;
}

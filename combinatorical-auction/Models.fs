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

type TransferCost = {
    onEdge: Edge;
    forPlayer: Player;
    price: float;
}

type SourcePrices = {
    fromProducer: Node;
    toConsumer: Player;
    price: float;
}

// piece of constant inverse demand curve
type Demand = {
    player: Player;
    fromAmount: float;
    toAmount: float;
    price: float;
}

type EdgePrice = {
    player: Player;
    edge: Edge;
    price: float;
}

type Direction =
    | Positive
    | Negative

type Transport = {
    player: Player;
    onEdge: Edge;
    Price: float;
    amount: float;
    direction: Direction;
}

type PartialRoute = {
    id: int;
    player: Player;
    edges: (Edge * Direction) list;
}

type Route = {
    id: int;
    player: Player;
    edges: (Edge * Direction) list;
    capacity: float;
    price: float;
}

type Bid = {
    route: Route;
    quantity: float;
    totalPrice: float;
}

type BidViewModel = {
    id: int;
    routeId: int;
    playerId: int;
    quantity: float;
    totalPrice: float;
}

type ConstraintRow = {
    weights: float list;
    upperBound: float;
}

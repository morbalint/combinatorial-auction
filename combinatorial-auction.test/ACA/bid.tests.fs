module CombinatorialAuction.ACA.Tests.bid

open Xunit
open CombinatorialAuction.Models
open CombinatorialAuction.ACA

let player : Player = { id = 1; node = { id = 1; } }

let make_route capacity unit_price = {
    id = 1;
    player = player;
    edges = [];
    capacity = capacity
    unitPrice = unit_price
}

let route_100_10 = make_route 100. 10.

[<Fact>]
let ``Empty demands return 0 capacity`` () =

    // Arrange
    let demands = []
    let route = make_route 100. 10.

    // Act
    let result_route, result_demands = bid demands route

    // Assert
    Assert.Equal(0. , result_route.capacity)
    Assert.Empty(result_demands)

[<Fact>]
let ``Empty route returns same route and same demands`` () =

    // Arrange
    let demands = [
        { fromAmount = 0.; toAmount = 50.; price = 20.; player = player; };
    ]
    let route = make_route 0. 10.

    // Act
    let result_route, result_demands = bid demands route

    // Assert
    Assert.Equal(0. , result_route.capacity)
    Assert.Equal<Demand>(demands, result_demands)

[<Fact>]
let ``3 demands 2 completely fulfilled, 1 partially`` () =

    // Arrange
    let demands = [
        { fromAmount = 0.; toAmount = 50.; price = 20.; player = player; };
        { fromAmount = 50.; toAmount = 70.; price = 17.; player = player; };
        { fromAmount = 70.; toAmount = 110.; price = 12.; player = player; };
    ]
    let expected = [
        { fromAmount = 100.; toAmount = 110.; price = 12.; player = player; };
    ]

    // Act
    let result_route, result_demands = bid demands route_100_10

    // Assert
    Assert.Equal(100. , result_route.capacity)
    Assert.Equal<Demand>(expected, result_demands)

[<Fact>]
let ``2 demands 1 fulfilled, 1 price too high`` () =

    // 2 demands 1 fulfilled, 1 price too high
    let demands = [
        { fromAmount = 0.; toAmount = 42.; price = 22.; player = player; };
        { fromAmount = 42.; toAmount = 70.; price = 7.; player = player; };
    ]

    // Act
    let result_route, result_demands = bid demands route_100_10

    // Assert
    Assert.Equal(42. , result_route.capacity)
    Assert.Equal<Demand>(demands.Tail, result_demands)

[<Fact>]
let ``One demand price too high`` () =

    // Arrange
    // 1 demand price too high
    let demands = [ { fromAmount = 0.; toAmount = 50.; price = 2.; player = player; };]

    // Act
    let result_route, result_demands = bid demands route_100_10

    // Assert
    Assert.Equal(0. , result_route.capacity)
    Assert.Equal<Demand>(demands, result_demands)

[<Fact>]
let ``One demand fulfilled completely`` () =

    // Arrange
    // 1 demand fulfilled completely
    let demands = [ { fromAmount = 0.; toAmount = 50.; price = 20.; player = player; }; ]

    // Act
    let result_route, result_demands = bid demands route_100_10

    // Assert
    Assert.Equal(50. , result_route.capacity)
    Assert.Equal<Demand>([], result_demands)

[<Fact>]
let ``Does not bid on equal price`` () =

    // Arrange
    let demands = [ { fromAmount = 0.; toAmount = 37.; price = 10.; player = player; }; ]

    // Act
    let result_route, result_demands = bid demands route_100_10

    // Assert
    Assert.Equal(0. , result_route.capacity)
    Assert.Equal<Demand>(demands, result_demands) // no change expected, unfulfilled demand

let ``Future demand does NOT "pay" for full route bid`` () =

    // Arrange
    let demands = [
        { fromAmount = 0.; toAmount = 50.; price = 0.; player = player; };
        { fromAmount = 50.; toAmount = 70.; price = 25.; player = player; };
        { fromAmount = 70.; toAmount = 110.; price = 22.; player = player; };
    ]

    // Act
    let result_route, result_demands = bid demands route_100_10

    // Assert
    Assert.Equal(100. , result_route.capacity)

    let expected_demands = [ { demands.[2] with fromAmount = 100. } ]

    Assert.Equal<Demand>(expected_demands, result_demands)

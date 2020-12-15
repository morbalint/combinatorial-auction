module Tests

open Xunit
open CombinatorialAuction.Models

[<Fact>]
let ``My test`` () =

    // Arrange

    let node1 = { id = 1; production = -100. }
    let node2 = { id = 2; production = -100. }
    let player : Player = { id = 1; node = { id = 1; production = 100.; }   }
    let qq : TransportRoute = { id = 12; player = player; capacity = 10.; edges = [ { id = 1; fromNode = node1; toNode = node2; capacityPositive = 50.; capacityNegative = 50.; }, Direction.Negative ]; unitPrice = 3. }

    // Act
    let result = CombinatorialAuction.ACA.closed_edges [ qq ]

    // Assert
    Assert.NotEmpty result

    Assert.True(true)

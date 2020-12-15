module CombinatorialAuction.ACA.Tests.closed_edges

open Xunit
open CombinatorialAuction.Models
open CombinatorialAuction.ACA

[<Fact>]
let ``My test`` () =

    // Arrange

    let node1 = { id = 1; }
    let node2 = { id = 2; }
    let player : Player = { id = 1; node = { id = 1; }   }
    let qq : TransportRoute = { id = 12; player = player; capacity = 10.; edges = [ { id = 1; fromNode = node1; toNode = node2; capacity = 50.; }, Direction.Negative ]; unitPrice = 3. }

    // Act
    let result = closed_edges [ qq ]

    // Assert
    Assert.NotEmpty result

    Assert.True(true)

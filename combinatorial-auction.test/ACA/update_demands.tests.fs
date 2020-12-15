module CombinatorialAuction.ACA.Tests.update_demand

open Xunit
open CombinatorialAuction.Models
open CombinatorialAuction.ACA

// how to avoid ?? // TS has duck typing, it would work there.
let player = { id = 1; node = { id = 1; } }

// Arrange
let ``Test data for happy path`` = [|
    [| 300. :> obj; [
        { fromAmount = 0.; toAmount = 100.; price = 10.; player = player };
        ] :> obj; // input demands
        [
            { fromAmount = 30.; toAmount = 100.; price = 10.; player = player };
        ] :> obj; // expected output
    |];
    [| 300. :> obj; [
        { fromAmount = 0.; toAmount = 100.; price = 10.; player = player };
        { fromAmount = 100.; toAmount = 150.; price = 5.; player = player };
        ] :> obj; // input demands
        [
            { fromAmount = 30.; toAmount = 100.; price = 10.; player = player };
            { fromAmount = 100.; toAmount = 150.; price = 5.; player = player };
        ] :> obj; // expected output
    |];
    [| 1000. :> obj; [
        { fromAmount = 0.; toAmount = 100.; price = 10.; player = player };
        { fromAmount = 100.; toAmount = 150.; price = 5.; player = player };
        ] :> obj; // input demands
        [
            { fromAmount = 100.; toAmount = 150.; price = 5.; player = player };
        ] :> obj; // expected output
    |];
    [| 300. :> obj; [
        { fromAmount = 0.; toAmount = 20.; price = 10.; player = player };
        { fromAmount = 20.; toAmount = 50.; price = 5.; player = player };
        ] :> obj; // input demands
        [
            { fromAmount = 40.; toAmount = 50.; price = 5.; player = player };
        ] :> obj; // expected output
    |];
    [| 350. :> obj; [
        { fromAmount = 0.; toAmount = 20.; price = 10.; player = player };
        { fromAmount = 20.; toAmount = 50.; price = 5.; player = player };
        { fromAmount = 50.; toAmount = 90.; price = 3.; player = player };
        ] :> obj; // input demands
        [
            { fromAmount = 50.; toAmount = 90.; price = 3.; player = player };
        ] :> obj; // expected output
    |];
|]

[<Theory>]
[<MemberData(nameof ``Test data for happy path``)>]
let ``Happy path``
    decrement
    demands
    expectedResult
    =

    // Act
    let result = update_demands decrement demands

    // Assert
    Assert.Equal<Demand>(expectedResult, result)

[<Fact>]
let ``Zero decrement changes nothing`` () =

    // Arrange

    let decrement = 0.

    let demands = [
        { fromAmount = 0.; toAmount = 20.; price = 10.; player = player };
        { fromAmount = 20.; toAmount = 50.; price = 5.; player = player } ]

    // Act
    let result = update_demands decrement demands

    // Assert
    Assert.Equal<Demand>(demands, result)

// Arrange
let ``Test data for too large decrement removes everything `` = [|
    [| 1000. :> obj ; [
        { fromAmount = 0.; toAmount = 20.; price = 10.; player = player };
        { fromAmount = 20.; toAmount = 50.; price = 5.; player = player };
        ] :> obj |];
    [| 350. :> obj ; [
        { fromAmount = 0.; toAmount = 20.; price = 10.; player = player };
        { fromAmount = 20.; toAmount = 50.; price = 5.; player = player };
        ] :> obj |];
    [| 500. :> obj ; [
        { fromAmount = 0.; toAmount = 20.; price = 10.; player = player };
        { fromAmount = 20.; toAmount = 50.; price = 5.; player = player };
        { fromAmount = 50.; toAmount = 100.; price = 3.; player = player };
        ] :> obj |];
|]

[<Theory>]
[<MemberData(nameof ``Test data for too large decrement removes everything `` )>]
let ``Too large decrement removes everything`` decrement demands =

    // Act
    let result = update_demands decrement demands

    // Assert
    Assert.Empty(result)

module Tests.Cube

open Xunit
open FsUnit.Xunit
open Cube

[<Fact>]
let ``Solved cube has 12 edges`` () = 
    solvedCube.EdgePositions.Length |> should equal 12
    solvedCube.EdgeFlips.Length |> should equal 21

[<Fact>]
let ``Solved cube has 8 corners`` () = 
    solvedCube.CornerPositions.Length |> should equal 8
    solvedCube.CornerTwists.Length |> should equal 8


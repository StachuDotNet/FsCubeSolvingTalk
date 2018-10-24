module Tests.TreeSearch

open Xunit
open FsUnit.Xunit
open Cube
open Scramble
open TreeSearch
open Moves

[<Fact>]
let ``Scrambled cube should be solvable with depth-first search`` () =
    let depth = 3

    let scrambledCube = Scramble solvedCube MoveSets.All depth
    let actualCube = fst scrambledCube

    let response =  BasicTreeSearch actualCube depth MoveSets.All
    (response |> List.length) > 0 |> should equal true

[<Fact>]
let ``Scrambled cube should be solvable with IDA search`` () =
    let depth = 3

    let scrambledCube = Scramble solvedCube MoveSets.All depth
    let actualCube = fst scrambledCube

    let response =  IDASearch actualCube depth MoveSets.All
    (response |> List.length) > 0 |> should equal true

[<Fact>]
let ``2-gen scramble should be solvable with IDA search`` () =
    let depth = 100

    let moves = [ { Label = "R2"; Transformation = R2 }; { Label = "U2"; Transformation = U2 } ]

    let scrambledCube = Scramble solvedCube moves depth
    let actualCube = fst scrambledCube

    let response =  IDASearch actualCube depth MoveSets.TwoGen
    (response |> List.length) > 0 |> should equal true
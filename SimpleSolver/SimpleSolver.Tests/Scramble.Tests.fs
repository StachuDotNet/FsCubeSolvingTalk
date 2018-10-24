module Tests.Scramble

open Xunit
open FsUnit.Xunit
open Cube
open Moves
open Scramble

[<Fact>]
let ``Scrambles should result in a scrambled cube...`` () =
    Scramble solvedCube MoveSets.All 25 |> should not' (equal solvedCube)

[<Fact>]
let ``Scrambles should not have repeated moves.`` () =
    1 |> should equal 2 
    (*
        not sure how to write this test.
        basically want to prevent things like [L L] or [L L2] or [L R' L2]
    *)
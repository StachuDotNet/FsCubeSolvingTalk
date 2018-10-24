module Tests.Execute

open Xunit
open FsUnit.Xunit
open Cube
open Execute
open Moves

[<Fact>]
let ``Given a solved cube, R should mess the cube up`` () =
    solvedCube 
    |> Execute R 
    |> should  not' (equal solvedCube)

[<Fact>]
let ``Given a solved cube, R four times should result in a solved cube`` () =
    let endState = ExecuteN solvedCube R 4
    endState |> should equal solvedCube

[<Fact>]
let ``n four times should result in a solved cube`` () =
    let movesToTry = [| U; U'; R; R'; F; F'; D; D'; L; L'; B; B' |]

    for move in movesToTry do
         (ExecuteN solvedCube move 4) |> should equal solvedCube



[<Fact>]
let ``n2 should have no twisted corners or flipped edges`` () =
    let movesToTry = [| U2; F2; L2; R2; D2; B2 |]

    for move in movesToTry do
        (solvedCube |> Execute move).CornerTwists
            |> Array.exists (fun z-> z <> 0) 
            |> should equal false

        (solvedCube |> Execute move).EdgeFlips   
            |> Array.exists (fun z-> z <> 0) 
            |> should equal false


[<Fact>]
let ``n2 twice should result in a solved cube`` () =
    let movesToTry = [| U2; F2; L2; R2; D2; B2 |]

    for move in movesToTry do
         (ExecuteN solvedCube move 2) |> should equal solvedCube

[<Fact>]
let ``U-perm thrice results in a solved cube`` () =
    let uPerm = R2 |> Execute U |> Execute R |> Execute U |> Execute R' |> Execute U' |> Execute R' |> Execute U' |> Execute R' |> Execute U |> Execute R'

    let endState = ExecuteN solvedCube uPerm 3

    endState |> should equal solvedCube

[<Fact>]
let ``T-perm twice results in a solved cube`` () =
    let tPerm = R |> Execute U |> Execute R' |> Execute U' |> Execute R' |> Execute F |> Execute R2 |> Execute U' |> Execute R' |> Execute U' |> Execute R |> Execute U |> Execute R' |> Execute F'

    let endState = ExecuteN solvedCube tPerm 2

    endState |> should equal solvedCube

[<Fact>]
let ``Moveset 'all' should have 18 available turns.`` () =
    MoveSets.All |> List.length |> should equal 18 
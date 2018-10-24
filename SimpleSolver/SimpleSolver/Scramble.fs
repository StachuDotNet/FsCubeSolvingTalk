module Scramble

open Cube
open Execute

let Scramble cubeState (movesAllowed : CubeTransformation list) n =
    let rnd = new System.Random()

    let mutable scrambledCube = cubeState
    let mutable movesPerformed = []

    for i = 1 to n do
        let nextMove = movesAllowed.[rnd.Next(movesAllowed.Length)]
        scrambledCube <- scrambledCube |> Execute nextMove.Transformation
        movesPerformed <- nextMove.Label :: movesPerformed

    (scrambledCube, System.String.Join(" ", movesPerformed))



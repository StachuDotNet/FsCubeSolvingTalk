module TreeSearch

open Cube
open Execute

let BasicTreeSearch scrambledState maxDepth (movesAllowed: CubeTransformation list) =
    let solutions = ref []

    let rec BasicTreeSearch_Internal cubeState d (movesSoFar : string list) =
        if cubeState = solvedCube then 
           solutions:= (movesSoFar |> String.concat " " ) :: !solutions 

        elif d > 0 then
            movesAllowed 
            |> List.iter
                (fun move -> 
                    let applied = cubeState |> Execute move.Transformation
                    BasicTreeSearch_Internal applied (d-1) (move.Label :: movesSoFar) 
                ) 

    BasicTreeSearch_Internal scrambledState maxDepth []
    solutions.Value


let IDASearch scrambledState maxDepth (movesAllowed: CubeTransformation list) =
    let mutable solutions = []

    for a in 1 .. maxDepth do
        printfn "Searching depth %i" a
        if solutions.IsEmpty then

            let treeSearchResponse = BasicTreeSearch scrambledState a movesAllowed

            if not treeSearchResponse.IsEmpty then
                solutions <- treeSearchResponse

    solutions


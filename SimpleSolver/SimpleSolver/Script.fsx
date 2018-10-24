#load "Cube.fs"; open Cube
#load "Execute.fs"; open Execute
#load "Moves.fs"; open Moves
#load "Scramble.fs"; open Scramble
#load "TreeSearch.fs"; open TreeSearch

let depth = 100

let moves = [ { Label = "R2"; Transformation = R2 }; { Label = "U2"; Transformation = U2 } ]

let scrambledCube = Scramble solvedCube moves depth
let actualCube = fst scrambledCube

let response =  IDASearch actualCube depth moves
printfn "%A" response
System.Console.ReadLine() |> ignore


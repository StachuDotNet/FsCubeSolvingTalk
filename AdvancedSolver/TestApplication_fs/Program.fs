// Learn more about F# at http://fsharp.org
// See the 'F# Tutorial' project for more help.

open System
open RubiksCubeLib.RubiksCube
open RubiksCubeLib.Solving
open TwoPhaseAlgorithmSolver
open RubiksCubeLib
open BeginnerSolver

let cube = new Rubik()
let scramble = cube.Scramble(50)
let scrambleMoves = Algorithm.RemoveUnnecessaryMoves scramble

printfn "Scramble: %s"  (scrambleMoves.ToString())

let solvers = 
    [ 
        "2-phase", new TwoPhaseAlgorithm() :> CubeSolver
        "beginner", new BeginnerSolver() :> CubeSolver
    ]

solvers
    |> List.iter(fun (name, solver) ->
        solver.OnSolutionStepCompleted.Add(fun evArgs -> 
            let alg  = (Algorithm.RemoveUnnecessaryMoves evArgs.Algorithm).ToString()
            printfn "[%s]\t\tStep %s solved in %i with %s" name evArgs.Step evArgs.Milliseconds alg
        )
        solver.TrySolveAsync(cube)
    )

Console.Read() |> ignore
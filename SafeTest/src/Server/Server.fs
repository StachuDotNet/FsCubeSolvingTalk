open System.IO
open FSharp.Control.Tasks.V2
open Giraffe
open Saturn
open Shared
open RubiksCubeLib
open RubiksCubeLib.RubiksCube
open RubiksCubeLib.Solving
open TwoPhaseAlgorithmSolver
open BeginnerSolver

let publicPath = Path.GetFullPath "../Client/public"
let port = 8085us

let getSolution (solverParams: GetSolutionParams) = async {
    let cube = new Rubik()
    let scrambleAlg = Algorithm(solverParams.Scramble.Trim())
    cube.ScrambleAlg scrambleAlg
    
    printfn "Method: %A" solverParams.SolvingMethod

    let solver : CubeSolver = 
        match solverParams.SolvingMethod with
        | SolvingMethod.BeginnerMethod -> 
            new BeginnerSolver() :> CubeSolver
        | SolvingMethod.TwoPhase -> 
            new TwoPhaseAlgorithm() :> CubeSolver

    solver.Solve(cube)
    
    let alg  = (RubiksCubeLib.Algorithm.RemoveUnnecessaryMoves solver.Algorithm).ToString()

    return alg
}

let counterApi = {
    getSolution = getSolution
}

let webApp =
    router {
        post "/api/getSolution" (fun  next ctx ->
            task {
                let! wtfParams = ctx.BindJsonAsync<GetSolutionParams>()
                let! counter = getSolution wtfParams
                return! Successful.OK counter next ctx
            }
        )
    }

let app = application {
    url ("http://0.0.0.0:" + port.ToString() + "/")
    use_router webApp
    memory_cache
    use_static publicPath
    use_gzip
}

run app

namespace RubiksCubeLib.Solver

open RubiksCubeLib.RubiksCube
open System.Linq
open RubiksCubeLib

type TestScenario (rubik: Rubik, moves: Algorithm) = 
    member val Rubik = rubik.DeepClone() with get
    member val Algorithm = moves with get, set

    new(rubik: Rubik, move: LayerMove) = 
        let alg = Algorithm(move.ToString())
        TestScenario(rubik, alg)

    member this.TestCubePosition (c: Cube) (endPos: CubeFlag) : bool = 
        for move in this.Algorithm.Moves do
            this.Rubik.RotateLayer move

        (this.RefreshCube c).Position.HasFlag endPos

    member private this.RefreshCube (c: Cube) : Cube = 
        this.Rubik.Cubes.First(fun cu -> CollectionMethods.ScrambledEquals cu.Colors c.Colors)

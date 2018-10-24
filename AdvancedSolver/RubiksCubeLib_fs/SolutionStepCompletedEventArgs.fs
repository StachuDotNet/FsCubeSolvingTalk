namespace RubiksCubeLib.Solver

open RubiksCubeLib
open System

type SolutionStepCompletedEventArgs (step: string, finished: bool, moves: Algorithm, milliseconds: int) = 
    inherit EventArgs()

    member val Step = step with get
    member val Finished = finished with get
    member val Algorithm = moves with get
    member val Milliseconds = milliseconds with get

namespace RubiksCubeLib

open System

type SolutionErrorEventArgs (step: string, message: string) = 
    inherit EventArgs()

    let mutable step = step
    let mutable message = message

    /// The color of this face
    member this.Step
        with get() = step
        and set(value) = step <- value

    /// The color of this face
    member this.Message
        with get() = message
        and set(value) = message <- value

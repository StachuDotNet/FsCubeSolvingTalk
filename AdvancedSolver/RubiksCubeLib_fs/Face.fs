namespace RubiksCubeLib

open System
open System.Drawing

/// Represents a face of a Rubik
[<Serializable>]
type Face (color: System.Drawing.Color, position: FacePosition) = 
    let mutable color = color
    let mutable position = position

    /// Empty constructor (black color, no position)
    new() = Face(Color.Black, FacePosition.None)

    /// The color of this face
    member this.Color
        with get() = color
        and set(value) = color <- value

    ///// The position of this face
    member this.Position
        with get() = position
        and set(value) = position <- value
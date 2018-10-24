/// Represents the standard cube in solved form
module RubiksCubeLib.UniCube

open System.Collections.Generic
open System.Drawing

/// Returns a collection with six entries containing the six faces with a black color
let GenFaces (): IEnumerable<Face> =
    [|
        new Face(Color.Black, FacePosition.Front)
        new Face(Color.Black, FacePosition.Back)
        new Face(Color.Black, FacePosition.Top)
        new Face(Color.Black, FacePosition.Bottom)
        new Face(Color.Black, FacePosition.Right)
        new Face(Color.Black, FacePosition.Left)
    |] |> Seq.ofArray
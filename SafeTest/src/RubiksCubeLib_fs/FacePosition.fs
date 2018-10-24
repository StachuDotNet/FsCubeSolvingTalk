namespace RubiksCubeLib

/// Defines the position of a face
type FacePosition = 
    | None = 0
    | Top = 1
    | Bottom = 2
    | Left = 4
    | Right = 8
    | Back = 16
    | Front = 32
module FacePosition = 
    let XPos = FacePosition.Right ||| FacePosition.Left
    let YPos = FacePosition.Top ||| FacePosition.Bottom
    let ZPos = FacePosition.Front ||| FacePosition.Back
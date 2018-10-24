module Cube

type CubeState = 
    { EdgePositions: int array
      EdgeFlips: int array
      CornerPositions: int array
      CornerTwists: int array }

let solvedCube = 
    { EdgePositions = [|0..11|]
      EdgeFlips = Array.create 12 0
      CornerPositions = [|0..7|]
      CornerTwists = Array.create 8 0 }

type CubeTransformation = {
    Label: string
    Transformation: CubeState
}
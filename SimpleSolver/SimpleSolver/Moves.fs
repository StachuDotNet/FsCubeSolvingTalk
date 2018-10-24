module Moves

open Cube
open Execute

let R = 
    { EdgePositions   = [| 0; 10; 2; 3; 4; 8; 6; 7; 1; 9; 5; 11 |]
      EdgeFlips       = Array.create 12 0
      CornerPositions = [| 1; 7; 2; 3; 0; 5; 6; 4 |]   
      CornerTwists    = [| 1; 2; 0; 0; 2; 0; 0; 1 |] } 

let U = 
    { EdgePositions   = [| 3; 0; 1; 2; 4; 5; 6; 7; 8; 9; 10; 11|]
      EdgeFlips       = Array.create 12 0
      CornerPositions = [| 3; 0; 1; 2; 4; 5; 6; 7 |]
      CornerTwists    = Array.create 8 0 }

let F = 
    { EdgePositions   = [| 8; 1; 2; 3; 9; 5; 6; 7; 4; 0; 10; 11 |]
      EdgeFlips       = [| 1; 0; 0; 0; 1; 0; 0; 0; 1; 1;  0;  0 |]
      CornerPositions = [| 4; 1; 2; 0; 5; 3; 6; 7 |]
      CornerTwists    = [| 2; 0; 0; 1; 1; 2; 0; 0 |] }

let L = 
    { EdgePositions   = [| 0; 1; 2; 9; 4; 5; 6; 11; 8; 7; 10; 3 |]
      EdgeFlips      = Array.create 12 0
      CornerPositions= [| 0; 1; 3; 5; 4; 6; 2; 7 |]
      CornerTwists   = [| 0; 0; 1; 2; 0; 1; 2; 0 |] }

let D = 
    { EdgePositions   = [| 0; 1; 2; 3; 5; 6; 7; 4; 8; 9; 10; 11 |]
      EdgeFlips       = Array.create 12 0
      CornerPositions = [| 0; 1; 2; 3; 7; 4; 5; 6 |]
      CornerTwists    = Array.create 8 0 }

let B = 
    { EdgePositions   = [| 0; 1; 11; 3; 4; 5; 10; 7; 8; 9; 2; 6 |]
      EdgeFlips       = [| 0; 0;  1; 0; 0; 0;  1; 0; 0; 0; 1; 1 |]
      CornerPositions = [| 0; 2; 6; 3; 4; 5; 7; 1 |]
      CornerTwists    = [| 0; 1; 2; 0; 0; 0; 1; 2 |] }

let R2 = ExecuteN solvedCube R 2
let R' = ExecuteN solvedCube R 3

let U2 = ExecuteN solvedCube U 2
let U' = ExecuteN solvedCube U 3

let F2 = ExecuteN solvedCube F 2
let F' = ExecuteN solvedCube F 3

let L2 = ExecuteN solvedCube L 2
let L' = ExecuteN solvedCube L 3

let D2 = ExecuteN solvedCube D 2
let D' = ExecuteN solvedCube D 3

let B2 = ExecuteN solvedCube B 2
let B' = ExecuteN solvedCube B 3

type MoveSets =
    static member TwoGen = 
        [ { Label  = "R";  Transformation = R } 
          { Label  = "R'"; Transformation = R' } 
          { Label  = "R2"; Transformation = R2 }
          { Label  = "U";  Transformation = U } 
          { Label  = "U'"; Transformation = U' } 
          { Label  = "U2"; Transformation = U2 } ]

    static member All = 
        [ { Label  = "F";  Transformation = F }; 
          { Label  = "F'"; Transformation = F' }; 
          { Label  = "F2"; Transformation = F2 }; 
          { Label  = "U";  Transformation = U }; 
          { Label  = "U'"; Transformation = U' }; 
          { Label  = "U2"; Transformation = U2 }; 
          { Label  = "R";  Transformation = R }; 
          { Label  = "R'"; Transformation = R' }; 
          { Label  = "R2"; Transformation = R2 }; 
          { Label  = "L";  Transformation = L }; 
          { Label  = "L'"; Transformation = L' }; 
          { Label  = "L2"; Transformation = L2 }; 
          { Label  = "D";  Transformation = D }; 
          { Label  = "D'"; Transformation = D' }; 
          { Label  = "D2"; Transformation = D2 }; 
          { Label  = "B";  Transformation = B }; 
          { Label  = "B'"; Transformation = B' }; 
          { Label  = "B2"; Transformation = B2 }; ]


module Execute

open Cube

let Execute cubeState t = 
    { EdgePositions   = 
        [| for a in 0 .. 11 do 
            yield cubeState.EdgePositions.[t.EdgePositions.[a]] 
        |]
      EdgeFlips       = 
        [| for a in 0 .. 11 do 
            yield (cubeState.EdgeFlips.[t.EdgePositions.[a]] + t.EdgeFlips.[a]) % 2 
        |]
      CornerPositions = 
        [| for a in 0 .. 7 do 
            yield cubeState.CornerPositions.[t.CornerPositions.[a]] 
        |]
      CornerTwists    = 
        [| for a in 0 .. 7 do 
            yield (cubeState.CornerTwists.[t.CornerPositions.[a]] + t.CornerTwists.[a]) % 3 
        |] 
    }




let rec ExecuteN cubeState transformation n =
    match n with    
    | 0 -> cubeState
    | 1 -> Execute cubeState transformation
    | n when n > 1 -> ( ExecuteN cubeState transformation (n-1) ) |> Execute transformation
    | _ -> failwith "need moar n"

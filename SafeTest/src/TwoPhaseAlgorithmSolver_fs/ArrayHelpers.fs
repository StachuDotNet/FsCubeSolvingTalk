module TwoPhaseAlgorithmSolver.ArrayHelpers

let RotateLeft(arr: byte array,  r: int) = 
    let temp = arr.[0]

    for i = 0 to r-1 do
        arr.[i] <- arr.[i + 1]

    arr.[r] <- temp

let RotateRight(arr: byte array, l: int, r: int) =
    let temp = arr.[r]

    for i = r downto l+1 do
        arr.[i] <- arr.[i - 1]

    arr.[l] <- temp

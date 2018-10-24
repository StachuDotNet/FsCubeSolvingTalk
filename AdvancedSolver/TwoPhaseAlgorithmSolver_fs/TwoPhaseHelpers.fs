module TwoPhaseHelpers

open RubiksCubeLib.RubiksCube
open TwoPhaseAlgorithmSolver
open RubiksCubeLib
open RubiksCubeLib.Solver
open System.Linq

let ToCoordCube(rubik: Rubik): CoordCube  = 
    // get corner perm and orientation
    let corners = [|"UFR"; "UFL"; "UBL"; "URB"; "DFR"; "DFL"; "DBL"; "DRB" |]

    let cornerPermutation: byte[] = Array.zeroCreate ((int)TwoPhaseConstants.N_CORNER)
    let cornerOrientation: byte[] = Array.zeroCreate ((int)TwoPhaseConstants.N_CORNER)

    for i = 0 to ((int)TwoPhaseConstants.N_CORNER - 1) do
        let pos: CubeFlag = CubeFlagService.Parse(corners.[i])
        let matchingCube: Cube = rubik.Cubes.First(fun c -> c.Position.Flags = pos)
        let targetPos: CubeFlag = rubik.GetTargetFlags(matchingCube)

        cornerOrientation.[i] <- (byte)(Solvability.GetOrientation rubik matchingCube)

        for j = 0 to ((int)TwoPhaseConstants.N_CORNER - 1) do 
            if (corners.[j] = CubeFlagService.ToNotationString(targetPos)) then
                cornerPermutation.[i] <- (byte)(j + 1)

    // get edge perm and orientation
    let edges: string[] = [|"UR"; "UF"; "UL"; "UB"; "DR"; "DF"; "DL"; "DB"; "FR"; "FL"; "BL"; "RB" |]
    let edgePermutation: byte[] = Array.zeroCreate ((int)TwoPhaseConstants.N_EDGE)
    let edgeOrientation: byte[] = Array.zeroCreate ((int)TwoPhaseConstants.N_EDGE)
    
    for i = 0 to ((int)TwoPhaseConstants.N_EDGE - 1) do
        let pos: CubeFlag = CubeFlagService.Parse(edges.[i])
        let matchingCube: Cube = rubik.Cubes.Where(fun c -> c.IsEdge).First(fun c -> c.Position.Flags.HasFlag(pos))
        let targetPos: CubeFlag = rubik.GetTargetFlags(matchingCube)

        edgeOrientation.[i] <- (byte)(Solvability.GetOrientation rubik matchingCube)

        for j = 0 to ((int)TwoPhaseConstants.N_EDGE - 1) do
            if CubeFlagService.ToNotationString(targetPos).Contains(edges.[j]) then
                edgePermutation.[i] <- (byte)(j + 1)

    CoordCube(cornerPermutation, edgePermutation, cornerOrientation, edgeOrientation)

let IntsToLayerMove(axis: int, power: int): LayerMove =
    let axes: string[] = [| "U"; "R"; "F"; "D"; "L"; "B" |]
    let movePart = axes.[axis]

    let amountPart = 
        if power = 3 then 
            "'"
        else if power = 2 then
            "2"
        else 
            ""

    LayerMove.Parse(movePart + amountPart)

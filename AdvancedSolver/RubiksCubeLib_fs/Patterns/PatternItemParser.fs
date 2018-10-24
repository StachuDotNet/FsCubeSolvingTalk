module RubiksCubeLib.Solver.PatternItemParser

open RubiksCubeLib
open System.Linq

/// <summary>
/// Parses a string to a pattern item
/// </summary>
/// <param name="s">The string to be parsed
/// 1: "currentPos, targetPos, currentOrientation"
/// 2: "currentPos, currentOrientation" => any target position
/// 3: "currentPos, targetPos" => any orientation
/// </param>
let Parse (s: string) : PatternItem = 
    let split = s.Split(',')

    match split.Length with
    | 1 -> 
        let currPos = CubeFlagService.Parse split.[0]

        let testCount = CommonPositions.AllPositions |> Seq.where(fun p -> p.Flags = currPos) |> Seq.length
        if testCount <> 1 then
            failwith "At least one orientation or position is not possible"

        new PatternItem(new CubePosition(currPos), Orientation.Correct, currPos)
    | 2 -> 
        let currPos = CubeFlagService.Parse split.[0]
        let pos = CubeFlagService.Parse split.[1]

        let mutable orientation = 0

        let testCount1 = CommonPositions.AllPositions |> Seq.where(fun p -> p.Flags = currPos) |> Seq.length

        if testCount1 <> 1 || ((not <| System.Int32.TryParse(split.[1], &orientation)) && (CommonPositions.AllPositions |> Seq.where(fun p -> p.Flags = pos) |> Seq.length |> (<>) 1)) then
            failwith "At least one orientation or position is not possible"

        new PatternItem(new CubePosition(currPos), enum<Orientation>(orientation), pos);
    | 3 -> 
        // check valid cube position
        let currPos = CubeFlagService.Parse split.[0]
        let targetPos = CubeFlagService.Parse split.[1]


        if (not <| CommonPositions.AllPositions.Contains(new CubePosition(currPos))) || (not <| CommonPositions.AllPositions.Contains(new CubePosition(targetPos))) then
            failwith "At least one position does not exist"

        // check valid orientation
        let mutable orientation = 0

        if not <| System.Int32.TryParse(split.[2], &orientation) then
            failwith "At least one orientation is not possible"

        new PatternItem(new CubePosition(currPos), enum<Orientation>(orientation), targetPos);
    | _ ->
        failwith "Parsing error"

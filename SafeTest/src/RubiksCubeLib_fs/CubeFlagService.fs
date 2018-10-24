module RubiksCubeLib.CubeFlagService

open System.Text
open System

/// <summary> Returns true if the flag is an XFlag only </summary>
/// <param name="flag">Defines the CubeFlag to be analyzed</param>
let IsXFlag flag = CubeFlag.XFlags.HasFlag flag
    
/// <summary> Returns true if the flag is an YFlag only </summary>
/// <param name="flag">Defines the CubeFlag to be analyzed</param>
let IsYFlag flag = CubeFlag.YFlags.HasFlag flag

/// <summary> Returns true if the flag is an ZFlag only </summary>
/// <param name="flag">Defines the CubeFlag to be analyzed</param>
let IsZFlag flag = CubeFlag.ZFlags.HasFlag flag
        
let GetFlags (flags: CubeFlag): seq<CubeFlag> =
    Enum.GetValues(flags.GetType())
    |> Seq.cast<CubeFlag>
    |> Seq.where(fun f -> flags.HasFlag f)

/// <summary> Returns the first flag in the first parameter which the second parameter does not contain </summary>
/// <param name="flags">Defines the posiible flags to be returned</param>
/// <param name="invalid">Defines the invalid flags</param>
let FirstNotInvalidFlag (flags: CubeFlag) (invalid: CubeFlag) : CubeFlag = 
    GetFlags flags
    |> Seq.find(fun f -> not <| invalid.HasFlag f)
    
/// <summary>
/// Converts a FacePosition into a CubeFlag
/// </summary>
/// <param name="facePos">Defines the FacePostion to be converted</param>
let FromFacePosition facePos = 
    match facePos with
    | FacePosition.Top -> CubeFlag.TopLayer
    | FacePosition.Bottom -> CubeFlag.BottomLayer
    | FacePosition.Left -> CubeFlag.LeftSlice
    | FacePosition.Right -> CubeFlag.RightSlice
    | FacePosition.Back -> CubeFlag.BackSlice
    | FacePosition.Front -> CubeFlag.FrontSlice
    | _ -> CubeFlag.None
    
/// <summary> Converts a CubeFlag into a FacePosition </summary>
/// <param name="cubePos">Defines the CubeFlag to be converted</param>
let ToFacePosition cubePos = 
    match cubePos with
    | CubeFlag.TopLayer -> FacePosition.Top
    | CubeFlag.BottomLayer -> FacePosition.Bottom
    | CubeFlag.LeftSlice -> FacePosition.Left
    | CubeFlag.RightSlice -> FacePosition.Right
    | CubeFlag.FrontSlice -> FacePosition.Front
    | CubeFlag.BackSlice -> FacePosition.Back
    | _ -> FacePosition.None

/// <summary> Returns a CubeFlag which is the opposite of the parameter </summary>
/// <param name="flag">Defines the parameter to be analyzed</param>
let GetOppositeFlag flag = 
    match flag with 
    | CubeFlag.TopLayer -> CubeFlag.BottomLayer
    | CubeFlag.BottomLayer -> CubeFlag.TopLayer
    | CubeFlag.FrontSlice -> CubeFlag.BackSlice
    | CubeFlag.BackSlice -> CubeFlag.FrontSlice
    | CubeFlag.RightSlice -> CubeFlag.LeftSlice
    | CubeFlag.LeftSlice -> CubeFlag.RightSlice
    | CubeFlag.MiddleLayer
    | CubeFlag.MiddleSlice
    | CubeFlag.MiddleSliceSides -> flag
    | _ -> CubeFlag.None

/// Converts the given CubeFlag into a notation string
let ToNotationString (flag: CubeFlag) = 
    let notation = new StringBuilder()

    if flag.HasFlag CubeFlag.TopLayer then notation.Append "U" |> ignore
    if flag.HasFlag CubeFlag.BottomLayer then notation.Append "D" |> ignore
    if flag.HasFlag CubeFlag.FrontSlice then notation.Append "F" |> ignore
    if flag.HasFlag CubeFlag.RightSlice then notation.Append "R" |> ignore
    if flag.HasFlag CubeFlag.BackSlice then notation.Append "B" |> ignore
    if flag.HasFlag CubeFlag.LeftSlice then notation.Append "L" |> ignore
    if flag.HasFlag CubeFlag.MiddleSlice then notation.Append "S" |> ignore
    if flag.HasFlag CubeFlag.MiddleSliceSides then notation.Append "M" |> ignore
    if flag.HasFlag CubeFlag.MiddleLayer then notation.Append "E" |> ignore

    notation.ToString()
    
/// <summary> Returns the amount of flags in the parameter </summary>
/// <param name="flag">Defines the flag to be counted out</param>
let CountFlags flag = (GetFlags flag |> Seq.length) - 1

/// Parses a notation into a collection of cube flags
let Parse notation = 
    let mutable flags = CubeFlag.None

    for char in notation do
        match char with
        | 'R' -> flags <- flags ||| CubeFlag.RightSlice
        | 'L' -> flags <- flags ||| CubeFlag.LeftSlice
        | 'U' -> flags <- flags ||| CubeFlag.TopLayer
        | 'D' -> flags <- flags ||| CubeFlag.BottomLayer
        | 'F' -> flags <- flags ||| CubeFlag.FrontSlice
        | 'B' -> flags <- flags ||| CubeFlag.BackSlice
        | 'M' -> flags <- flags ||| CubeFlag.MiddleSliceSides
        | 'E' -> flags <- flags ||| CubeFlag.MiddleLayer
        | 'S' -> flags <- flags ||| CubeFlag.MiddleSlice
        | _ -> ()
        
    flags

/// <summary> Returns the first XFlag in the given CubeFlag </summary>
/// <param name="flags">Defines the CubeFlag to be analyzed</param>
let FirstXFlag flags = FirstNotInvalidFlag flags (CubeFlag.YFlags ||| CubeFlag.ZFlags)

/// <summary> Returns the first ZFlag in the given CubeFlag </summary>
/// <param name="flags">Defines the CubeFlag to be analyzed</param>
let FirstYFlag flags = FirstNotInvalidFlag flags (CubeFlag.XFlags ||| CubeFlag.ZFlags)

/// <summary> Returns the first ZFlag in the given CubeFlag </summary>
/// <param name="flags">Defines the CubeFlag to be analyzed</param>
let FirstZFlag flags = FirstNotInvalidFlag flags (CubeFlag.XFlags ||| CubeFlag.YFlags)

/// <summary> Converts a CubeFlag into values from -1 to 1 </summary>
///// <exception cref="System.Exception">Thrown when the CubeFlag is either invalid or has more than one flag</exception>
let ToInt flag = 
    match flag with
    | flag when IsXFlag flag -> 
        match flag with
        | CubeFlag.RightSlice -> 1
        | CubeFlag.MiddleSliceSides -> 0
        | _ -> -1
    | flag when IsYFlag flag ->
        match flag with
        | CubeFlag.TopLayer -> -1
        | CubeFlag.MiddleLayer -> 0
        | _ -> 1
    | flag when IsZFlag flag ->
        match flag with
        | CubeFlag.BackSlice -> 1
        | CubeFlag.MiddleSlice -> 0
        | _ -> -1
    | CubeFlag.None -> 0
    | _ -> raise <| new Exception("Flag can not be converted to an integer")

/// <summary> Returns a ClubFlag which contains all single flags in the first parameter which don't exist in the second parameter </summary>
/// <param name="flags">Defines all possible flags</param>
/// <param name="invalid">Defines the flags to be filtered out of the first parameter</param>
let ExceptFlag (flags: CubeFlag) (invalid: CubeFlag) : CubeFlag = 
    let mutable pos = CubeFlag.None;

    GetFlags flags
        |> Seq.where(fun f -> not <| invalid.HasFlag f)
        |> Seq.iter(fun f -> pos <- pos ||| f)

    pos

/// <summary> Returns true if the given CubeFlag contains a valid move </summary>
/// <param name="flags">Defines the CubeFlag to be analyzed</param>
let IsPossibleMove (flags: CubeFlag) : bool = 
    let flagsExcludingNone = ExceptFlag flags CubeFlag.None

    (GetFlags flagsExcludingNone |> Seq.forall(fun f -> IsXFlag f))
    || (GetFlags flagsExcludingNone |> Seq.forall(fun f -> IsYFlag f))
    || (GetFlags flagsExcludingNone |> Seq.forall(fun f -> IsZFlag f))

/// <summary>
/// Returns the next flag after a layer rotation
/// </summary>
/// <param name="rotationLayer">Rotation layer</param>
/// <param name="direction">Rotation direction</param>
/// <returns>Next cube flag</returns>
let NextCubeFlag (flag: CubeFlag) (rotationLayer: CubeFlag) (direction: bool) = 
    let mutable direction = direction
    let mutable flag = flag
    let mutable nextFlag = CubeFlag.None
    
    if (CountFlags flag) = 1 then
        if IsXFlag rotationLayer then
            if rotationLayer = CubeFlag.LeftSlice then
                direction <- not direction

            if (not direction) && (not <| IsXFlag flag) then
                flag <- GetOppositeFlag flag

            match flag with
            | CubeFlag.FrontSlice -> nextFlag <- CubeFlag.TopLayer
            | CubeFlag.MiddleSlice -> nextFlag <- CubeFlag.MiddleLayer
            | CubeFlag.BackSlice -> nextFlag <- CubeFlag.BottomLayer
            | CubeFlag.TopLayer-> nextFlag <- CubeFlag.BackSlice
            | CubeFlag.MiddleLayer -> nextFlag <- CubeFlag.MiddleSlice
            | CubeFlag.BottomLayer -> nextFlag <- CubeFlag.FrontSlice
            | _ -> nextFlag <- flag

        else if IsYFlag rotationLayer then
            if rotationLayer = CubeFlag.BottomLayer then
                direction <- not direction
            
            if (not direction) && (not <| IsYFlag flag) then
                flag <- GetOppositeFlag flag
                
            match flag with
            | CubeFlag.FrontSlice -> nextFlag <- CubeFlag.LeftSlice
            | CubeFlag.MiddleSlice -> nextFlag <- CubeFlag.MiddleSliceSides
            | CubeFlag.BackSlice -> nextFlag <- CubeFlag.RightSlice
            | CubeFlag.LeftSlice -> nextFlag <- CubeFlag.BackSlice
            | CubeFlag.MiddleSliceSides-> nextFlag <- CubeFlag.MiddleSlice
            | CubeFlag.RightSlice -> nextFlag <- CubeFlag.FrontSlice
            | _ -> nextFlag <- flag

        else if IsZFlag rotationLayer then
            if rotationLayer = CubeFlag.BackSlice then
                direction <- not direction
            
            if (not direction) && (not <| IsZFlag flag) then
                flag <- GetOppositeFlag flag
                
            match flag with
            | CubeFlag.TopLayer -> nextFlag <- CubeFlag.RightSlice
            | CubeFlag.MiddleLayer -> nextFlag <- CubeFlag.MiddleSliceSides
            | CubeFlag.BottomLayer -> nextFlag <- CubeFlag.LeftSlice
            | CubeFlag.LeftSlice -> nextFlag <- CubeFlag.TopLayer
            | CubeFlag.MiddleSliceSides-> nextFlag <- CubeFlag.MiddleLayer
            | CubeFlag.RightSlice -> nextFlag <- CubeFlag.BottomLayer
            | _ -> nextFlag <- flag

    ExceptFlag nextFlag CubeFlag.None


/// <summary> Returns the next flags of the old flags after a layer rotation </summary>
/// <param name="rotationLayer">Rotation layer</param>
/// <param name="direction">Rotation direction</param>
/// <returns>Next cube flags</returns>
let NextFlags (flags: CubeFlag) (rotationLayer: CubeFlag) (direction: bool) : CubeFlag = 
    let mutable newFlags = CubeFlag.None
    GetFlags flags |> Seq.iter(fun f -> newFlags <- newFlags ||| (NextCubeFlag f rotationLayer direction))
    newFlags

namespace RubiksCubeLib

open System

/// Defines the position (i.e. the layer(s)) of a cube
[<Serializable>]
[<Flags>]
type CubeFlag = 
    | None             = 0
    | TopLayer         = 1
    | MiddleLayer      = 2
    | BottomLayer      = 4
    | FrontSlice       = 8
    | MiddleSlice      = 16
    | BackSlice        = 32
    | LeftSlice        = 64
    | MiddleSliceSides = 128
    | RightSlice       = 256
module CubeFlag = 
    // note: can declare these as [<Literal>] if needed for match expressions
    let XFlags = CubeFlag.RightSlice ||| CubeFlag.LeftSlice ||| CubeFlag.MiddleSliceSides
    let YFlags = CubeFlag.TopLayer ||| CubeFlag.BottomLayer ||| CubeFlag.MiddleLayer
    let ZFlags = CubeFlag.FrontSlice ||| CubeFlag.BackSlice ||| CubeFlag.MiddleSlice

/// Every class implementing this interface are defined by a name and the information whether it allows multiple layers
type IMove =
    /// Describes the name of the move (i.e the notation)
    abstract member Name: string with get

    /// Describes whether the implementing class allowes multiple layers as move
    abstract member MultipleLayers: bool with get

    /// Gets the reverse move
    abstract member ReverseMove: IMove with get

    /// <summary> Transforms the move </summary>
    /// <param name="type">Rotation layer</param>
    abstract member Transform: CubeFlag -> IMove





namespace RubiksCubeLib

open System
open System.Collections.Generic

/// <summary> Represents the position of a cube and its orientation </summary> 
[<Serializable>] // TODO: may be failing due to lack of () constructor
type CubePosition (x, y, z) = 
    let mutable x = x
    let mutable y = y
    let mutable z = z

    /// <summary> The XFlag of the cube </summary>
    member __.X with get() = x and set(value) = x <- value

    ///// <summary> The YFlag of the cube </summary>
    member __.Y with get() = y and set(value) = y <- value
    
    ///// <summary> The ZFlag of the cube </summary>
    member __.Z with get() = z and set(value) = z <- value
    
    /// <summary> Constructor with one CubeFlag </summary>
    /// <param name="flags">Defines the CubeFlag where the X-, Y- and ZFlag are filtered out</param>
    new(flags: CubeFlag) = 
        CubePosition(
            CubeFlagService.FirstXFlag flags, 
            CubeFlagService.FirstYFlag flags, 
            CubeFlagService.FirstZFlag flags
        )

    /// <summary> Returns all CubeFlags in one </summary>
    member __.Flags = __.X ||| __.Y ||| __.Z

    /// <summary> Returns true if the given CubeFlag describes a corner cube </summary>
    /// <param name="position">Defines the CubeFlag to be analyzed</param>
    static member IsCorner (position: CubeFlag) : bool = 
        (position = (CubeFlag.TopLayer ||| CubeFlag.FrontSlice ||| CubeFlag.LeftSlice))
        || (position = (CubeFlag.TopLayer ||| CubeFlag.FrontSlice ||| CubeFlag.RightSlice))
        || (position = (CubeFlag.TopLayer ||| CubeFlag.BackSlice ||| CubeFlag.LeftSlice))
        || (position = (CubeFlag.TopLayer ||| CubeFlag.BackSlice ||| CubeFlag.RightSlice))
        || (position = (CubeFlag.BottomLayer ||| CubeFlag.FrontSlice ||| CubeFlag.LeftSlice))
        || (position = (CubeFlag.BottomLayer ||| CubeFlag.FrontSlice ||| CubeFlag.RightSlice))
        || (position = (CubeFlag.BottomLayer ||| CubeFlag.BackSlice ||| CubeFlag.LeftSlice))
        || (position = (CubeFlag.BottomLayer ||| CubeFlag.BackSlice ||| CubeFlag.RightSlice))

    /// <summary> Returns true if the given CubeFlag describes a edge cube </summary>
    /// <param name="position"> Defines the CubeFlag to be analyzed </param>
    static member IsEdge (position: CubeFlag) : bool = 
        (position = (CubeFlag.TopLayer ||| CubeFlag.FrontSlice ||| CubeFlag.MiddleSliceSides))
        || (position = (CubeFlag.TopLayer ||| CubeFlag.BackSlice ||| CubeFlag.MiddleSliceSides))
        || (position = (CubeFlag.TopLayer ||| CubeFlag.RightSlice ||| CubeFlag.MiddleSlice))
        || (position = (CubeFlag.TopLayer ||| CubeFlag.LeftSlice ||| CubeFlag.MiddleSlice))
        || (position = (CubeFlag.MiddleLayer ||| CubeFlag.FrontSlice ||| CubeFlag.RightSlice))
        || (position = (CubeFlag.MiddleLayer ||| CubeFlag.FrontSlice ||| CubeFlag.LeftSlice))
        || (position = (CubeFlag.MiddleLayer ||| CubeFlag.BackSlice ||| CubeFlag.RightSlice))
        || (position = (CubeFlag.MiddleLayer ||| CubeFlag.BackSlice ||| CubeFlag.LeftSlice))
        || (position = (CubeFlag.BottomLayer ||| CubeFlag.FrontSlice ||| CubeFlag.MiddleSliceSides))
        || (position = (CubeFlag.BottomLayer ||| CubeFlag.BackSlice ||| CubeFlag.MiddleSliceSides))
        || (position = (CubeFlag.BottomLayer ||| CubeFlag.RightSlice ||| CubeFlag.MiddleSlice))
        || (position = (CubeFlag.BottomLayer ||| CubeFlag.LeftSlice ||| CubeFlag.MiddleSlice))


    /// <summary> Returns true if the given CubeFlag describes a center cube  </summary>
    /// <param name="position"> Defines the CubeFlag to be analyzed </param>
    static member IsCenter (position: CubeFlag) : bool = 
        (position = (CubeFlag.TopLayer ||| CubeFlag.MiddleSlice ||| CubeFlag.MiddleSliceSides))
        || (position = (CubeFlag.BottomLayer ||| CubeFlag.MiddleSlice ||| CubeFlag.MiddleSliceSides))
        || (position = (CubeFlag.LeftSlice ||| CubeFlag.MiddleSlice ||| CubeFlag.MiddleLayer))
        || (position = (CubeFlag.RightSlice ||| CubeFlag.MiddleSlice ||| CubeFlag.MiddleLayer))
        || (position = (CubeFlag.FrontSlice ||| CubeFlag.MiddleSliceSides ||| CubeFlag.MiddleLayer))
        || (position = (CubeFlag.BackSlice ||| CubeFlag.MiddleSliceSides ||| CubeFlag.MiddleLayer))

    
    /// <summary> Returns true if this CubeFlag has the given CubeFlag </summary>
    /// <param name="flag">Defines the CubeFlag which will be checked</param>
    member __.HasFlag flag = __.Flags.HasFlag flag

    /// <summary> Returns the single flags in this CubeFlag </summary>
    member __.GetFlags () = CubeFlagService.GetFlags __.Flags

    /// <summary> Calculates the next position after a layer rotation </summary>
    /// <param name="layer">Rotation layer</param>
    /// <param name="direction">Rotation direction</param>
    member __.NextFlag (layer: CubeFlag) (direction: bool): unit = 
        let newFlags: CubeFlag = CubeFlagService.NextFlags __.Flags layer direction
        
        __.X <- CubeFlagService.FirstXFlag newFlags
        __.Y <- CubeFlagService.FirstYFlag newFlags
        __.Z <- CubeFlagService.FirstZFlag newFlags

    override __.Equals(b) = 
        let other : CubePosition = downcast b 

        __.X = other.X 
          && __.Y = other.Y
          && __.Z = other.Z

    override __.GetHashCode() = 
        (int32)__.X + (int32)__.Y + (int32)__.Z

    override __.ToString() = __.Flags.ToString()
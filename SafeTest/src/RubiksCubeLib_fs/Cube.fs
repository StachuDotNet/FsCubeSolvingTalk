namespace RubiksCubeLib.RubiksCube

open RubiksCubeLib
open System.Drawing
open System
open System.IO
open System.Runtime.Serialization.Formatters.Binary

/// <summary> Represents a single cube of the Rubik </summary>
[<Serializable>]
type Cube (faces: seq<Face>, position: CubeFlag) = 
    let mutable faces = faces
    let mutable colors = new ResizeArray<Color>() // refactor this a bit into just colors = faces |> list.map f.color or osmething
    
    do 
        faces |> Seq.toList |> List.iter(fun f -> colors.Add f.Color)

    new(position:CubeFlag) = Cube(UniCube.GenFaces(), position)

    /// <summary> The faces where the cube belongs to </summary>
    member __.Faces 
        with get() = faces 
        and set(value) = faces <- value

    /// <summary> The colors the cube has </summary>
    member val Colors = colors with get, set

    /// <summary> The position in the Rubik </summary>
    member val Position = CubePosition(position) with get, set
    
    /// <summary> Returns true if this cube is placed at the corner </summary>
    member __.IsCorner = CubePosition.IsCorner __.Position.Flags

    /// <summary> Returns true if this cube is placed at the edge </summary>
    member __.IsEdge = CubePosition.IsEdge __.Position.Flags

    /// <summary> Returns true if this cube is placed at the center </summary>
    member __.IsCenter = CubePosition.IsCenter __.Position.Flags

    /// <summary> Returns a clone of this cube (same properties but different instance) </summary>
    member __.DeepClone () : Cube = 
        use ms = new MemoryStream()
        let formatter = new BinaryFormatter()
        formatter.Serialize(ms, __)
        ms.Position <- (int64)0
        downcast (formatter.Deserialize ms)


    /// <summary> Changes the color of the given face of this cube </summary>
    /// <param name="face">Defines the face to be changed</param>
    /// <param name="color">Defines the color to be set</param>
    member __.SetFaceColor face color = 
        __.Faces |> Seq.filter(fun f -> f.Position = face) |> Seq.toList |> Seq.iter(fun f -> f.Color <- color)
        __.Colors.Clear()
        __.Faces |> Seq.toList |> Seq.iter(fun f -> __.Colors.Add f.Color)

    /// <summary> Returns the color of the given face </summary>
    /// <param name="face">Defines the face to be analyzed</param>
    member __.GetFaceColor face = 
        (__.Faces |> Seq.find(fun f -> f.Position = face)).Color

    /// <summary> Set the color of all faces back to black </summary>
    member __.ResetColors() = 
        __.Faces |> Seq.toList |> Seq.iter(fun f -> f.Color <- Color.Black)
        __.Colors.Clear()
        __.Faces |> Seq.toList |> Seq.iter(fun f -> __.Colors.Add f.Color)

    /// <summary> Change the position of the cube by rotating it on the given layer and the given direction </summary>
    /// <param name="layer">Defines the layer the cube is to rotate on</param>
    /// <param name="direction">Defines the direction of the rotation (true == clockwise)</param>
    member __.NextPos layer direction = 
        let oldCube = __.DeepClone()
        __.Position.NextFlag layer direction

        if __.IsCorner then
            //Set colors
            let layerFace = __.Faces |> Seq.find(fun f -> f.Position = CubeFlagService.ToFacePosition layer)
            let layerColor = layerFace.Color

            let newFlag = CubeFlagService.FirstNotInvalidFlag __.Position.Flags oldCube.Position.Flags
            let commonFlag = CubeFlagService.FirstNotInvalidFlag __.Position.Flags (newFlag ||| layer)
            let oldFlag = CubeFlagService.FirstNotInvalidFlag oldCube.Position.Flags (commonFlag ||| layer)

            let colorNewPos = (__.Faces |> Seq.find(fun f -> f.Position = CubeFlagService.ToFacePosition commonFlag)).Color
            let colorCommonPos = (__.Faces |> Seq.find(fun f -> f.Position = CubeFlagService.ToFacePosition oldFlag)).Color

            __.ResetColors()

            __.SetFaceColor layerFace.Position layerColor
            __.SetFaceColor <| CubeFlagService.ToFacePosition newFlag <| colorNewPos
            __.SetFaceColor <| CubeFlagService.ToFacePosition commonFlag <| colorCommonPos
        
        if __.IsCenter then

            let oldFlag = CubeFlagService.FirstNotInvalidFlag oldCube.Position.Flags  (CubeFlag.MiddleLayer ||| CubeFlag.MiddleSlice ||| CubeFlag.MiddleSliceSides)
            let centerColor = (__.Faces |> Seq.find(fun f -> f.Position = CubeFlagService.ToFacePosition oldFlag)).Color
            let newPos = CubeFlagService.FirstNotInvalidFlag __.Position.Flags (CubeFlag.MiddleSliceSides ||| CubeFlag.MiddleSlice ||| CubeFlag.MiddleLayer)

            __.ResetColors()
            __.SetFaceColor <| CubeFlagService.ToFacePosition newPos <| centerColor

        if __.IsEdge then
            let newFlag = CubeFlagService.FirstNotInvalidFlag __.Position.Flags (oldCube.Position.Flags ||| CubeFlag.MiddleSlice ||| CubeFlag.MiddleSliceSides ||| CubeFlag.MiddleLayer)
            let commonFlag = CubeFlagService.FirstNotInvalidFlag __.Position.Flags (newFlag ||| CubeFlag.MiddleSlice ||| CubeFlag.MiddleSliceSides ||| CubeFlag.MiddleLayer)
            let oldFlag = CubeFlagService.FirstNotInvalidFlag oldCube.Position.Flags (commonFlag ||| CubeFlag.MiddleSlice ||| CubeFlag.MiddleSliceSides ||| CubeFlag.MiddleLayer)

            let colorNewPos = (__.Faces |> Seq.find(fun f -> f.Position = CubeFlagService.ToFacePosition commonFlag)).Color;
            let colorCommonPos = (__.Faces |> Seq.find(fun f -> f.Position = CubeFlagService.ToFacePosition oldFlag)).Color;

            __.ResetColors()


            if layer = CubeFlag.MiddleLayer || layer = CubeFlag.MiddleSlice || layer = CubeFlag.MiddleSliceSides then
                __.SetFaceColor <| CubeFlagService.ToFacePosition newFlag <| colorNewPos
                __.SetFaceColor <| (CubeFlagService.ToFacePosition commonFlag) <| colorCommonPos
            else
                __.SetFaceColor <| CubeFlagService.ToFacePosition commonFlag <| colorNewPos
                __.SetFaceColor <| CubeFlagService.ToFacePosition newFlag <| colorCommonPos

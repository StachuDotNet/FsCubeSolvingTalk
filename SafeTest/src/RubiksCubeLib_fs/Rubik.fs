namespace RubiksCubeLib.RubiksCube

open System
open System.Collections.Generic
open System.Linq
open System.Drawing
open System.IO
open System.Runtime.Serialization.Formatters.Binary
open RubiksCubeLib

/// <summary> Defines a Rubik's Cube </summary>
[<Serializable>]
type Rubik(cfront, cback, ctop, cbottom, cright, cleft) as this = 
    let mutable colors = [| cfront; cback; ctop; cbottom; cright; cleft |]
    let mutable cubes = new List<Cube>()

    /// <summary> Returns the CubeFlag of the Cube at the given 3D position  </summary>
    let genSideFlags i j k = 
        let mutable p = CubeFlag()

        match i with
        | -1 -> p <- p ||| CubeFlag.LeftSlice
        | 0 -> p <- p ||| CubeFlag.MiddleSliceSides
        | 1 -> p <- p ||| CubeFlag.RightSlice
        | _ -> ()

        match j with
        | -1 -> p <- p ||| CubeFlag.TopLayer
        | 0 -> p <- p ||| CubeFlag.MiddleLayer
        | 1 -> p <- p ||| CubeFlag.BottomLayer
        | _ -> ()

        match k with
        | -1 -> p <- p ||| CubeFlag.FrontSlice
        | 0 -> p <- p ||| CubeFlag.MiddleSlice
        | 1 -> p <- p ||| CubeFlag.BackSlice
        | _ -> ()

        p
    
    do
        for i = -1 to 1 do 
            for j = -1 to 1 do
                for k = -1 to 1 do 
                    cubes.Add <| new Cube(genSideFlags i j k)
        
        this.SetFaceColor CubeFlag.FrontSlice FacePosition.Front cfront
        this.SetFaceColor CubeFlag.BackSlice FacePosition.Back cback
        this.SetFaceColor CubeFlag.TopLayer FacePosition.Top ctop
        this.SetFaceColor CubeFlag.BottomLayer FacePosition.Bottom cbottom
        this.SetFaceColor CubeFlag.RightSlice FacePosition.Right cright
        this.SetFaceColor CubeFlag.LeftSlice FacePosition.Left cleft
        

    new() = Rubik(Color.Orange, Color.Red, Color.Yellow, Color.White, Color.Blue, Color.Green)

    
    //private member _cubes = new List()
    /// The collection of cubes of this Rubik
    member __.Cubes with get() = cubes //and set(value) = cubes <- cubes

    /// The colors of this Rubik
    member __.Colors with get() = colors

    member this.SetFaceColor (affected: CubeFlag) (face: FacePosition) (color: Color) = 
        this.Cubes
            |> Seq.filter(fun c -> c.Position.HasFlag affected)
            |> Seq.toList
            |> List.iter(fun c -> c.SetFaceColor face color)

        this.Cubes
            |> Seq.toList
            |> Seq.iter(fun c -> 
                c.Colors.Clear()
                c.Faces.ToList().ForEach(fun f -> c.Colors.Add f.Color)
            )

    /// <summary> Returns the color of the face of the first cube with the given CubeFlag </summary>
    /// <param name="position">Defines the CubeFlag which filters this cubes</param>
    /// <param name="face">Defines the face to be analyzed</param>
    member this.GetFaceColor (position: CubeFlag) (face: FacePosition) : Color = 
        this.Cubes.First(fun c -> c.Position.Flags = position).GetFaceColor(face)

    /// Returns the color of the front face
    member this.FrontColor with get() = this.GetFaceColor (CubeFlag.FrontSlice ||| CubeFlag.MiddleSliceSides ||| CubeFlag.MiddleLayer) FacePosition.Front
    
    /// Returns the color of the back face
    member this.BackColor with get() = this.GetFaceColor (CubeFlag.BackSlice ||| CubeFlag.MiddleSliceSides ||| CubeFlag.MiddleLayer) FacePosition.Back
    
    /// Returns the color of the top face
    member this.TopColor with get() = this.GetFaceColor (CubeFlag.TopLayer ||| CubeFlag.MiddleSliceSides ||| CubeFlag.MiddleSlice) FacePosition.Top
    
    /// Returns the color of the bottom face
    member this.BottomColor with get() = this.GetFaceColor (CubeFlag.BottomLayer ||| CubeFlag.MiddleSliceSides ||| CubeFlag.MiddleSlice) FacePosition.Bottom
    
    /// Returns the color of the right face
    member this.RightColor with get() = this.GetFaceColor (CubeFlag.RightSlice ||| CubeFlag.MiddleLayer ||| CubeFlag.MiddleSlice) FacePosition.Right
    
    /// Returns the color of the left face
    member this.LeftColor with get() = this.GetFaceColor (CubeFlag.LeftSlice ||| CubeFlag.MiddleSlice ||| CubeFlag.MiddleLayer) FacePosition.Left

    /// Returns a clone of this cube (i.e. same properties but new instance)
    member this.DeepClone() : Rubik = 
        use ms = new MemoryStream()
        let formatter = new BinaryFormatter()
        formatter.Serialize(ms, this)
        ms.Position <- (int64)0

        downcast (formatter.Deserialize(ms))

    /// Executes the given move (rotation)
    member __.RotateLayer (move: IMove) : unit = 
        if move.MultipleLayers then
            let moves : LayerMoveCollection = downcast move

            for m in (moves :> IList<LayerMove>) do
                __.RotateLayer m
        else
            __.RotateLayerMove <| downcast move

    /// <summary> Executes the given LayerMove </summary>
    member this.RotateLayerMove (move: LayerMove): unit = 
        let repetitions = if move.Twice then 2 else 1

        for _i = 0 to repetitions - 1 do
            let affected = this.Cubes.Where(fun c -> c.Position.HasFlag move.Layer)
            affected.ToList().ForEach(fun c -> c.NextPos move.Layer move.Direction)

    /// <summary> Rotates a layer of the Rubik </summary>
    /// <param name="layer">Defines the layer to be rotated on</param>
    /// <param name="direction">Defines the direction of the rotation (true == clockwise)</param>
    member this.RotateLayerFromCubeFlag (layer: CubeFlag) (direction: bool) : unit = 
        let affected = this.Cubes.Where(fun c -> c.Position.HasFlag layer)
        affected.ToList().ForEach(fun c -> c.NextPos layer direction)

    /// <summary> Execute random LayerMoves on the cube to scramble it </summary>
    /// <param name="moves">Defines the amount of moves</param>
    member this.Scramble (moves: int) : Algorithm = 
        let rnd = new Random()

        let moveStrings = new List<string>();

        let availableMoves = [
            CubeFlag.TopLayer
            CubeFlag.BottomLayer
            CubeFlag.BackSlice
            CubeFlag.FrontSlice
            CubeFlag.LeftSlice
            CubeFlag.RightSlice
        ]

        for i = 1 to moves do
            let move =
                new LayerMove(
                    availableMoves.[rnd.Next(0,5)],
                    Convert.ToBoolean(rnd.Next(0, 2)),
                    false
                )

            moveStrings.Add(move.ToString())

            this.RotateLayer move

        new Algorithm(String.Join(" ", moveStrings))

    member this.ScrambleAlg (alg: Algorithm) : unit = 
        for move in alg.Moves do
            this.RotateLayer move
        
        ()         
        

    /// Returns the amount of edges of the top layer which have the right orientation
    member this.CountEdgesWithCorrectOrientation () : int = 
        let topColor = this.GetFaceColor (CubeFlag.TopLayer ||| CubeFlag.MiddleSliceSides ||| CubeFlag.MiddleSlice) FacePosition.Top
        
        this.Cubes 
            |> Seq.filter(fun c -> c.IsEdge && c.Faces.First(fun f -> f.Position = FacePosition.Top).Color = topColor) 
            |> Seq.length

    /// Returns a solved Rubik 
    member this.GenStandardCube () : Rubik = 
        let standardCube = new Rubik()

        standardCube.SetFaceColor CubeFlag.TopLayer FacePosition.Top this.TopColor
        standardCube.SetFaceColor CubeFlag.BottomLayer FacePosition.Bottom this.BottomColor
        standardCube.SetFaceColor CubeFlag.RightSlice FacePosition.Right this.RightColor
        standardCube.SetFaceColor CubeFlag.LeftSlice FacePosition.Left this.LeftColor
        standardCube.SetFaceColor CubeFlag.FrontSlice FacePosition.Front this.FrontColor
        standardCube.SetFaceColor CubeFlag.BackSlice FacePosition.Back this.BackColor

        standardCube

    /// <summary> Returns the position of given cube where it has to be when the Rubik is solved </summary>
    /// <param name="cube">Defines the cube to be analyzed</param>
    member this.GetTargetFlags (cube: Cube) : CubeFlag = 
        this.GenStandardCube().Cubes.First(fun cu -> CollectionMethods.ScrambledEquals cu.Colors cube.Colors).Position.Flags

    member __.IsCorrect (cube: Cube) : bool = 
        true
        //cube.Position.Flags = this.GetTargetFlags cube && Solvability.GetOrientation(this, cube) = 0
        
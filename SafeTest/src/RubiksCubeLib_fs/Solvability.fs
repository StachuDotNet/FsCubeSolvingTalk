namespace RubiksCubeLib.Solver

open System.Linq
open RubiksCubeLib.RubiksCube
open RubiksCubeLib
open System.Collections.Generic

type PatternHelpers () = 
    /// <summary> Converts a rubik to a pattern </summary>
    /// <param name="r">Rubik </param>
    /// <returns>The pattern of the given rubik</returns>
    static member FromRubik (r: Rubik): Pattern = 
        let p = new Pattern()
        let newItems = new List<PatternItem>()

        for pos in CommonPositions.AllPositions do
            let cube = r.Cubes.First(fun c -> r.GetTargetFlags(c) = pos.Flags);
            newItems.Add(new PatternItem(cube.Position, (Solvability.GetOrientation r cube), pos.Flags));

        p.Items <- newItems
        p
and Solvability () =
    /// <summary> Refreshes the position of a cube </summary>
    /// <param name="r">Parent rubik of the cube</param>
    static let refreshCube (r: Rubik) (c: Cube) = 
        r.Cubes.First(fun cu -> CollectionMethods.ScrambledEquals cu.Colors c.Colors)

    static member GetOrientation (rubik: Rubik) (c: Cube) : Orientation = 
        let mutable orientation = Orientation.Correct

        if c.IsEdge then
            let targetFlags = rubik.GetTargetFlags c
            let clone = rubik.DeepClone()

            if not <| targetFlags.HasFlag CubeFlag.MiddleLayer then
                while (refreshCube clone c).Position.HasFlag CubeFlag.MiddleLayer do
                    clone.RotateLayerFromCubeFlag c.Position.X true

                let clonedCube = refreshCube clone c
                let yFace = clonedCube.Faces.First(fun f -> f.Color = rubik.TopColor || f.Color = rubik.BottomColor)

                if not <| FacePosition.YPos.HasFlag yFace.Position then
                    orientation <- Orientation.Clockwise

            else
                let zFace = c.Faces.First(fun f -> f.Color = rubik.FrontColor || f.Color = rubik.BackColor)

                if c.Position.HasFlag CubeFlag.MiddleLayer then
                    if not <| FacePosition.ZPos.HasFlag zFace.Position then
                        orientation <- Orientation.Clockwise
                else
                    if not <| FacePosition.YPos.HasFlag zFace.Position then
                        orientation <- Orientation.Clockwise
        else if c.IsCorner then
            let face = c.Faces.First(fun f -> f.Color = rubik.TopColor || f.Color = rubik.BottomColor)

            if not <| FacePosition.YPos.HasFlag face.Position then
                let part1 = FacePosition.XPos.HasFlag face.Position
                let part2 = not <| c.Position.HasFlag CubeFlag.BottomLayer
                let part3 = c.Position.HasFlag CubeFlag.FrontSlice
                let part4 = c.Position.HasFlag CubeFlag.RightSlice

                if part1 <> part2 <> part3 <> part4 then
                    orientation <- Orientation.CounterClockwise
                else
                    orientation <- Orientation.Clockwise

        orientation

    /// <summary> Permutation parity test </summary>
    /// <param name="rubik">Rubik to be tested</param>
    /// <returns>True, if the given Rubik passes the permutation parity test</returns>
    static member PermutationParityTest (rubik: Rubik): bool =
        let p = PatternHelpers.FromRubik rubik
        (p.Inversions % 2) = 0

    /// <summary> Corner parity test </summary>
    /// <param name="rubik">Rubik to be tested</param>
    /// <returns>True, if the given Rubik passes the corner parity test</returns>
    static member CornerParityTest (rubik:Rubik) : bool = 
        rubik.Cubes
            .Where(fun c -> c.IsCorner)
            .Sum(fun c -> (int)(Solvability.GetOrientation rubik c)) % 3 = 0

    /// <summary> Edge parity test </summary>
    /// <param name="rubik">Rubik to be tested</param>
    /// <returns>True, if the given Rubik passes the edge parity test</returns>
    static member EdgeParityTest (rubik: Rubik): bool = 
        rubik.Cubes
            .Where(fun c -> c.IsEdge)
            .Sum(fun c -> (int)(Solvability.GetOrientation rubik c)) % 2 = 0

    static member CorrectColors (r: Rubik): bool = 
        let wat = r.GenStandardCube().Cubes.Where(fun sc ->  r.Cubes.Where(fun c -> CollectionMethods.ScrambledEquals c.Colors sc.Colors).Count() = 1)
        
        wat.Count() = r.Cubes.Count

    static member FullParityTest (r: Rubik): bool =        
        Solvability.PermutationParityTest r 
            && Solvability.CornerParityTest r 
            && Solvability.EdgeParityTest r

    static member FullTest (r: Rubik): bool = 
        Solvability.CorrectColors r && Solvability.FullParityTest r

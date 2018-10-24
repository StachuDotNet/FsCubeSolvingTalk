namespace RubiksCubeLib.Solver

open System
open RubiksCubeLib

/// <summary> Represents an item in a pattern </summary>
[<Serializable>]
type PatternItem(currPos: CubePosition, currOrientation: Orientation, targetPos: CubeFlag) = 
    let mutable currPos = currPos
    let mutable currOrientation = currOrientation
    let mutable targetPos = targetPos

    /// <summary> Gets or sets the current cube position </summary>
    member __.CurrentPosition with get() = currPos and set(value) = currPos <- value

    /// <summary> Gets or sets the current cube orientation </summary>
    member __.CurrentOrientation with get() = currOrientation and set(value) = currOrientation <- value

    /// <summary> Gets or sets the target position </summary>
    member __.TargetPosition with get() = targetPos and set(value) = targetPos <- value

    override __.ToString() = 
        sprintf "%A-%A %A" __.CurrentPosition __.TargetPosition __.CurrentOrientation

    /// <summary> True, if the item has the same current position and the other equality conditions </summary>
    /// <param name="item">Item to compare</param>
    member this.Equals (item: PatternItem) : bool = 
        let sameOrientation =
            item.CurrentOrientation = Orientation.None
            || this.CurrentOrientation = Orientation.None
            || item.CurrentOrientation = this.CurrentOrientation

        let sameTargetPos =
            item.TargetPosition = CubeFlag.None 
            || this.TargetPosition = CubeFlag.None 
            || item.TargetPosition = this.TargetPosition;

        let result =
            item.CurrentPosition.Flags = this.CurrentPosition.Flags
            && sameOrientation && sameTargetPos

        result

    /// <summary> Transforms the pattern item </summary>
    member this.Transform (rotationLayer: CubeFlag) : unit = 
        if this.CurrentPosition.HasFlag rotationLayer then
            let oldFlags = this.CurrentPosition.Flags
            this.CurrentPosition.NextFlag rotationLayer true
            
            let mutable newOrientation = this.CurrentOrientation

            if CubePosition.IsCorner(this.CurrentPosition.Flags) && (not <| CubeFlagService.IsYFlag rotationLayer) then
                if (new CubePosition(oldFlags)).Y = (new CubePosition(this.CurrentPosition.Flags)).Y then
                    newOrientation <- enum<Orientation>((int)newOrientation + 2 |> (%) 3)
                else
                    newOrientation <- enum<Orientation>((int)newOrientation + 1 |> (%) 3)

            else if CubePosition.IsEdge(this.CurrentPosition.Flags) && CubeFlagService.IsZFlag(rotationLayer) then
                newOrientation <- enum<Orientation>((int)newOrientation ^^^ 1)

            this.CurrentOrientation <- newOrientation

        if this.TargetPosition.HasFlag(rotationLayer) then
            this.TargetPosition <- CubeFlagService.NextFlags this.TargetPosition rotationLayer true

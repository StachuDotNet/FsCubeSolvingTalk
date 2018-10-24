namespace RubiksCubeLib.Solver

open RubiksCubeLib.RubiksCube
open System
open System.Collections.Generic
open System.IO
open System.Linq
open System.Runtime.Serialization.Formatters.Binary
open RubiksCubeLib
open System.Runtime.InteropServices

/// Represents a cube pattern
[<Serializable>]
type Pattern() = 

    /// <summary> Counts the required inversions </summary>
    /// <param name="standard">Standard order of positions</param>
    /// <param name="input">Current Order of positions</param>
    /// <returns>Number of required inversions</returns>
    let CountInversions (standard: List<CubeFlag>) (input: List<CubeFlag>) = 
        let mutable inversions = 0

        for i = 0 to input.Count-1 do
            let index = standard.IndexOf input.[i]

            for j = 0 to input.Count-1 do
                let index2 = standard.IndexOf(input.[j])

                if index2 > index && j < i then
                    inversions <- inversions + 1
        
        inversions

    /// Gets or sets the specific pattern elements
    member val Items = new List<PatternItem>() with get, set

    /// Gets or sets the probalitiy of the pattern
    member val Probability = 0.0 with get, set

    /// Gets the number of required corner inversions
    member __.CornerInversions 
        with get() = 
            let newOrder = new List<CubeFlag>()

            for p in CommonPositions.CornerPositions do
                let affected = __.Items |> Seq.where(fun i -> i.TargetPosition = p.Flags) |> Seq.tryHead
                let pos = 
                    match affected with
                    | None -> p
                    | Some v -> v.CurrentPosition

                newOrder.Add(pos.Flags)

            let asdf = (CommonPositions.CornerPositions |> Seq.map(fun pos -> pos.Flags) |> Seq.toList).ToList()
            CountInversions asdf newOrder

    /// <summary> Gets the number of required edge inversions </summary>
    member __.EdgeInversions
        with get() = 
            let newOrder = new List<CubeFlag>()

            for p in CommonPositions.EdgePositions do
                let affected = __.Items |> Seq.where(fun i -> i.TargetPosition = p.Flags) |> Seq.tryHead
                
                let pos = 
                    match affected with
                    | None -> p
                    | Some v -> v.CurrentPosition

                newOrder.Add(pos.Flags)

            let asdf = (CommonPositions.EdgePositions |> Seq.map(fun pos -> pos.Flags) |> Seq.toList).ToList()
            CountInversions asdf newOrder

    
    /// Gets the number of required inversions
    member this.Inversions with get() = this.CornerInversions + this.EdgeInversions

    /// <summary> Gets the number of required 120° corner rotations </summary>
    member this.CornerRotations
        with get() = 
            this.Items |> Seq.where(fun i -> CommonPositions.CornerPositions.Contains i.CurrentPosition) |> Seq.sumBy(fun c -> (int)c.CurrentOrientation)

    /// <summary> Gets the number of flipped edges </summary>
    member this.EdgeFlips
        with get() = 
            this.Items |> Seq.where(fun i -> CommonPositions.EdgePositions.Contains i.CurrentPosition) |> Seq.sumBy(fun c -> (int)c.CurrentOrientation)

            

    /// <summary>
    /// Put to normal form
    /// </summary>
    /// <param name="standard">Normal form</param>
    /// <param name="newOrder">New order</param>
    static member Order(standard: IEnumerable<CubePosition>, newOrder: IEnumerable<PatternItem>) : IEnumerable<PatternItem> = 
        let result = new List<PatternItem>()

        for p in standard do
            let affected = newOrder |> Seq.where(fun i -> i.TargetPosition = p.Flags) |> Seq.tryHead

            match affected with
            | None -> ()
            | Some a -> result.Add a

        for i in newOrder do
            if i.TargetPosition = CubeFlag.None then
                result.Add(i)

        result :> IEnumerable<PatternItem>

    /// <summary> Initializes a new instance of the Pattern class </summary>
    /// <param name="pattern">
    /// All pattern elements in the following format:
    /// Cube positions as string: "RFT" => Right | Front | Top
    /// Orientations as string "0" => 0 (max value = 2)
    /// 1: "currentPos, targetPos, currentOrientation"
    /// 2: "currentPos, currentOrientation" => any target position
    /// 3: "currentPos, targetPos" => any orientation
    /// </param>
    static member FromPatternStringArray (pattern: string array, [<Optional; DefaultParameterValue(0.0)>]probability: float) = 
        let result = new Pattern()
        result.Probability <- probability
        let newItems = new List<PatternItem>()

        for item in pattern do
            newItems.Add <| PatternItemParser.Parse(item)

        result.Items <- (Pattern.Order(CommonPositions.AllPositions, newItems)).ToList()
        result

    static member FromPatternItems (items: IEnumerable<PatternItem>, [<Optional; DefaultParameterValue(0.0)>]probability: double) : Pattern = 
        let pattern = new Pattern()
        pattern.Probability <- probability;
        pattern.Items <- Pattern.Order(CommonPositions.AllPositions, items).ToList();
        pattern

    /// <summary> True, if this pattern includes all the pattern elements of another pattern </summary>
    /// <param name="pattern">Pattern to compare</param>
    member __.IncludesAllPatternElements (pattern: Pattern) : bool = 
        pattern.Items
        |> Seq.forall(fun p -> 
            __.Items.Any(fun i -> i.Equals p)
        )

    member this.DeepClone () : Pattern = 
        use ms = new MemoryStream()
        let formatter = new BinaryFormatter()
        formatter.Serialize(ms, this)
        ms.Position <- (int64)0 // may not work ?
        downcast (formatter.Deserialize(ms))

    /// Transforms the pattern
    member this.Transform (rotationLayer: CubeFlag) : Pattern = 
        let newPattern = this.DeepClone()

        for item in newPattern.Items do
            item.Transform rotationLayer

        newPattern

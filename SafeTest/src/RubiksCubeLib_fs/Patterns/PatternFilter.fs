namespace RubiksCubeLib.Solver

open System
open System.Runtime.InteropServices

/// Represents a filter to prevent unnecessary pattern equality checks
type PatternFilter(filter: System.Func<Pattern, Pattern, bool>, [<Optional; DefaultParameterValue(false)>]onlyAtBeginning: bool) =
    /// Equality check between two patterns
    member val Filter = filter with get, set

    /// Gets or sets whether the filter is used only in the beginning
    member val OnlyAtBeginning = onlyAtBeginning with get, set

type CommonPatternFilters () = 
    /// True, if both patterns have the equivalent count of edge and corner inversions
    static member SameInversionCount
        with get() = 
            let f (p1: Pattern) (p2: Pattern): bool = p1.EdgeInversions = p2.EdgeInversions && p1.CornerInversions = p2.CornerInversions
            let pattern = Func<Pattern, Pattern, bool>(f)
            PatternFilter(pattern)

    
    ///// True if both patterns have equivalent count of edge flips and corner rotations
    static member SameFlipCount
        with get() = 
            let f (p1: Pattern) (p2: Pattern): bool = p1.EdgeFlips = p2.EdgeFlips && p1.CornerRotations = p2.CornerRotations
            let pattern = Func<Pattern, Pattern, bool>(f)
            PatternFilter(pattern, true)

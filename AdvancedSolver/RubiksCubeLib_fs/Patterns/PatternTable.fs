namespace RubiksCubeLib.Solver

open System.Collections.Generic
open System.Linq
open RubiksCubeLib

/// Represents a pattern table to search for equivalent patterns easily
[<AbstractClass>]
type PatternTable () = // abstract
    /// Collection of comparison patterns with related algorithms
    abstract member Patterns: Dictionary<Pattern, Algorithm> with get

    /// <summary> inds all possible algorithms for this pattern </summary>
    /// <param name="p">Current rubik pattern</param>
    /// <param name="rotationLayer">Transformation rotation</param>
    /// <returns>Returns all possible solutions for this pattern</returns>
    member this.FindMatches (p: Pattern) (rotationLayer: CubeFlag) (filter: PatternFilter) : Dictionary<Pattern, Algorithm> = 
        let mutable transformedPatterns = 
            this.Patterns.ToDictionary((fun kvp -> kvp.Key.DeepClone()), (fun a -> a.Value))

        let mutable filteredPatterns = 
            transformedPatterns
                .Where(fun kvp -> filter.Filter.Invoke(p, kvp.Key))// filter
                .ToDictionary((fun pa -> pa.Key), (fun a -> a.Value))
                .OrderByDescending(fun k -> k.Key.Probability)// order by probability
                .ToDictionary((fun pa -> pa.Key.DeepClone()), (fun a -> a.Value))

        let matches = new Dictionary<Pattern, Algorithm>()

        let mutable hack = false

        // 4 possible standard transformations
        for i = 0 to 4-1 do
            if not hack then 

                // Get matches
                for kvp in filteredPatterns.Where(fun pa -> p.IncludesAllPatternElements(pa.Key)) do
                    matches.Add(kvp.Key, kvp.Value) // Add to matches

                if rotationLayer = CubeFlag.None then
                    hack <- true
                
                else 

                    if (filter.OnlyAtBeginning) then
                        transformedPatterns <- filteredPatterns.Except(matches).ToDictionary((fun pa -> pa.Key.Transform(rotationLayer)), (fun a -> a.Value.Transform(rotationLayer)))
                        filteredPatterns <- transformedPatterns
                    else
                        transformedPatterns <- transformedPatterns.ToDictionary((fun pa -> pa.Key.Transform(rotationLayer)), (fun a -> a.Value.Transform(rotationLayer)))
                        filteredPatterns <- transformedPatterns.Where(fun kvp -> filter.Filter.Invoke(p, kvp.Key)).ToDictionary((fun pa -> pa.Key), (fun a -> a.Value))

        matches

    /// <summary> Searches for the best algorithm for the given pattern </summary>
    /// <param name="p">Current rubik pattern</param>
    /// <param name="rotationLayer">Transformation layer</param>
    /// <returns>Returns the best match</returns>
    member this.FindBestMatch (p: Pattern) (rotationLayer: CubeFlag) (filter: PatternFilter) : Algorithm = 
        let matches = this.FindMatches p rotationLayer filter
        let bestAlgo = matches.OrderByDescending(fun item -> item.Key.Items.Count).FirstOrDefault().Value
        bestAlgo

namespace RubiksCubeLib

open System
open System.Collections.Generic

/// Represents a collection of layermoves
type Algorithm (algorithm: string) = 
    let mutable moves = List<IMove>()

    do
        match algorithm with
        | null -> ()
        | alg -> 
            alg.Split(' ') 
                |> Array.iter(fun s -> 
                    let mutable move = Unchecked.defaultof<LayerMove>
                    let mutable collection = Unchecked.defaultof<LayerMoveCollection>

                    if LayerMove.TryParse(s, &move) then 
                        moves.Add move
                    else if LayerMoveCollection.TryParse(s, &collection) then
                        moves.Add collection
                    else
                        failwith "Invalid notation"
                )

    /// <summary> Constructor with a notation string, but splitted into two parameters for string.Format() </summary>
    /// <param name="format">Defines the format pattern of string.Format()</param>
    /// <param name="args">Defines the arguments of string.Format()</param>
    new(format: string, [<ParamArray>] args: Object[]) = 
        Algorithm(System.String.Format(format, args))

    /// Gets or sets the the collection of layer moves
    member __.Moves with get() = moves and set(value) = moves <- value

    /// <summary> Transforms the algorithm </summary>
    /// <param name="rotationLayer">Transformation layer</param>
    /// <returns>Transformed algorithm</returns>
    member this.Transform rotationLayer = 
        let newAlg = Algorithm(Unchecked.defaultof<string>)

        for move in moves do
            newAlg.Moves.Add <| move.Transform rotationLayer

        newAlg

    /// Converts the collection into a notation
    override this.ToString() = System.String.Join (" ", this.Moves)

    /// Returns a simplified version of a given algorithm
    static member RemoveUnnecessaryMoves(alg: Algorithm): Algorithm = 
        let mutable finished = false

        while not finished do
            finished <- true

            let mutable i = 0

            while i < alg.Moves.Count - 1 do 

                let currentMove: IMove = alg.Moves.[i]
                
                if i < alg.Moves.Count - 1 then
                    if (currentMove.ReverseMove.Equals(alg.Moves.[i + 1])) then
                        finished <- false
                        alg.Moves.RemoveAt(i + 1)
                        alg.Moves.RemoveAt(i)

                        if i <> 0 then
                            i <- i - 1

                if i < alg.Moves.Count - 2 then
                    if (currentMove.Equals(alg.Moves.[i + 1]) && currentMove.Equals(alg.Moves.[i + 2])) then
                        finished <- false
                        let reverse: IMove = alg.Moves.[i + 2].ReverseMove
                        alg.Moves.RemoveAt(i + 1)
                        alg.Moves.RemoveAt(i)
                        alg.Moves.[i] <- reverse

                        if (i <> 0) then
                            i <- i - 1

                i <- i + 1

        alg

namespace RubiksCubeLib

open System
open System.Linq
open System.Runtime.InteropServices
open System.Collections.Generic

/// <summary> Constructor </summary>
/// <param name="layer">Defines the layer to be moved</param>
/// <param name="direction">Defines the direction (true == clockwise and false == counter-clockwise)</param>
/// <param name="twice">Defines whether this layer will be turned twice or not</param>
/// <exception cref="System.Exception">Thrown when layer contains more than one flag</exception>
type LayerMove (layer: CubeFlag, direction: bool, twice: bool) = 
    let mutable layer = layer
    let mutable direction = direction
    let mutable twice = twice

    do 
        if CubeFlagService.CountFlags layer <> 1 then 
            failwith "Impossible movement"

    /// Describes the layer of this LayerMove
    member __.Layer with get() = layer and set(value) = layer <- value
    
    /// Describes the direction of this LayerMove
    member __.Direction with get() = direction and set(value) = direction <- value
    
    /// Describes whether this LayerMove will be executed twice
    member __.Twice with get() = twice and set(value) = twice <- value
    
    interface IMove with
        /// Returns true if MultipleLayers are allowed
        member __.MultipleLayers with get() = false

        /// Returns the name (the notation) of this LayerMove
        member this.Name
            with get() = 
                let directionString = 
                    match this.Direction with 
                    | true -> "Clockwise"
                    | false -> "Counter-Clockwise"

                this.Layer.ToString() + " " + (if this.Twice then "x2" else if this.Direction then "Clockwise" else "Counter-Clockwise")
    
        /// Gets the reverse move
        member this.ReverseMove with get() = upcast LayerMove(this.Layer, not this.Direction, this.Twice)

         /// <summary> Transforms the layer move </summary>
        /// <param name="rotationLayer">Transformation layer</param>
        /// <returns>Transformed layer move</retur
        member this.Transform rotationLayer =
            let switchDir = 
                CubeFlagService.IsYFlag rotationLayer && this.Layer.HasFlag CubeFlag.MiddleSlice
                || CubeFlagService.IsXFlag rotationLayer && this.Layer.HasFlag CubeFlag.MiddleLayer
                || CubeFlagService.IsZFlag rotationLayer && this.Layer.HasFlag CubeFlag.MiddleSliceSides

            let nextCubeFlag = CubeFlagService.NextCubeFlag this.Layer rotationLayer true

            upcast LayerMove(nextCubeFlag, this.Direction <> switchDir, this.Twice)

    /// Combines two LayerMoves into a LayerMoveCollection
    static member AndItem move1 move2 = 
    //static member op_Amp (first: LayerMove, second: LayerMove) = 
        let moves = new LayerMoveCollection()
        (moves :> IList<LayerMove>).Add move1
        (moves :> IList<LayerMove>).Add move2
        moves

    /// <summary> Parses a notation string into a LayerMove </summary>
    /// <param name="notation">Defines to string to be parsed</pa
    static member Parse (notation : string) : LayerMove = 
        let layer = notation.[0].ToString()
        let rotationLayer = CubeFlagService.Parse layer

        let ccwChars = [| '\''; 'i'|]
        let direction = ccwChars |> Array.filter(fun c -> notation.Contains c) |> Array.isEmpty

        let twice = notation.Contains "2"
        LayerMove(rotationLayer, direction, twice)

    /// <summary> Parses a notation string into a layer move </summary>
    /// <param name="notation">String to be parsed</param>
    /// <param name="move">The resulting layer move</param>
    /// <returns>True, if the string was successfully parsed into a layermove</returns>
    static member TryParse (notation : string, [<Out>] move : byref<LayerMove>) : bool = 
        move <- Unchecked.defaultof<LayerMove>

        let layer = notation.[0].ToString()
        let rotationLayer = CubeFlagService.Parse layer
         
        let ccwChars = [| '\''; 'i'|]
        let direction = ccwChars |> Array.filter(fun c -> notation.Contains c) |> Array.isEmpty
         
        let twice = notation.Contains "2"
         
        match CubeFlagService.CountFlags rotationLayer with 
        | 1 -> 
            move <- LayerMove(rotationLayer, direction, twice)
            true
        | _ -> false

    /// Converts this LayerMove into a notation string
    override this.ToString() = 
        let mutable move = String.Empty

        match this.Layer with
            | CubeFlag.TopLayer -> Some "U"
            | CubeFlag.MiddleLayer -> Some "E"
            | CubeFlag.BottomLayer -> Some "D"
            | CubeFlag.FrontSlice -> Some "F"
            | CubeFlag.MiddleSlice -> Some "S"
            | CubeFlag.BackSlice -> Some "B"
            | CubeFlag.LeftSlice -> Some "L"
            | CubeFlag.MiddleSliceSides -> Some "M"
            | CubeFlag.RightSlice -> Some "R"
            | _ -> None
        |> function 
            | Some c -> move <- c
            | None -> ()
        
        if not this.Direction then
            move <- move + "'"

        if this.Twice then
            move <- move + "2"

        move
   
    /// <summary> True, if the item accomplishes the equality conditions </summary>
    /// <param name="obj">Layer move to be compared</param>
    override this.Equals o =
        // can be super pertty.
        if o :? LayerMove then
            let move: LayerMove = downcast o
            this.Direction = move.Direction && this.Layer = move.Layer && this.Twice = move.Twice
        else
            false

    override this.GetHashCode() = hash this

and LayerMoveCollection () = 
    /// The list of the single moves 
    //let moves = new System.Collections.Generic.List<LayerMove>()
    member private this.moves = System.Collections.Generic.List<LayerMove>()

    interface IList<LayerMove> with
        /// <summary> Returns the index of a item in the collection </summary>
        /// <param name="item">Defines the item</param>
        member this.IndexOf item = this.moves.IndexOf item
        
        /// Returns the enumerator of the collection
        member this.GetEnumerator () : IEnumerator<LayerMove> = upcast this.moves.GetEnumerator()

        /// Returns the enumerator of the collection
        member this.GetEnumerator () : System.Collections.IEnumerator = upcast this.moves.GetEnumerator()

        /// <summary> Removes the given item out of the collection </summary>
        /// <param name="item">Defines the item to be removed</param>
        member this.Remove item = this.moves.Remove item

        /// <summary> Copies the entire collection into the given array starting at the given index </summary>
        /// <param name="array">The</param>
        /// <param name="arrayIndex"></param>
        member this.CopyTo(array, arrayIndex) = this.moves.CopyTo(array, arrayIndex)

        /// <summary> Returns true if this collection contains the given item </summary>
        /// <param name="item">Defines the item to be searched for</param>
        member this.Contains item = this.moves.Contains item

        /// Clears the collection
        member this.Clear () = this.moves.Clear()

         /// <summary> Adds an item at the end of the collection </summary>
        /// <param name="item">Defines the item which is meant to be added</param>
        /// <exception cref="System.Exception">Thrown if this movement would be impossible</exception>
        member this.Add item = 
            let mutable flag = item.Layer;

            this.moves.ToArray() |> Array.iter(fun m -> flag <- flag ||| m.Layer)

            if CubeFlagService.IsPossibleMove flag then
                this.moves.Add item
            else
                failwith "Impossible movement"
        
        /// Returns true if this Layer is readonly
        member this.IsReadOnly with get() = false

        /// Returns the count of the moves
        member this.Count with get() = this.moves.Count

        /// <summary> Removes the item at the given index </summary>
        /// <param name="index">Defines the index where the item is meant to be removed</param>
        member this.RemoveAt index = this.moves.RemoveAt index

        /// <summary> Inserts an item in the collection at a index </summary>
        /// <param name="index">Defines the index where the item is meant to be inserted</param>
        /// <param name="item">Defines te</param>
        member this.Insert(index, item) = this.moves.Insert(index, item)

        /// <summary> Returns the LayerMove at the given index </summary>
        /// <param name="index">Defines the index of the item</param>
        member this.Item
            with get(index) = this.moves.[index]
            and set index value = this.moves.[index] <- value

    /// <summary> Adds multiple items at the end of the collection </summary>
    /// <param name="items">Defines the items which are meant to be added</param>
    member this.AddRange (items: IEnumerable<LayerMove>) = 
        items |> Seq.iter(fun item -> (this :> IList<LayerMove>).Add item)

    member this.AddRange (items: LayerMoveCollection) = 
        items |> Seq.iter(fun item -> (this :> IList<LayerMove>).Add item)
    
    
    /// <summary> Adds a single LayerMove to the given collection </summary>
    /// <param name="collection">Defines the collection to be expanded</param>
    /// <param name="newMove">Defines the additional LayerMove</param>
    static member AndItem collection item = 
        let lmc = new LayerMoveCollection()
        lmc.AddRange collection
        (lmc :> IList<LayerMove>).Add item
        lmc

    /// <summary> Adds a collection of LayerMoves to the given collection </summary>
    /// <param name="collection1">Defines the collection to be expanded</param>
    /// <param name="collection2">Defines the collection to be added</param>    
    static member AndCollection collection1 collection2 = 
        let lmc = new LayerMoveCollection()
        lmc.AddRange collection1
        lmc.AddRange collection2
        lmc

    interface IMove with
        /// <summary> Returns a connected strings of all LayerMove names </summary>
        member this.Name 
            with get() = 
                let moves = this.moves.Select(fun m -> (m :> IMove).Name).ToArray()
                System.String.Join(", ", moves)

        /// Returns true if MultipleLayers are allowed
        member __.MultipleLayers with get() = true

        /// Gets the reverse move
        member __.ReverseMove 
            with get() = 
                let reverseMove = LayerMoveCollection()
                let idk = __.Select(fun m -> LayerMove(m.Layer, not m.Direction, m.Twice))
                reverseMove.AddRange(idk)
                reverseMove :> IMove
        
        
        /// <summary> Transforms the layer move collection 
        /// <param name="rotationLayer">Transformation layer</param>
        /// <returns>The transformed layer move collection</returns>
        member __.Transform (rotationLayer: CubeFlag) = 
            let mutable newMoves = LayerMoveCollection()

            __.moves.ForEach(fun m -> 
                let newMove = new LayerMove(m.Layer, m.Direction, m.Twice)
                
                newMoves <- LayerMove.AndItem newMove <| downcast (newMove :> IMove).Transform rotationLayer
            )

            newMoves :> IMove
    
    /// <summary> Parses a notation string into a layer move collection </summary>
    /// <param name="notation">String to be parsed</param>
    /// <param name="moves">The resulting layer moves</param>
    /// <returns>True, if the string was successfully parsed into a layermove</returns>
    static member TryParse (notation: string, [<Out>] moves : byref<LayerMoveCollection>) = 
        let layer = notation.[0].ToString()
        moves <- new LayerMoveCollection()
        
        let ccwChars = [| '\''; 'i'|]
        let direction = ccwChars |> Array.filter(fun c -> notation.Contains c) |> Array.isEmpty
        let twice = notation.Contains("2")

        match layer with
            | "r" -> 
                let move1 = LayerMove(CubeFlag.RightSlice, direction, twice)
                let move2 = LayerMove(CubeFlag.MiddleSliceSides, direction, twice)
                moves <- LayerMove.AndItem move1 move2
                true
            | "l" -> 
                let move1 = LayerMove(CubeFlag.LeftSlice, direction, twice)
                let move2 = LayerMove(CubeFlag.MiddleSliceSides, not direction, twice)
                moves <- LayerMove.AndItem move1 move2
                true
            | "u" -> 
                let move1 = LayerMove(CubeFlag.TopLayer, direction, twice)
                let move2 = LayerMove(CubeFlag.MiddleLayer, direction, twice)
                moves <- LayerMove.AndItem move1 move2
                true
            | "d" -> 
                let move1 = LayerMove(CubeFlag.BottomLayer, direction, twice)
                let move2 = LayerMove(CubeFlag.MiddleLayer, not direction, twice)
                moves <- LayerMove.AndItem move1 move2
                true
            | "f" -> 
                let move1 = LayerMove(CubeFlag.FrontSlice, direction, twice)
                let move2 = LayerMove(CubeFlag.MiddleSlice, direction, twice)
                moves <- LayerMove.AndItem move1 move2
                true
            | "b" -> 
                let move1 = LayerMove(CubeFlag.BackSlice, direction, twice)
                let move2 = LayerMove(CubeFlag.MiddleSlice, not direction, twice)
                moves <- LayerMove.AndItem move1 move2
                true
            | "x" -> 
                let move1 = LayerMove(CubeFlag.RightSlice, direction, twice)
                let move2 = LayerMove(CubeFlag.MiddleSliceSides, direction, twice)
                let move3 = LayerMove(CubeFlag.LeftSlice, not direction, twice)
                moves <- LayerMoveCollection.AndItem (LayerMove.AndItem move1 move2) move3
                true
            | "y" -> 
                let move1 = LayerMove(CubeFlag.TopLayer, direction, twice)
                let move2 = LayerMove(CubeFlag.MiddleLayer, direction, twice)
                let move3 = LayerMove(CubeFlag.BottomLayer, not direction, twice)
                moves <- LayerMoveCollection.AndItem (LayerMove.AndItem move1 move2) move3
                true
            | "z" -> 
                let move1 = LayerMove(CubeFlag.FrontSlice, direction, twice)
                let move2 = LayerMove(CubeFlag.MiddleSlice, direction, twice)
                let move3 = LayerMove(CubeFlag.BackSlice, not direction, twice)
                moves <- LayerMoveCollection.AndItem (LayerMove.AndItem move1 move2) move3
                true
            | _ -> false

    /// <summary> True, if the item accomplishes the equality conditions </summary>
    /// <param name="obj">LayerMoveCollection to be compared</param>

    override this.Equals o = 
        // can be super pertty.
        if o :? LayerMoveCollection then
            let coll: LayerMoveCollection = downcast o
            coll.moves.SequenceEqual this.moves
        else
            false

    override this.GetHashCode() = hash this

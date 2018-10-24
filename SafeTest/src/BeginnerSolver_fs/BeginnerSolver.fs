namespace BeginnerSolver

open RubiksCubeLib.Solving

open System.Collections.Generic
open System
open RubiksCubeLib
open RubiksCubeLib.Solver
open System.Drawing
open RubiksCubeLib.RubiksCube

type BeginnerSolver() as this = 
    inherit CubeSolver()
    
    do
        let d = new Dictionary<_, _>()
        d.Add("Cross on bottom layer", Action this.SolveFirstCross)
        d.Add("Complete bottom layer", Action this.CompleteFirstLayer)
        d.Add("Complete middle layer", Action this.CompleteMiddleLayer)
        d.Add("Cross on top layer", Action this.SolveCrossTopLayer)
        d.Add("Complete top layer", Action this.CompleteLastLayer)
        base.SolutionSteps <- d

    member private this.SolveFirstCross(): unit = 

        // Step 1: Get the edges with target position on the bottom layer
        let bottomEdges = 
            this.Rubik.Cubes
                |> Seq.filter(fun c -> 
                    let isEdge = c.IsEdge
                    let targetIsBottom = (this.GetTargetFlags(c)).HasFlag(CubeFlag.BottomLayer)

                    isEdge && targetIsBottom
                )


        // Step 2: Rotate a correct orientated edge of the bottom layer to target position
        let mutable bottomEdgesInBottomLayerCorrectlyOriented = 
            bottomEdges 
                |> Seq.filter(fun bottomEdge ->
                    let isInBottomLayer = bottomEdge.Position.Flags = this.GetTargetFlags bottomEdge
                    let isOrientedCorrectly = (bottomEdge.Faces |> Seq.find(fun f -> f.Color = this.Rubik.BottomColor)).Position = FacePosition.Bottom

                    isInBottomLayer && isOrientedCorrectly
                )


        let anyEdgesAreSolvableWithDMoves =
            bottomEdges 
                |> Seq.filter(fun bE ->
                    let thingA = bE.Position.HasFlag CubeFlag.BottomLayer
                    let thingB = 
                        (bE.Faces |> Seq.find(fun f -> f.Color = this.Rubik.BottomColor)).Position = FacePosition.Bottom
                    thingA && thingB
                )
                |> Seq.length
                |> (>) 0

        if anyEdgesAreSolvableWithDMoves then
            while not <| ((bottomEdgesInBottomLayerCorrectlyOriented |> Seq.length) > 0) do
                // turn the bottom layer until at least one is 
                base.SolverMove(CubeFlag.BottomLayer, true)

                bottomEdgesInBottomLayerCorrectlyOriented <-
                    bottomEdges |> Seq.filter(fun bE ->
                        let thingA = bE.Position.Flags = this.GetTargetFlags(bE)
                        let thingB = (bE.Faces |> Seq.find(fun f -> f.Color = this.Rubik.BottomColor)).Position = FacePosition.Bottom
                        
                        thingA && thingB
                    )


        // Step 3: Solve incorrect edges of the bottom layer
        while (bottomEdgesInBottomLayerCorrectlyOriented |> Seq.length) < 4 do
            let unsolvedBottomEdges = 
                System.Linq.Enumerable.Except(bottomEdges, bottomEdgesInBottomLayerCorrectlyOriented)

            let e =
                match unsolvedBottomEdges |> Seq.tryFind(fun c -> c.Position.HasFlag CubeFlag.TopLayer) with
                | Some result -> result
                | None -> unsolvedBottomEdges |> Seq.head

            let secondColor = 
                e.Colors |> Seq.find(fun co -> 
                    co <> this.Rubik.BottomColor && co <> Color.Black
                )

            if e.Position.Flags <> this.GetTargetFlags(e) then
                // Rotate to top layer
                let layer: CubeFlag =
                    let position = 
                        (e.Faces |> Seq.find(fun f ->
                            let thingA = f.Color = this.Rubik.BottomColor || f.Color = secondColor
                            let thingB = f.Position <> FacePosition.Top && f.Position <> FacePosition.Bottom
                            
                            thingA && thingB
                        )).Position

                    CubeFlagService.FromFacePosition(position)

                let targetLayer: CubeFlag =
                    let position = 
                        let a = this.StandardCube.Cubes |> Seq.find(fun cu -> CollectionMethods.ScrambledEquals cu.Colors e.Colors)
                        let b = a.Faces
                        let c = b |> Seq.find(fun f -> f.Color = secondColor)
                        c.Position

                    CubeFlagService.FromFacePosition position

                if (e.Position.HasFlag(CubeFlag.MiddleLayer)) then
                    if (layer = targetLayer) then
                        while not <| e.Position.HasFlag CubeFlag.BottomLayer do
                            this.SolverMove(layer, true);
                 
                    else
                        this.SolverMove(layer, true)
                    
                        if (e.Position.HasFlag(CubeFlag.TopLayer)) then
                            this.SolverMove(CubeFlag.TopLayer, true)
                            this.SolverMove(layer, false)
                        else
                            for i = 0 to (2-1) do
                                this.SolverMove(layer, true)

                            this.SolverMove(CubeFlag.TopLayer, true)
                            this.SolverMove(layer, true)

                if (e.Position.HasFlag(CubeFlag.BottomLayer)) then
                    for i = 0 to (2-1) do 
                        this.SolverMove(layer, true);

                // Rotate over target position
                while not <| e.Position.HasFlag targetLayer do
                    this.SolverMove(CubeFlag.TopLayer, true)

                //Rotate to target position
                for i = 0 to (2-1) do
                    this.SolverMove(targetLayer, true);

            // Flip the incorrect orientated edges with the algorithm: F' D R' D'
            if (Solvability.GetOrientation this.Rubik e) <> Orientation.Correct then
                let frontSlice: CubeFlag = 
                    let position = (e.Faces |> Seq.find(fun f -> f.Color = this.Rubik.BottomColor)).Position
                    CubeFlagService.FromFacePosition position

                this.SolverMove(frontSlice, false)
                this.SolverMove(CubeFlag.BottomLayer, true)

                let rightSlice: CubeFlag = 
                    let position = (e.Faces |> Seq.find(fun f -> f.Color = secondColor)).Position
                    CubeFlagService.FromFacePosition position

                this.SolverMove(rightSlice, false)
                this.SolverMove(CubeFlag.BottomLayer, false)

            bottomEdgesInBottomLayerCorrectlyOriented <-
                bottomEdges
                    |> Seq.filter(fun bE ->
                        let thingA = bE.Position.Flags = this.GetTargetFlags bE
                        let thingB = (bE.Faces |> Seq.find(fun f -> f.Color = this.Rubik.BottomColor)).Position = FacePosition.Bottom

                        thingA && thingB
                    )
            ()
        ()
        
    member private this.CompleteFirstLayer(): unit = 
        // Step 1: Get the corners with target position on bottom layer
        let bottomCorners = 
            this.Rubik.Cubes |> Seq.filter(fun c -> c.IsCorner && (this.GetTargetFlags c).HasFlag CubeFlag.BottomLayer)

        let mutable solvedBottomCorners = 
            bottomCorners 
                |> Seq.filter(fun bC ->
                    let thingA = bC.Position.Flags = this.GetTargetFlags bC
                    let thingB = (bC.Faces |> Seq.find(fun f -> f.Color = this.Rubik.BottomColor)).Position = FacePosition.Bottom

                    thingA && thingB
                )

        // Step 2: Solve incorrect edges
        while (solvedBottomCorners |> Seq.length) < 4 do
            let unsolvedBottomCorners = 
                System.Linq.Enumerable.Except(bottomCorners, solvedBottomCorners)
            
            let c: Cube = 
                match unsolvedBottomCorners |> Seq.tryFind(fun bC -> bC.Position.HasFlag CubeFlag.TopLayer) with
                | Some result -> result
                | None -> unsolvedBottomCorners |> Seq.head

            if c.Position.Flags <> this.GetTargetFlags c then
                // Rotate to top layer
                if c.Position.HasFlag CubeFlag.BottomLayer then
                    let mutable leftFace: Face = c.Faces |> Seq.find(fun f -> f.Position <> FacePosition.Bottom && f.Color <> Color.Black)
                    let mutable leftSlice: CubeFlag = CubeFlagService.FromFacePosition(leftFace.Position)

                    this.SolverMove(leftSlice, false);

                    if c.Position.HasFlag CubeFlag.BottomLayer then
                        this.SolverMove(leftSlice, true)

                        leftFace <-
                            c.Faces |> Seq.find(fun f ->
                                let isNotInBottom = f.Position <> FacePosition.Bottom
                                let isNotLeftColor = f.Color <> leftFace.Color
                                let isNotBlack = f.Color <> Color.Black

                                isNotInBottom && isNotLeftColor && isNotBlack
                            )

                        leftSlice <- CubeFlagService.FromFacePosition(leftFace.Position);

                        this.SolverMove(leftSlice, false);

                    this.SolverAlgorithm("U' {0} U", CubeFlagService.ToNotationString(leftSlice));

                // Rotate over target position
                let targetPos: CubeFlag = CubeFlagService.ExceptFlag (this.GetTargetFlags(c)) CubeFlag.BottomLayer

                while not <| c.Position.HasFlag targetPos do
                    this.SolverMove(CubeFlag.TopLayer, true);

            // Rotate to target position with the algorithm: Li Ui L U
            let mutable leftFac: Face =
                c.Faces |> Seq.find(fun f -> 
                    let notInTop = f.Position <> FacePosition.Top
                    let notInBottom = f.Position <> FacePosition.Bottom
                    let notBlack = f.Color <> Color.Black

                    notInTop && notInBottom && notBlack
                )

            let mutable leftSlic: CubeFlag = CubeFlagService.FromFacePosition leftFac.Position

            this.SolverMove(leftSlic, false);

            if not <| c.Position.HasFlag CubeFlag.TopLayer then
                this.SolverMove(leftSlic, true);

                leftFac <-
                    c.Faces |> Seq.find(fun f ->
                        let isNotInTop = f.Position <> FacePosition.Top
                        let isNotInBottom = f.Position <> FacePosition.Bottom
                        let isNotLeftPiece = f.Color <> leftFac.Color
                        let isNotBlack = f.Color <> Color.Black

                        isNotInTop && isNotInBottom && isNotLeftPiece && isNotBlack
                    )

                leftSlic <- CubeFlagService.FromFacePosition leftFac.Position
            else 
                this.SolverMove(leftSlic, true)

            while (c.Faces |> Seq.find(fun f -> f.Color = this.Rubik.BottomColor)).Position <> FacePosition.Bottom do
                if (c.Faces |> Seq.find(fun f -> f.Color = this.Rubik.BottomColor)).Position = FacePosition.Top then
                    this.SolverAlgorithm("{0}' U U {0} U", CubeFlagService.ToNotationString(leftSlic));
                else
                    let frontFac: Face =
                        c.Faces |> Seq.find(fun f ->
                            let isNotInTop = f.Position <> FacePosition.Top
                            let isNotInBottom = f.Position <> FacePosition.Bottom
                            let isNotBlack = f.Color <> Color.Black
                            let otherThing = f.Position <> CubeFlagService.ToFacePosition leftSlic

                            isNotInTop && isNotInBottom && isNotBlack && otherThing
                        )

                    let thingA = (c.Faces |> Seq.find(fun f -> f.Color = this.Rubik.BottomColor)).Position = frontFac.Position
                    let thingB = not <| c.Position.HasFlag CubeFlag.BottomLayer

                    if thingA && thingB then
                        this.SolverAlgorithm("U' {0}' U {0}", CubeFlagService.ToNotationString(leftSlic));
                    else
                        this.SolverAlgorithm("{0}' U' {0} U", CubeFlagService.ToNotationString(leftSlic));

            solvedBottomCorners <-
                bottomCorners |> Seq.filter(fun bC ->
                    let thingA = bC.Position.Flags = this.GetTargetFlags(bC)
                    let thingB = (bC.Faces |> Seq.find(fun f -> f.Color = this.Rubik.BottomColor)).Position = FacePosition.Bottom

                    thingA && thingB
                )
            ()

        ()

    member private this.CompleteMiddleLayer(): unit = 
        // Step 1: Get the egdes of the middle layer
        let middleEdges = 
            this.Rubik.Cubes
                |> Seq.filter(fun c -> c.IsEdge)
                |> Seq.filter(fun c -> (this.GetTargetFlags c).HasFlag CubeFlag.MiddleLayer)
                |> Seq.toList

        let coloredFaces: List<Face>= new List<Face>()

        this.Rubik.Cubes
            |> Seq.filter(fun cu -> cu.IsCenter)
            |> Seq.toList
            |> List.iter(fun cu -> 
                coloredFaces.Add(cu.Faces |> Seq.find(fun f -> f.Color <> Color.Black))
            )

        let mutable solvedMiddleEdges: seq<Cube> =
            middleEdges
                |> Seq.filter(fun mE -> 
                    let thingA = mE.Position.Flags = this.GetTargetFlags mE

                    let thingB = 
                        mE.Faces 
                            |> Seq.filter(fun f -> 
                                coloredFaces 
                                    |> Seq.filter(fun cf -> cf.Color = f.Color && cf.Position = f.Position)
                                    |> Seq.length
                                    |> (=) 1
                            )
                            |> Seq.length
                            |> (=) 2

                    thingA && thingB
                )

        // Step 2: solve incorrect middle edges 
        while (solvedMiddleEdges |> Seq.length) < 4 do
            let unsolvedMiddleEdges: seq<Cube> = 
                System.Linq.Enumerable.Except(middleEdges, solvedMiddleEdges)
            
            let c: Cube =
                match unsolvedMiddleEdges |> Seq.tryFind(fun cu -> not <| cu.Position.HasFlag CubeFlag.MiddleLayer) with
                | Some result -> result
                | None -> unsolvedMiddleEdges |> Seq.head

            // Rotate to top layer
            if not <| c.Position.HasFlag CubeFlag.TopLayer then
                let frontFace: Face = c.Faces |> Seq.find(fun f -> f.Color <> Color.Black)
                let frontSlice: CubeFlag  = CubeFlagService.FromFacePosition frontFace.Position
                let face: Face  = c.Faces |> Seq.find(fun f -> f.Color <> Color.Black && f.Color <> frontFace.Color)
                let slice: CubeFlag  = CubeFlagService.FromFacePosition face.Position

                let testPassed = 
                    let scenario = new TestScenario(this.Rubik, new LayerMove(slice, true, false))
                    scenario.TestCubePosition c CubeFlag.TopLayer

                if testPassed then
                    // Algorithm to the right: U R U' R' U' F' U F
                    this.SolverAlgorithm(
                        "U {0} U' {0}' U' {1}' U {1}",
                        CubeFlagService.ToNotationString(slice),
                        CubeFlagService.ToNotationString(frontSlice)
                    );
                else
                    // Algorithm to the left: U' L' U L U F U' F'
                    this.SolverAlgorithm(
                        "U' {0}' U {0} U {1} U' {1}'",
                        CubeFlagService.ToNotationString(slice),
                        CubeFlagService.ToNotationString(frontSlice)
                    );

            // Rotate to start position for the algorithm
            let mutable centers = 
                this.Rubik.Cubes
                    |> Seq.filter(fun m -> m.IsCenter)
                    |> Seq.filter(fun m -> 
                        let thing2 =
                            let left = m.Colors |> Seq.find(fun co -> co <> Color.Black)
                            let right = 
                                (c.Faces |> Seq.find(fun f -> f.Color <> Color.Black && f.Position <> FacePosition.Top)).Color;

                            left = right

                        let thing3 =
                            let left = m.Position.Flags &&& (~~~ CubeFlag.MiddleLayer)
                            let right = c.Position.Flags &&& (~~~ CubeFlag.TopLayer)
                            left = right

                        thing2 && thing3
                    )
                    |> Seq.toList

            while (centers |> List.length) = 0 do
                this.SolverMove(CubeFlag.TopLayer, true)

                centers <-
                    this.Rubik.Cubes
                    |> Seq.filter(fun m -> m.IsCenter)
                    |> Seq.filter(fun m -> 
                        let thing2 =
                            let left = m.Colors |> Seq.find(fun co -> co <> Color.Black)
                            let right = 
                                (c.Faces |> Seq.find(fun f -> f.Color <> Color.Black && f.Position <> FacePosition.Top)).Color;

                            left = right

                        let thing3 =
                            let left = m.Position.Flags &&& (~~~ CubeFlag.MiddleLayer)
                            let right = c.Position.Flags &&& (~~~ CubeFlag.TopLayer)
                            left = right

                        thing2 && thing3
                    )
                    |> Seq.toList

            // Rotate to target position
            let frontFac: Face = c.Faces |> Seq.find(fun f -> f.Color <> Color.Black && f.Position <> FacePosition.Top)
            let frontSlic: CubeFlag = CubeFlagService.FromFacePosition frontFac.Position

            let slic: CubeFlag = CubeFlagService.FirstNotInvalidFlag (this.GetTargetFlags(c)) (CubeFlag.MiddleLayer ||| frontSlic)

            let scenarioPassed = 
                let scenario = new TestScenario(this.Rubik, new LayerMove(CubeFlag.TopLayer, true, false))
                scenario.TestCubePosition c slic


            if not scenarioPassed then
                // Algorithm to the right: U R Ui Ri Ui Fi U F
                this.SolverAlgorithm(
                    "U {0} U' {0}' U' {1}' U {1}",
                    CubeFlagService.ToNotationString(slic),
                    CubeFlagService.ToNotationString(frontSlic)
                )
            else
                // Algorithm to the left: Ui Li U L U F Ui Fi
                this.SolverAlgorithm(
                    "U' {0}' U {0} U {1} U' {1}'",
                    CubeFlagService.ToNotationString(slic),
                    CubeFlagService.ToNotationString(frontSlic)
                )

            solvedMiddleEdges <-
                middleEdges |> Seq.filter(fun mE ->
                    mE.Faces
                        |> Seq.filter(fun f ->
                            coloredFaces |> Seq.filter(fun cf -> cf.Color = f.Color && cf.Position = f.Position)  |> Seq.length |> (=) 1
                        ) 
                        |> Seq.length |> (=) 2
                );

    member private this.SolveCrossTopLayer(): unit = 
        // Step 1: Get edges with the color of the top face
        let topEdges: seq<Cube> = 
            this.Rubik.Cubes
                |> Seq.filter(fun c -> c.IsEdge)
                |> Seq.filter(fun c -> c.Position.HasFlag CubeFlag.TopLayer)

        //// Step 2: Check if the cube is insoluble
        //if (topEdges.Count(tE => tE.Faces.First(f => f.Position == FacePosition.Top).Color == Rubik.TopColor) % 2 != 0)
        //    return;

        let mutable correctEdges = 
            topEdges
                |> Seq.filter(fun c -> 
                    (c.Faces |> Seq.find(fun f -> f.Position = FacePosition.Top)).Color = this.Rubik.TopColor
                )

        let solveTopCrossAlgorithmI = new Algorithm("F R U R' U' F'");
        let solveTopCrossAlgorithmII = new Algorithm("F S R U R' U' F' S'");

        // Step 3: Solve the cross on the top layer
        if (this.Rubik.CountEdgesWithCorrectOrientation()) = 0 then
            this.SolverAlgorithm(solveTopCrossAlgorithmI)
            
            correctEdges <-
                topEdges
                    |> Seq.filter(fun c -> 
                        (c.Faces |> Seq.find(fun f -> f.Position = FacePosition.Top)).Color = this.Rubik.TopColor
                    )

        if (this.Rubik.CountEdgesWithCorrectOrientation()) = 2 then
            let firstCorrect = correctEdges |> Seq.head
            let secondCorrect = correctEdges |> Seq.find(fun f -> f <> firstCorrect)

            let mutable opposite = false;
            let mutable STOP = false

            for flag in firstCorrect.Position.GetFlags() do
                let pos: CubeFlag = CubeFlagService.GetOppositeFlag(flag);

                if not STOP && secondCorrect.Position.HasFlag pos && pos <> CubeFlag.None then
                    opposite <- true
                    STOP <- true

            if opposite then
                while (correctEdges |> Seq.filter(fun c -> c.Position.HasFlag CubeFlag.RightSlice) |> Seq.length) <> 1 do
                    this.SolverMove(CubeFlag.TopLayer, true)

                this.SolverAlgorithm(solveTopCrossAlgorithmI)
            else
                while (correctEdges |> Seq.filter(fun c -> c.Position.HasFlag CubeFlag.RightSlice || c.Position.HasFlag CubeFlag.FrontSlice) |> Seq.length) <> 2 do
                    this.SolverMove(CubeFlag.TopLayer, true)

                this.SolverAlgorithm(solveTopCrossAlgorithmII)

        // Step 4: Move the edges of the cross to their target positions
        while (topEdges |> Seq.filter(fun c -> c.Position.Flags = this.GetTargetFlags c) |> Seq.length) < 4 do
            let correctEdges2: seq<Cube> = topEdges |> Seq.filter(fun c -> c.Position.Flags = this.GetTargetFlags c)
            while (correctEdges2 |> Seq.length) < 2 do
                this.SolverMove(CubeFlag.TopLayer, true);

            let mutable rightSlice: CubeFlag = 
                let position = 
                    let faces = (correctEdges2 |> Seq.head).Faces
                    (faces |> Seq.filter(fun f -> f.Position <> FacePosition.Top && f.Color <> Color.Black) |> Seq.head).Position

                CubeFlagService.FromFacePosition position
            
            this.SolverMove(CubeFlag.TopLayer, false)

            if (correctEdges2 |> Seq.filter(fun c -> c.Position.HasFlag rightSlice) |> Seq.length) = 0 then
                this.SolverMove(CubeFlag.TopLayer, true)
            else
                this.SolverMove(CubeFlag.TopLayer, true);

                rightSlice <-
                    let position = 
                        let faces = (correctEdges2 |> Seq.head).Faces
                        (faces |> Seq.filter(fun f -> f.Position <> FacePosition.Top && f.Color <> Color.Black) |> Seq.head).Position

                    CubeFlagService.FromFacePosition position

            // Algorithm: R U R' U R U U R'
            this.SolverAlgorithm("{0} U {0}' U {0} U U {0}'", CubeFlagService.ToNotationString rightSlice)

            while (correctEdges2 |> Seq.length) < 2 do
                this.SolverMove(CubeFlag.TopLayer, true);

    member private this.CompleteLastLayer(): unit = 
        // Step 1: Get edges with the color of the top face
        let topCorners: seq<Cube> =
            this.Rubik.Cubes
                |> Seq.filter(fun c -> c.IsCorner)
                |> Seq.filter(fun c -> c.Position.HasFlag CubeFlag.TopLayer)

        // Step 2: Bring corners to their target position
        while (topCorners |> Seq.filter(fun c -> c.Position.Flags = this.GetTargetFlags c) |> Seq.length) < 4 do
            let correctCorners = topCorners |> Seq.filter(fun c -> c.Position.Flags = this.GetTargetFlags c)

            let rightSlice: CubeFlag = 
                if (correctCorners |> Seq.length) <> 0 then
                    let firstCube: Cube = correctCorners |> Seq.head
                    let rightFace = firstCube.Faces |> Seq.find(fun f -> f.Color <> Color.Black && f.Position <> FacePosition.Top)

                    let mutable result = CubeFlagService.FromFacePosition(rightFace.Position);

                    let scenarioPassed = 
                        let scenario = new TestScenario(this.Rubik, new LayerMove(result, true, false))
                        scenario.TestCubePosition firstCube CubeFlag.TopLayer

                    if not scenarioPassed then
                        result <-
                            CubeFlagService.FromFacePosition(
                                (firstCube.Faces
                                    |> Seq.find(fun f -> 
                                        f.Color <> rightFace.Color && f.Color <> Color.Black && f.Position <> FacePosition.Top
                                    )).Position
                            )

                    result
                else
                    CubeFlag.RightSlice;

            this.SolverAlgorithm(
                "U {0} U' {1}' U {0}' U' {1}",
                CubeFlagService.ToNotationString(rightSlice),
                CubeFlagService.ToNotationString(CubeFlagService.GetOppositeFlag(rightSlice))
            );

        // Step 3: Orientation of the corners on the top layer
        let rightFac: Face = 
            (topCorners |> Seq.head).Faces
                |> Seq.find(fun f -> f.Color <> Color.Black && f.Position <> FacePosition.Top)

        let scenario = new TestScenario(this.Rubik, new LayerMove(CubeFlagService.FromFacePosition(rightFac.Position), true, false))
        let scenarioPassed = scenario.TestCubePosition (topCorners |> Seq.head) CubeFlag.TopLayer

        let positionForRightSlic =
            if not scenarioPassed then
                rightFac.Position
            else
                ((topCorners |> Seq.head).Faces
                    |> Seq.find(fun f -> f.Color <> rightFac.Color && f.Color <> Color.Black && f.Position <> FacePosition.Top)
                ).Position

        let rightSlic: CubeFlag = CubeFlagService.FromFacePosition positionForRightSlic

        for c in topCorners do
            while not <| c.Position.HasFlag rightSlic do
                this.SolverMove(CubeFlag.TopLayer, true)

            let scenarioPassed = 
                let scenario = new TestScenario(this.Rubik, new LayerMove(rightSlic, true, false))
                scenario.TestCubePosition c CubeFlag.TopLayer

            if not scenarioPassed then
                this.SolverMove(CubeFlag.TopLayer, true)

            // Algorithm: R' D' R D
            while (c.Faces |> Seq.find(fun f -> f.Position = FacePosition.Top)).Color <> this.Rubik.TopColor do
                this.SolverAlgorithm("{0}' D' {0} D", CubeFlagService.ToNotationString rightSlic)

        while (topCorners |> Seq.filter(fun tC -> tC.Position.Flags = this.GetTargetFlags tC) |> Seq.length) <> 4 do
            this.SolverMove(CubeFlag.TopLayer, true)

    override this.Name with get() = "Beginner_fs"
